using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using UnityEngine;
using VendingMachine.ConfigObjects;

namespace VendingMachine;

public sealed class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;

    public bool Debug { get; set; } = false;

    [Description("The chances for different drinks to be dispensed")]
    public List<DrinkChance> DrinkChances { get; set; } = new();

    // TODO: This should default to any of the basic 'rooms' (not 'hallways') in Entrance Zone
    [Description("The rooms and positions where the vending machine can spawn")]
    public Dictionary<RoomType, Vector3> SpawnPoints { get; set; } = new();
}