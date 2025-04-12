using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;

using VendingMachine.Utils;
using CustomPlayerEffects;
using System.Linq;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.SCP207)]
public class RandomRootbeer : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 6;

    [YamlIgnore]
    public override string Name { get; set; } = "B.A.R.Q.S.";

    [YamlIgnore]
    public override string Description { get; set; } = "The Original Barq's Blooded Random Rootbeer";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 60.0f;

    [Description("A list of possible effects and their additive chances.")]
    public Dictionary<EffectType, int> EffectChances { get; private set; } = new()
    {
        // Good effects
        { EffectType.BodyshotReduction, 10 },
        { EffectType.DamageReduction, 10 },
        { EffectType.Invigorated, 10 },
        { EffectType.Invisible, 10 },
        { EffectType.MovementBoost, 10 },
        { EffectType.SilentWalk, 10 },
        { EffectType.Vitality, 10 },
        // Bad effects
        { EffectType.AmnesiaItems, 5 },
        { EffectType.Blurred, 5 },
        { EffectType.Concussed, 5 },
        { EffectType.Deafened, 5 },
        { EffectType.Disabled, 5 },
        { EffectType.Exhausted, 5 },
    };

    private Dictionary<string, EffectType> AffectedUserEffects { get; set; }

    protected override void SubscribeEvents()
    {
        Player.UsedItem += OnItemUsed;
        Player.Dying += OnDying;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Player.UsedItem -= OnItemUsed;
        Player.Dying -= OnDying;

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
        ev.Player.DisableEffect(EffectType.Scp207);
        Log.Debug($"{ev.Player.Nickname} used a custom item: {Name}");

        if (AffectedUserIds.ContainsKey(ev.Player.UserId))
        {
            Log.Debug($"{ev.Player.Nickname} is already under the affects of {Name}: ignoring");
            return;
        }

        int roll = RollHelper.RollChanceFromCollection(EffectChances.Values);
        Log.Debug($"RandomRootbeer: rolled: {roll}");
        foreach (var kvp in EffectChances)
        {
            EffectType effect = kvp.Key;
            int chance = kvp.Value;
            Log.Debug($"-- current roll: {roll} - current chance: {chance} for effect: {effect}");

            if (roll <= chance)
            {
                AffectedUserEffects.Add(ev.Player.UserId, effect);
                AffectedUserIds.Add(ev.Player.UserId, true);
                ev.Player.EnableEffect(effect, 200, Duration, addDurationIfActive: true);
                ev.Player.ShowHint($"You got the {effect} effect!", 5.0f);
                Log.Info($"Enabling random {Name} effect: {effect} on player: {ev.Player.Nickname} for {Duration} seconds");
                if (Duration > 0)
                {
                    Timing.CallDelayed(Duration, () =>
                    {
                        Disable(ev.Player, expired: true);
                    });
                }
                break;
            }

            if (MainPlugin.Configs.AdditiveProbabilities)
            {
                roll -= chance;
            }
        }
        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnDying(DyingEventArgs ev)
    {
        Disable(ev.Player, died: true);
    }

    protected override void DisableEffects(Exiled.API.Features.Player player)
    {
        var affectedUserEffect = AffectedUserEffects.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserEffect.Key != null)
        {
            player.DisableEffect(affectedUserEffect.Value);
            AffectedUserEffects.Remove(player.UserId);
        }
    }
}