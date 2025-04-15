using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Mirror;
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

    [Description("Whether or not the coin will spawn around the map on round start")]
    public bool SpawnEnabled { get; set; } = true;

    // TODO: Need to find a way to make this variable unique per item and not 'static'
    [Description("How many times the coin can be used before the string breaks.")]
    public int Uses { get; set; } = 4;

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
}