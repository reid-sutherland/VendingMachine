﻿using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using YamlDotNet.Serialization;

namespace VendingMachine;

[CustomItem(ItemType.Coin)]
public class CoinWithAString : CustomItem
{
    [YamlIgnore]
    public override uint Id { get; set; } = 50;

    [YamlIgnore]
    public override string Name { get; set; } = "Coin with a string";

    [YamlIgnore]
    public override string Description { get; set; } = "This coin has a string tied around it...";

    [YamlIgnore]
    public override float Weight { get; set; } = 0.005f;    // Base Coin weighs 0.0025

    [YamlIgnore]
    public override Vector3 Scale { get; set; } = new(2.0f, 2.0f, 2.0f);

    [Description("How many times the coin can be used before the string breaks.")]
    public int StartingUses { get; set; } = 4;

    [Description("The list of spawn locations and chances for each one.")]
    public override SpawnProperties SpawnProperties { get; set; } = new()
    {
        Limit = 3,
        LockerSpawnPoints = new()
        {
            new LockerSpawnPoint()
            {
                Zone = ZoneType.Entrance,
                UseChamber = true,
                Type = LockerType.Misc,
                Chance = 45,
            },
            new LockerSpawnPoint()
            {
                Zone = ZoneType.LightContainment,
                UseChamber = true,
                Type = LockerType.Misc,
                Chance = 5,
            },
        },
        DynamicSpawnPoints = new()
        {
            new()
            {
                Chance = 33,
                Location = SpawnLocationType.InsideIntercom,
            },
            new()
            {
                Chance = 25,
                Location = SpawnLocationType.InsideGateA,
            },
            new()
            {
                Chance = 25,
                Location = SpawnLocationType.InsideGateB,
            },
            new()
            {
                Chance = 50,
                Location = SpawnLocationType.Inside939Cryo,
            },
        },
    };

    [YamlIgnore]
    public Dictionary<int, int> ItemUses { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;

        base.UnsubscribeEvents();
    }

    public void OnRoundStarted()
    {
        Log.Debug($"Round started: spawning CoinWithAString");
        ItemUses = new();
        foreach (int serial in TrackedSerials)
        {
            ItemUses.Add(serial, StartingUses);
        }
        Log.Info($"Spawned {TrackedSerials.Count} CoinWithAString's around the map");
    }

    // Use the item's serial to update the uses tracker
    // Returns whether the coin should be removed
    public bool Use(Player player)
    {
        bool removeCoin;
        if (ItemUses.ContainsKey(player.CurrentItem.Serial))
        {
            if (ItemUses[player.CurrentItem.Serial] > 1)
            {
                removeCoin = false;
                ItemUses[player.CurrentItem.Serial] -= 1;
                Log.Debug($"-- uses left: {ItemUses[player.CurrentItem.Serial]}");
            }
            else
            {
                removeCoin = true;
                player.ShowHint($"Uh oh... the string broke!", 5.0f);
                Log.Debug($"-- no uses left: removing");
            }
        }
        else
        {
            // this shouldn't happen but if it does... they probably shouldn't have the coin lol
            removeCoin = true;
            Log.Error("!!! Player was holding a rogue CWAS");
        }
        return removeCoin;
    }
}