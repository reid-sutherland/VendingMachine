using System.ComponentModel;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

public abstract class CustomDrink : CustomItem
{
    // This should ONLY be defined in the config.
    [Description("The chance that the drink is dispensed.")]
    public int Chance { get; set; }

    // This is defined here so that the config can't try to define a spawn location, since drinks come out of the vending machine.
    [YamlIgnore]
    public override SpawnProperties SpawnProperties { get; set; } = new();
}