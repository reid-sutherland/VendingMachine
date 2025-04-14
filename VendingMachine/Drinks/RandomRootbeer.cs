using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

using VendingMachine.Utils;

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
    public override float Duration { get; set; } = 120.0f;

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

    private Dictionary<string, EffectType> AffectedUserEffects { get; set; } = new();

    protected override void EnableEffects(Player player)
    {
        int roll = RollHelper.RollChanceFromCollection(EffectChances.Values);
        Log.Debug($"RandomRootbeer: rolled: {roll}");
        foreach (var kvp in EffectChances)
        {
            EffectType effect = kvp.Key;
            int chance = kvp.Value;
            Log.Debug($"-- current roll: {roll} - current chance: {chance} for effect: {effect}");

            if (roll <= chance)
            {
                AffectedUserEffects.Add(player.UserId, effect);
                player.EnableEffect(effect, 200, Duration, addDurationIfActive: true);
                player.ShowHint($"You got the {effect} effect!", 5.0f);
                Log.Info($"{Name} random effect for player {player.Nickname}: temporary {effect}");
                break;
            }

            if (MainPlugin.Configs.AdditiveProbabilities)
            {
                roll -= chance;
            }
        }
    }

    protected override void DisableEffects(Player player)
    {
        var affectedUserEffect = AffectedUserEffects.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserEffect.Key != null)
        {
            player.DisableEffect(affectedUserEffect.Value);
            AffectedUserEffects.Remove(player.UserId);
        }
    }
}