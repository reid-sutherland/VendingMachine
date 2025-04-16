using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;
using Player = Exiled.API.Features.Player;

namespace VendingMachine.Drinks;

public abstract class CustomDrink : CustomItem
{
    // This should ONLY be defined in the config
    [Description("The chance that the drink is dispensed.")]
    public int Chance { get; set; }

    // This should be overrided to set a code-defined default duration per drink
    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public virtual float Duration { get; set; } = 180.0f;

    // This is defined here so that the config can't try to set a spawn location, since drinks come out of the vending machine
    [YamlIgnore]
    public override SpawnProperties SpawnProperties { get; set; } = new();

    // Each drink should use this to track players that are actively affected by the drink
    [YamlIgnore]
    protected Dictionary<string, bool> AffectedUserIds { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem += OnItemUsed;
        Exiled.Events.Handlers.Player.Dying += OnDying;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem -= OnItemUsed;
        Exiled.Events.Handlers.Player.Dying -= OnDying;

        base.UnsubscribeEvents();
    }

    private void OnItemUsed(UsedItemEventArgs ev)
    {
        // Disable effect when SCP-500 (red pill) is used
        if (ev.Item.Type == ItemType.SCP500)
        {
            Disable(ev.Player, usedScp500: true);
            return;
        }

        if (!Check(ev.Item))
        {
            return;
        }
        Log.Debug($"{ev.Player.Nickname} used a custom item: {Name}");
        if (Type == ItemType.SCP207)
        {
            ev.Player.DisableEffect(EffectType.Scp207);
        }
        else if (Type == ItemType.AntiSCP207)
        {
            ev.Player.DisableEffect(EffectType.AntiScp207);
        }
        else
        {
            Log.Warn($"{ev.Player.Nickname} used a custom item with unexpected ItemType: {Type}");
            Log.Warn($"Effects of custom item may stack with the root ItemType's effect");
        }

        bool consumed = Enable(ev.Player);
        if (consumed)
        {
            // TODO: This doesn't actually work yet, item is always removed. might be the base OnItemUsed is still removing it. Maybe give them a new one back or some shit
            // TODO: Add RemoveOnUse to this conditional to support items that can be re-used
            ev.Player.RemoveItem(ev.Player.CurrentItem);
        }
    }

    public void OnDying(DyingEventArgs ev)
    {
        Disable(ev.Player, died: true);
    }

    // Call this when the drink's effect should be applied to a player
    // Returns bool: whether item should be removed
    protected bool Enable(Player player)
    {
        if (AffectedUserIds.ContainsKey(player.UserId))
        {
            player.ShowHint($"You are already under the effects of {Name}!", 5.0f);
            Log.Debug($"{player.Nickname} is already under the effects of {Name}: ignoring");
            return false;
        }

        AffectedUserIds.Add(player.UserId, true);
        Log.Info($"Enabling {Name} effect on player: {player.Nickname} for {Duration} seconds");
        EnableEffects(player);
        if (Duration > 0)
        {
            Timing.CallDelayed(Duration, () =>
            {
                Disable(player, expired: true);
            });
        }
        return true;
    }

    // Call this with a reason selected when the drink's effect should be removed from a player
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
                // TODO: We can just maybe remove the secondary log here
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

    // Override this to define what needs to happen when the drink's effect is enabled
    protected abstract void EnableEffects(Player player);

    // Override this to define what needs to happen when the drink's effect is disabled
    protected abstract void DisableEffects(Player player);
}