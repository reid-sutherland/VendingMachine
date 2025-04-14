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

    public override Vector3 Scale { get; set; } = new Vector3(2.0f, 2.0f, 2.0f);

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 60.0f;

    //[Description("Chance out of 100 for cardiac arrest to occur.")]
    //public int CardiacArrestChance { get; set; } = 40;

    protected override void EnableEffects(Player player)
    {
        player.EnableEffect(EffectType.BodyshotReduction, 128, Duration);
        player.EnableEffect(EffectType.Invigorated, 255, Duration);
        player.EnableEffect(EffectType.MovementBoost, 255, Duration);

        Timing.CallDelayed(Duration, () =>
        {
            Log.Debug($"PaneraLemonade: duration expired: applying heart attack to player: {player.Nickname}");
            player.EnableEffect(EffectType.CardiacArrest, 255, 30.0f);
        });

        // The chance heart attack version - too OP if movementboost is full
        //int roll = MainPlugin.Random.Next(100);
        //if (roll < CardiacArrestChance)
        //{
        //    Timing.CallDelayed(Duration, () =>
        //    {
        //        Log.Debug($"PaneraLemonade: effect expired: applying heart attack to player: {player.Nickname}"); 
        //        player.EnableEffect(EffectType.CardiacArrest, 128, Duration);
        //    });
        //    Log.Debug($"PaneraLemonade: roll {roll} was less than cardiac arrest chance: {CardiacArrestChance} - applying heart attack to player: {player.Nickname} after main effect expires");
        //}
        //else
        //{
        //    Log.Debug($"PaneraLemonade: roll {roll} was NOT less than cardiac arrest chance: {CardiacArrestChance}");
        //}
    }

    protected override void DisableEffects(Player player)
    {
        player.DisableEffect(EffectType.DamageReduction);
        player.DisableEffect(EffectType.Invigorated);
        player.DisableEffect(EffectType.MovementBoost);
    }
}