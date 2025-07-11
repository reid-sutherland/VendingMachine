﻿using CommonUtils.Core.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;
using Player = Exiled.API.Features.Player;

namespace VendingMachine.Drinks;

public abstract class CustomDrink : CustomItem, IChanceObject
{
    // These are defined here so that the config doesn't try to mess with them
    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [YamlIgnore]
    public override Vector3 Scale { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);

    // This is defined here so that the config can't try to set a spawn location, since drinks come out of the vending machine
    [YamlIgnore]
    public override SpawnProperties SpawnProperties { get; set; } = new();

    // Each drink should use this to track players that are actively affected by the drink
    [YamlIgnore]
    protected Dictionary<string, bool> AffectedUserIds { get; set; } = new();

    // This should ONLY be defined in the config
    [Description("The chance that the drink is dispensed.")]
    public int Chance { get; set; } = 10;

    // This should be overrided to set a code-defined default duration per drink
    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public virtual float Duration { get; set; } = 180.0f;

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        Exiled.Events.Handlers.Server.RestartingRound += OnRestartingRound;
        Exiled.Events.Handlers.Player.UsedItem += OnItemUsed;
        Exiled.Events.Handlers.Player.Dying += OnDying;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
        Exiled.Events.Handlers.Server.RestartingRound -= OnRestartingRound;
        Exiled.Events.Handlers.Player.UsedItem -= OnItemUsed;
        Exiled.Events.Handlers.Player.Dying -= OnDying;

        base.UnsubscribeEvents();
    }

    protected void OnRoundEnded(RoundEndedEventArgs ev)
    {
        Log.Debug("Round ended: disabling custom drink effects on all players");
        DisableAll();
    }

    protected void OnRestartingRound()
    {
        Log.Debug("Round restarting: disabling custom drink effects on all players");
        DisableAll();
    }

    protected void OnItemUsed(UsedItemEventArgs ev)
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
        // Log if effects weren't cleared correctly in case it happens on a monkey's server
        if (ev.Item.Type == ItemType.SCP207 && ev.Player.TryGetEffect(EffectType.Scp207, out var scp207Effect) && scp207Effect.IsEnabled)
        {
            Log.Error($"-- player is STILL affected by Scp207");
        }
        if (ev.Item.Type == ItemType.AntiSCP207 && ev.Player.TryGetEffect(EffectType.AntiScp207, out var antiScp207Effect) && antiScp207Effect.IsEnabled)
        {
            Log.Error($"-- player is STILL affected by AntiScp207");
        }

        bool consumed = Enable(ev.Player);
        // TODO: Add RemoveOnUse to this conditional to support items that can be re-used
        if (consumed)
        {
            // TODO: This doesn't actually work yet, item is always removed. might be the base OnItemUsed is still removing it. Maybe give them a new one back or some shit
            ev.Player.RemoveItem(ev.Player.CurrentItem);
        }
    }

    protected void OnDying(DyingEventArgs ev)
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
    protected void Disable(Player player, bool expired = false, bool usedScp500 = false, bool died = false, bool roundEnded = false)
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
            else if (usedScp500)
            {
                logMsg += " - player used SCP-500";
            }
            else if (died)
            {
                logMsg += " - player died";
            }
            else if (roundEnded)
            {
                logMsg += " - round ended";
            }
            Log.Debug(logMsg);
        }
    }

    // This should only be called when the round ends
    protected void DisableAll()
    {
        // Disable() changes the collection, so compile a list of players first and then disable each
        List<Player> playersToDisable = new();
        foreach (var kvp in AffectedUserIds)
        {
            string userId = kvp.Key;
            Player player = Player.List.Where(p => p.UserId == userId).FirstOrDefault();
            if (player is not null)
            {
                playersToDisable.Add(player);
            }
        }
        foreach (Player player in playersToDisable)
        {
            Disable(player, roundEnded: true);
        }
    }

    // Override this to define what needs to happen when the drink's effect is enabled
    protected abstract void EnableEffects(Player player);

    // Override this to define what needs to happen when the drink's effect is disabled
    protected abstract void DisableEffects(Player player);
}