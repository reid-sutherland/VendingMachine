using System.Collections.Generic;
using System.ComponentModel;
using AdvancedMERTools;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using VendingMachine.Utils;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;

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

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 90.0f;

    [Description("A list of possible effects and their additive chances.")]
    public Dictionary<EffectType, int> EffectChances { get; private set; } = new();

    protected override void SubscribeEvents()
    {
        Player.UsedItem += OnItemUsed;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Player.UsedItem -= OnItemUsed;

        base.UnsubscribeEvents();
    }

    private void OnItemUsed(UsedItemEventArgs ev)
    {
        if (!Check(ev.Item))
        {
            return;
        }
        Log.Debug($"{ev.Player.Nickname} used a custom item: {ev.Item}");
        ev.Player.DisableEffect(EffectType.AntiScp207);

        int roll = RollHelper.RollChanceFromCollection(EffectChances.Values);
        Log.Debug($"RandomRootbeer: rolled: {roll}");

        foreach (var kvp in EffectChances)
        {
            EffectType effect = kvp.Key;
            int chance = kvp.Value;
            Log.Debug($"-- current roll: {roll} - current chance: {chance} for effect: {effect}");
            if (roll <= chance)
            {
                Log.Info($"Enabling random effect: {effect} on player: {ev.Player.Nickname} for {Duration} seconds");
                ev.Player.EnableEffect(effect, 255, Duration);
                ev.Player.RemoveItem(ev.Player.CurrentItem);
                return;
            }

            if (MainPlugin.Configs.AdditiveProbabilities)
            {
                roll -= chance;
            }
        }
        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }
}