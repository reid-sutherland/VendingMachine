using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;

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

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("Effect given to non-gobbys. Defaults to RainbowTaste which reduces negative effects.")]
    public EffectType Effect { get; set; } = EffectType.RainbowTaste;

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 60.0f;

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
        ev.Player.DisableEffect(EffectType.AntiScp207);
        Log.Debug($"{ev.Player.Nickname} used a custom item: {Name}");

        if (ev.Player.UserId == "76561198076399181@steam")
        {
            // the gobby effect is reserved for gobby-equivalents
            Log.Info($"Enabling {Name} effect: {EffectType.Blinded} on gobby-equivalent: {ev.Player.Nickname} until the end of time");
            ev.Player.EnableEffect(EffectType.Blinded, 255, 0.0f, addDurationIfActive: true);
        }
        else
        {
            Log.Info($"Enabling {Name} effect: {Effect} on player: {ev.Player.Nickname} for {Duration} seconds");
            ev.Player.EnableEffect(Effect, Duration);
        }
        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }
}