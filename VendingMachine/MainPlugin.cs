using AdvancedMERTools;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using MEC;
using System;
using System.IO;
using UnityEngine;
using UserSettings.ServerSpecific;
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

    public static Random Random { get; private set; }

    public const int EVALUE = 101;

    public override void OnEnabled()
    {
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
        // Note: delay here is recommended so that all other plugins and components can load first
        Timing.CallDelayed(5f, () =>
        {
            Log.Debug("Registering custom items...");
            try
            {
                CustomItem.RegisterItems(overrideClass: Configs);
                Log.Info("All custom items registered successfully");
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

        Log.Debug("Un-registering custom items...");
        try
        {
            CustomItem.UnregisterItems();
            Log.Info("All custom items un-registered successfully");
        }
        catch (Exception ex)
        {
            Log.Error("Some custom items failed to un-register");
            Log.Debug(ex);
        }

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
                    if (Configs.AmertDebug)
                    {
                        Log.Debug($"-- Player {Player.Get(sender).Nickname} interacted with object: {hit.collider.gameObject.name} - interactable: {interactable.gameObject.name} - distance: {hit.distance}");
                    }
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
                                if (Config.AmertDebug)
                                {
                                    Log.Debug($"Unknown interactable: {interactable.gameObject.name}");
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}