using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using MEC;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.AntiSCP207)]
public class McDonaldsSprite : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 7;

    [YamlIgnore]
    public override string Name { get; set; } = "McDonald's Sprite";

    [YamlIgnore]
    public override string Description { get; set; } = "Drink it to experience that cool and crisp Sprite taste";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 30.0f;

    [Description("How much health the player gets every {HealthInterval} seconds of Hypothermia.")]
    public float HealthGiven { get; set; } = 25.0f;

    [Description("How often to give artificial health while Hypothermia is active.")]
    public float HealthInterval { get; set; } = 5.0f;

    [Description("How much to increase health given after each proc. This is done because Hypothermia damage ramps up over time.")]
    public float HealthIncrement { get; set; } = 10.0f;

    private Dictionary<string, float> AffectedUserCurrentHealthGiven { get; set; } = new();

    protected override void EnableEffects(Player player)
    {
        // Give them artificial health on an interval so they don't die from the hypothermia
        if (Duration <= 0.0f)
        {
            // This drink should never be infinite lol
            Duration = 30.0f;
        }

        AffectedUserCurrentHealthGiven.Add(player.UserId, HealthGiven);
        player.EnableEffect(EffectType.Hypothermia, 255, Duration, addDurationIfActive: true);
        GiveHealth(player);
    }

    protected override void DisableEffects(Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            player.DisableEffect(EffectType.Hypothermia);
            AffectedUserIds[affectedUserStatus.Key] = false;
            AffectedUserCurrentHealthGiven.Remove(affectedUserStatus.Key);
        }
    }

    private void GiveHealth(Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        var currentHealthGiven = AffectedUserCurrentHealthGiven.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null && currentHealthGiven.Key != null)
        {
            if (affectedUserStatus.Value)
            {
                // Note: HumeShield stat cannot be seen by player but it works well
                player.HumeShield += currentHealthGiven.Value;
                AffectedUserCurrentHealthGiven[currentHealthGiven.Key] += HealthIncrement;
                Timing.CallDelayed(HealthInterval, () =>
                {
                    GiveHealth(player);
                });
            }
        }
    }
}