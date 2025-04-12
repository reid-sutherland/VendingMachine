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
    public float Duration { get; set; } = 120.0f;

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
        ev.Player.DisableEffect(EffectType.Scp207);
        Log.Debug($"{ev.Player.Nickname} used a custom item: {Name}");

        if (AffectedUserIds.ContainsKey(ev.Player.UserId))
        {
            Log.Debug($"{ev.Player.Nickname} is already under the affects of {Name}: ignoring");
            return;
        }

        AffectedUserIds.Add(ev.Player.UserId, true);
        ev.Player.EnableEffect(EffectType.Ghostly, 255, Duration);
        Log.Info($"Enabling {Name} effect on player: {ev.Player.Nickname} for {Duration} seconds");
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
        player.DisableEffect(EffectType.Ghostly);
    }
}