using System;
using System.IO;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace VendingMachine;

public class MainPlugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "SCP-294";

    public override string Prefix { get; } = "VendingMachine";

    public override Version Version { get; } = new(1, 0, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 5, 0);

    public static MainPlugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public Scp294 VendingMachine { get; private set; }

    public override PluginPriority Priority { get; } = PluginPriority.Last;

    public override void OnEnabled()
    {
        if (!File.Exists(Path.Combine(Paths.Plugins, "MapEditorReborn.dll")))
        {
            Log.Error("MapEditorReborn is missing!, aborting plugin startup.");
            return;
        }

        Singleton = this;

        Exiled.Events.Handlers.Player.SearchingPickup += VendingMachine.OnInteracted;
        Exiled.Events.Handlers.Player.VoiceChatting += VendingMachine.OnVoiceChatting;
        Exiled.Events.Handlers.Player.Dying += VendingMachine.OnDying;
        Exiled.Events.Handlers.Server.RoundStarted += VendingMachine.OnRoundStart;
        Exiled.Events.Handlers.Server.EndingRound += VendingMachine.OnEndingRound;

        // Internal
        //Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        // Internal
        //Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

        Exiled.Events.Handlers.Player.SearchingPickup -= VendingMachine.OnInteracted;
        Exiled.Events.Handlers.Player.VoiceChatting -= VendingMachine.OnVoiceChatting;
        Exiled.Events.Handlers.Player.Dying -= VendingMachine.OnDying;
        Exiled.Events.Handlers.Server.RoundStarted -= VendingMachine.OnRoundStart;
        Exiled.Events.Handlers.Server.EndingRound -= VendingMachine.OnEndingRound;

        Singleton = null;
        VendingMachine = null;
        base.OnDisabled();
    }

    //private static void OnWaitingForPlayers() => StartupChecks.UnRegisterIncompatibilities();
}