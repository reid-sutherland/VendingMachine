using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.SCP207)]
public class Poopsi : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 4;

    [YamlIgnore]
    public override string Name { get; set; } = "Poopsi";

    [YamlIgnore]
    public override string Description { get; set; } = "Causes extreme diarrhea... that makes you stronger";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 90.0f;

    [Description("How much artificial health the player gets when pooping.")]
    public float TantrumArtificialHealth { get; set; } = 25.0f;

    [Description("How many seconds between poops")]
    public float TantrumInterval { get; set; } = 10.0f;

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
            Log.Debug($"-- check failed: {ev.Item}");
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
        PlaceTantrum(ev.Player);
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

    private void PlaceTantrum(Exiled.API.Features.Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            if (affectedUserStatus.Value)
            {
                Log.Debug($"Placing tantrum and adding health for player: {player.Nickname}");
                player.PlaceTantrum();
                player.ArtificialHealth += TantrumArtificialHealth;
                Timing.CallDelayed(TantrumInterval, () =>
                {
                    PlaceTantrum(player);
                });
            }
        }
    }

    protected override void DisableEffects(Exiled.API.Features.Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            AffectedUserIds[affectedUserStatus.Key] = false;
        }
    }
}