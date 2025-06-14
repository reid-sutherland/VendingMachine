using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using System;
using VendingMachine.Utils;
using Random = System.Random;

namespace VendingMachine;

public class MainPlugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "Vending Machine";

    public override string Prefix { get; } = "VendingMachine";

    public override Version Version { get; } = new(1, 1, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 6, 1);

    public override PluginPriority Priority { get; } = PluginPriority.Low;

    public static MainPlugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public Scp294 Scp294 { get; private set; }

    public static Random Random { get; private set; }

    public override void OnEnabled()
    {
        Singleton = this;
        Scp294 = new();
        Random = new();

        Exiled.Events.Handlers.Server.RoundStarted += Scp294.OnRoundStarted;
        Exiled.Events.Handlers.Server.RoundEnded += Scp294.OnRoundEnded;
        Exiled.Events.Handlers.Server.RestartingRound += Scp294.OnRestartingRound;

        // Register custom items here
        // Note: delay here is recommended so that all other plugins and components can load first
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

        // Load music for vending machine
        Log.Info($"Loading SCP-294 audio clips from directory: {Scp294.AudioPath}");
        AudioHelper.LoadAudioClip(Scp294.AudioPath, Scp294.AudioDispenseEffect);
        AudioHelper.LoadAudioClips(Scp294.AudioPath, Scp294.AudioAmbient);

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

        Exiled.Events.Handlers.Server.RoundStarted -= Scp294.OnRoundStarted;
        Exiled.Events.Handlers.Server.RoundEnded -= Scp294.OnRoundEnded;
        Exiled.Events.Handlers.Server.RestartingRound -= Scp294.OnRestartingRound;

        Singleton = null;
        Scp294 = null;
    }
}