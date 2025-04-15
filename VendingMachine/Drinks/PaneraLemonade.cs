using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.CustomItems.API.Features;
using MEC;
using System.ComponentModel;
using UnityEngine;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.AntiSCP207)]
public class PaneraLemonade : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 8;

    [YamlIgnore]
    public override string Name { get; set; } = "Panera Lemonade";

    [YamlIgnore]
    public override string Description { get; set; } = "There's a LOT of caffeine in this... drink at your own risk!";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [YamlIgnore]
    public override Vector3 Scale { get; set; } = new Vector3(2.0f, 2.0f, 2.0f);

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 60.0f;

    [Description("Level of intensity for the MovementBoost effect in the range 0-255.")]
    public byte MovementBoostIntensity { get; set; } = 128;

    [Description("Chance out of 100 for a cardiac arrest to occur after the main effects end.")]
    public int CardiacArrestChance { get; set; } = 50;

    protected override void EnableEffects(Player player)
    {
        player.EnableEffect(EffectType.BodyshotReduction, 128, Duration);
        player.EnableEffect(EffectType.Invigorated, 255, Duration);
        player.EnableEffect(EffectType.MovementBoost, MovementBoostIntensity, Duration);

        int roll = MainPlugin.Random.Next(100);
        if (roll < CardiacArrestChance)
        {
            Log.Debug($"PaneraLemonade: roll {roll} was less than cardiac arrest chance: {CardiacArrestChance} - applying heart attack to player: {player.Nickname} after main effects expire");
            Timing.CallDelayed(Duration, () =>
            {
                Log.Debug($"PaneraLemonade: effect expired: applying heart attack to player: {player.Nickname}");
                player.EnableEffect(EffectType.CardiacArrest, 128, Duration);
            });
        }
        else
        {
            Log.Debug($"PaneraLemonade: roll {roll} was NOT less than cardiac arrest chance: {CardiacArrestChance}");
        }
    }

    protected override void DisableEffects(Player player)
    {
        player.DisableEffect(EffectType.DamageReduction);
        player.DisableEffect(EffectType.Invigorated);
        player.DisableEffect(EffectType.MovementBoost);
    }
}