using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using InventorySystem.Items.MicroHID.Modules;
using UnityEngine;
using VendingMachine.Drinks;

namespace VendingMachine;

public sealed class Config : IConfig
{
    [Description("Whether the vending machine (SCP-294) should be enabled and spawn in the map.")]
    public bool IsEnabled { get; set; } = true;

    public bool Debug { get; set; } = false;

    [Description("Whether or not probabilities should be additive (50 + 50 = 100) or not (50 + 50 = 2 seperate 50% chances)")]
    public bool AdditiveProbabilities { get; set; } = true;

    // TODO: This should default to any of the basic 'rooms' (not 'hallways') in Entrance Zone
    [Description("The rooms and positions where the vending machine can spawn.")]
    public Dictionary<RoomType, Vector3> SpawnPoints { get; set; } = new();

    // It's too difficult to make a list of drinks since CustomDrink can't be instantiated
    // So define them individually here

    [Description("The remaining fields are drinks to be dispensed from SCP-294")]
    public GobbyPop GobbyPop { get; set; } = new();
}