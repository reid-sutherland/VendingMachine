using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

public abstract class CustomDrink : CustomItem
{
    // This is defined here so that the config can't try to define a spawn location, since drinks come out of the vending machine.
    [YamlIgnore]
    public override SpawnProperties SpawnProperties { get; set; } = new();

    // This should ONLY be defined in the config.
    [Description("The chance that the drink is dispensed.")]
    public int Chance { get; set; }

    // These must be used by all drinks so that things like SCP-500 uses can be tracked accordingly
    [YamlIgnore]
    protected Dictionary<string, bool> AffectedUserIds { get; set; } = new();

    // TODO: A lot of this approach could potentially be reworked if I switch this all over to the CustomEffects approach

    // Override this to define what needs to happen when the drink's effect is disabled
    protected abstract void DisableEffects(Player player);

    // Call this with a reason selected when the drink's effect should be removed
    protected void Disable(Player player, bool expired = false, bool died = false, bool usedScp500 = false)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            string logMsg = "";
            if (affectedUserStatus.Value)
            {
                DisableEffects(player);

                logMsg = $"Disabled {Name} effect on player: {player.Nickname}";
            }
            else
            {
                // TODO: Once it works then we can just maybe remove the secondary log here
                logMsg = $"{Name} effect is ALREADY DISABLED on player: {player.Nickname}";
            }

            AffectedUserIds.Remove(affectedUserStatus.Key);

            if (expired)
            {
                logMsg += " - duration expired";
            }
            else if (died)
            {
                logMsg += " - player died";
            }
            else if (usedScp500)
            {
                logMsg += " - player used SCP-500";
            }
            Log.Debug(logMsg);
        }
    }
}