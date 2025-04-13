using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.SCP207)]
public class OldManRye : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 5;

    [YamlIgnore]
    public override string Name { get; set; } = "Old Man Rye";

    [YamlIgnore]
    public override string Description { get; set; } = "The whiskey that reminds you of your old man";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 120.0f;

    protected override void EnableEffects(Player player)
    {
        player.EnableEffect(EffectType.Ghostly, 255, Duration);
    }

    protected override void DisableEffects(Player player)
    {
        player.DisableEffect(EffectType.Ghostly);
    }
}