global using Log = CommonUtils.Core.Logger;

using CommonUtils.Core;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VendingMachine.Drinks;
using Random = System.Random;

namespace VendingMachine;

public class MainPlugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "Vending Machine";

    public override string Prefix { get; } = "VendingMachine";

    public override Version Version { get; } = new(1, 1, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 10, 1);

    public override PluginPriority Priority { get; } = PluginPriority.Low;

    public static Random Random { get; private set; }

    public static MainPlugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public static Scp294 Scp294 { get; private set; }

    public static List<CustomDrink> CustomDrinkList { get; set; }


    public override void OnEnabled()
    {
        Singleton = this;
        Scp294 = new();
        Random = new();

        if (Configs.Debug)
        {
            Log.EnableDebug();
        }

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

        // Create a list of drinks from the config for rolling chances
        CustomDrinkList = new();
        foreach (PropertyInfo pInfo in Configs.GetType().GetProperties())
        {
            if (typeof(CustomDrink).IsAssignableFrom(pInfo.PropertyType))
            {
                CustomDrink drink = pInfo.GetValue(Configs) as CustomDrink;
                if (drink is not null)
                {
                    CustomDrinkList.Add(drink);
                    Log.Debug($"-- added {drink.Name} to list of chances with chance: {drink.Chance}", print: Configs.RollDebug);
                }
            }
        }
        Log.Debug($"Total chance for {CustomDrinkList.Count} CustomDrinks: {CustomDrinkList.Sum(x => x.Chance)}", print: Configs.RollDebug);

        // Load music for vending machine
        Log.Info($"Loading SCP-294 audio clips from directory: {Scp294.AudioPath}");
        AudioHelper.LoadAudioClip(Scp294.AudioPath, Scp294.DispenseEffectAudio, log: Configs.AudioDebug);
        AudioHelper.LoadAudioClips(Scp294.AudioPath, Scp294.AmbientAudio, log: Configs.AudioDebug);

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        base.OnDisabled();

        Exiled.Events.Handlers.Server.RoundStarted -= Scp294.OnRoundStarted;
        Exiled.Events.Handlers.Server.RoundEnded -= Scp294.OnRoundEnded;
        Exiled.Events.Handlers.Server.RestartingRound -= Scp294.OnRestartingRound;

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

        Singleton = null;
        Scp294 = null;
    }
}