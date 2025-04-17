using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.AntiSCP207)]
public class GobbyPop : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 2;

    [YamlIgnore]
    public override string Name { get; set; } = "Gobby-Pop";

    [YamlIgnore]
    public override string Description { get; set; } = "Drink it to get the Gobby treatment :)";

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 0.0f;

    [Description("User IDs of players that should receive the Gobby effect. :)")]
    public List<string> GobbyEquivalentIds { get; set; } = new();

    [Description("Effect given to non-gobbys. Defaults to RainbowTaste which reduces negative effects.")]
    public EffectType Effect { get; set; } = EffectType.RainbowTaste;

    protected override void EnableEffects(Player player)
    {
        if (GobbyEquivalentIds.Contains(player.UserId))
        {
            // the gobby effect is reserved for gobby-equivalents
            player.EnableEffect(EffectType.Blinded, 255, 0.0f, addDurationIfActive: true);
            Log.Info($"{Name} effect for gobby-equivalent player {player.Nickname}: permanent {EffectType.Blinded}");
        }
        else
        {
            player.EnableEffect(Effect, 255, Duration, addDurationIfActive: true);
            Log.Info($"{Name} effect for non-gobby player {player.Nickname}: temporary {Effect}");
        }
    }

    protected override void DisableEffects(Player player)
    {
        if (GobbyEquivalentIds.Contains(player.UserId))
        {
            player.DisableEffect(EffectType.Blinded);
        }
        else
        {
            player.DisableEffect(Effect);
        }
    }
}