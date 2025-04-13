using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.AntiSCP207)]
public class AntiScp207 : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 1;

    [YamlIgnore]
    public override string Name { get; set; } = "AntiSCP-207";

    [YamlIgnore]
    public override string Description { get; set; } = "Become tanky at the cost of move speed";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    protected override void EnableEffects(Player player)
    {
    }

    protected override void DisableEffects(Player player)
    {
    }
}