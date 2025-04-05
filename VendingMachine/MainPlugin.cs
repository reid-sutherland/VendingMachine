using System;
using System.IO;
using AdvancedMERTools;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using MEC;
using UnityEngine;
using UnityEngine.Rendering;
using UserSettings.ServerSpecific;
using VendingMachine.Drinks;
using Random = System.Random;

namespace VendingMachine;

public class MainPlugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "Vending Machine";

    public override string Prefix { get; } = "VendingMachine";

    public override Version Version { get; } = new(1, 0, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 5, 0);

    public override PluginPriority Priority { get; } = PluginPriority.Last;

    public static MainPlugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public Scp294 Scp294 { get; private set; }

    public readonly string AudioPath = Path.Combine(Paths.Plugins, "audio\\SCP294\\");

    public static Random Random { get; private set; }

    public const int EVALUE = 101;

    public override void OnEnabled()
    {
        Log.Error(AudioPath);
        if (!File.Exists(Path.Combine(Paths.Plugins, "MapEditorReborn.dll")))
        {
            Log.Error("MapEditorReborn is missing!, aborting plugin startup.");
            return;
        }

        Singleton = this;
        Scp294 = new();
        Random = new();

        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSSInput;
        Exiled.Events.Handlers.Server.RoundStarted += Scp294.OnRoundStarted;
        Exiled.Events.Handlers.Server.RoundEnded += Scp294.OnRoundEnded;

        // Register custom items here
        Timing.CallDelayed(5f, () =>
        {
            Log.Debug("Registering custom items...");
            try
            {
                CustomItem.RegisterItems(overrideClass: Configs);
            }
            catch (Exception ex)
            {
                Log.Error("Some custom items failed to register");
                Log.Debug(ex);
            }
        });

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        base.OnDisabled();

        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSSInput;
        Exiled.Events.Handlers.Server.RoundStarted -= Scp294.OnRoundStarted;
        Exiled.Events.Handlers.Server.RoundEnded -= Scp294.OnRoundEnded;

        Singleton = null;
        Scp294 = null;
    }

    public void OnSSInput(ReferenceHub sender, ServerSpecificSettingBase setting)
    {
        if (setting.OriginalDefinition is SSKeybindSetting sSKeybind && (setting as SSKeybindSetting).SyncIsPressed)
        {
            KeyCode key = sSKeybind.SuggestedKey;
            if ((int)key == EVALUE && Physics.Raycast(sender.PlayerCameraReference.position, sender.PlayerCameraReference.forward, out RaycastHit hit, 1000f, 1))
            {
                foreach (InteractableObject interactable in hit.collider.GetComponentsInParent<InteractableObject>())
                {
                    Log.Debug($"-- Player {Player.Get(sender).Nickname} interacted with object: {hit.collider.gameObject.name} - interactable: {interactable.gameObject.name} - distance: {hit.distance}");
                    if (hit.distance <= interactable.Base.InteractionMaxRange)
                    {
                        Player player = Player.Get(sender);
                        switch (interactable.gameObject.name)
                        {
                            case Scp294.ControlPanelName:
                                Scp294.OnControlPanelInteracted(player);
                                break;
                            case Scp294.DrawerName:
                                Scp294.OnDrawerInteracted(player);
                                break;
                            default:
                                Log.Debug($"Unknown interactable: {interactable.gameObject.name}");
                                break;
                        }
                    }
                }
            }
        }
    }

    public void PlaySound()
    {
        //public string AudioName;
        //[Header("0: Loop")]
        //public int PlayCount;
        //public bool IsSpatial;
        //public float MaxDistance;
        //public float MinDistance;
        //public float Volume;
        //public SVector3 LocalPlayPosition;
        //public AudioPlayer AP;
        //bool loaded;

        //MEC.Timing.CallDelayed(1.0f, () =>
        //{
        //    loaded = false;
        //    if (!loaded)
        //    {
        //        if (!Directory.Exists(Configs.AudioFolderPath))
        //        {
        //            ServerConsole.AddLog("Cannot find Audio Folder Directory!", ConsoleColor.Red);
        //            return;
        //        }
        //        if (!AudioClipStorage.AudioClips.ContainsKey(AudioName))
        //            AudioClipStorage.LoadClip(Path.Combine(AdvancedMERTools.Singleton.Config.AudioFolderPath, AudioName), AudioName);
        //        loaded = true;
        //    }

        //    if (AP == null)
        //    {
        //        AP = AudioPlayer.Create($"AudioHandler-{args.transform.GetHashCode()}-{GetHashCode()}");
        //        Speaker speaker = AP.AddSpeaker("Primary", args.transform.TransformPoint(LocalPlayPosition), Volume, IsSpatial, MinDistance, MaxDistance);
        //        AP.transform.parent = speaker.transform.parent = args.transform;
        //        AP.transform.localPosition = speaker.transform.localPosition = LocalPlayPosition;
        //        //ServerConsole.AddLog(speaker.transform.position.ToPreciseString());
        //    }
        //    if (PlayCount == 0)
        //        AP.AddClip(AudioName, Volume, true, false);
        //    for (int i = 0; i < PlayCount; i++)
        //        AP.AddClip(AudioName, Volume, false, false);
        //});
    }

    //private static void OnWaitingForPlayers() => StartupChecks.UnRegisterIncompatibilities();
}