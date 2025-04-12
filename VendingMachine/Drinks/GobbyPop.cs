using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.ComponentModel;
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

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 0.0f;

    [Description("Effect given to non-gobbys. Defaults to RainbowTaste which reduces negative effects.")]
    public EffectType Effect { get; set; } = EffectType.RainbowTaste;

    protected override void SubscribeEvents()
    {
        Player.UsedItem += OnItemUsed;
        Player.Dying += OnDying;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Player.UsedItem -= OnItemUsed;
        Player.Dying -= OnDying;

        base.UnsubscribeEvents();
    }

    private void OnItemUsed(UsedItemEventArgs ev)
    {
        // Disable effect when SCP-500 (red pill) is used
        if (ev.Item.Type == ItemType.SCP500)
        {
            Disable(ev.Player, usedScp500: true);
            return;
        }

        if (!Check(ev.Item))
        {
            return;
        }
        ev.Player.DisableEffect(EffectType.AntiScp207);
        Log.Debug($"{ev.Player.Nickname} used a custom item: {Name}");

        if (AffectedUserIds.ContainsKey(ev.Player.UserId))
        {
            Log.Debug($"{ev.Player.Nickname} is already under the affects of {Name}: ignoring");
            return;
        }

        AffectedUserIds.Add(ev.Player.UserId, true);
        if (ev.Player.UserId == "76561198076399181@steam")
        {
            // the gobby effect is reserved for gobby-equivalents
            Duration = 0.0f;
            ev.Player.EnableEffect(EffectType.Blinded, 255, Duration, addDurationIfActive: true);
            Log.Info($"Enabling {Name} effect: {EffectType.Blinded} on gobby-equivalent: {ev.Player.Nickname} until the end of time");
        }
        else
        {
            ev.Player.EnableEffect(Effect, 255, Duration, addDurationIfActive: true);
            Log.Info($"Enabling {Name} effect: {Effect} on player: {ev.Player.Nickname} for {Duration} seconds");
        }
        if (Duration > 0)
        {
            Timing.CallDelayed(Duration, () =>
            {
                Disable(ev.Player, expired: true);
            });
        }

        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnDying(DyingEventArgs ev)
    {
        Disable(ev.Player, died: true);
    }

    protected override void DisableEffects(Exiled.API.Features.Player player)
    {
        if (player.UserId == "76561198076399181@steam")
        {
            player.DisableEffect(EffectType.Blinded);
        }
        else
        {
            player.DisableEffect(Effect);
        }
    }
}