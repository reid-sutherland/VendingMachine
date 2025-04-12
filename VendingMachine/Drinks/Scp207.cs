using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.SCP207)]
public class Scp207 : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 0;

    [YamlIgnore]
    public override string Name { get; set; } = "SCP-207";

    [YamlIgnore]
    public override string Description { get; set; } = "Move fast at the cost of health";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    protected override void DisableEffects(Player player)
    {
        return;
    }
}