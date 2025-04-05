using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp173;
using MEC;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;
using Scp173 = Exiled.Events.Handlers.Scp173;

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

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 120.0f;

    [Description("How much artificial health the player gets when pooping")]
    public float TantrumArtificialHealth { get; set; } = 5.0f;

    [Description("How many seconds between poops")]
    public float TantrumInterval { get; set; } = 5.0f;

    private bool TantrumActive { get; set; } = false;

    private string AffectedUserId { get; set; } = "";

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
        if (!Check(ev.Item))
        {
            return;
        }
        Log.Debug($"{ev.Player.Nickname} used a custom item: {ev.Player.CurrentItem}");
        ev.Player.DisableEffect(EffectType.Scp207);

        AffectedUserId = ev.Player.UserId;
        TantrumActive = true;
        Log.Info($"Enabling Poopsi effect on player: {ev.Player.Nickname} for {Duration} seconds");
        PlaceTantrum(ev.Player);

        if (Duration > 0)
        {
            Log.Debug($"Disabling Poopsi effect in {Duration} seconds...");
            Timing.CallDelayed(Duration, () =>
            {
                Log.Debug($"Disabling Poopsi effect on player: {ev.Player.Nickname}");
                TantrumActive = false;
            });
        }

        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnDying(DyingEventArgs ev)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && ev.Player.UserId == AffectedUserId)
        {
            Log.Debug($"Affected player {ev.Player.UserId} died - removing Poopsi effect");
            TantrumActive = false;
        }
    }

    private void PlaceTantrum(Exiled.API.Features.Player player)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && player.UserId == AffectedUserId)
        {
            if (TantrumActive)
            {
                player.PlaceTantrum();
                player.ArtificialHealth += TantrumArtificialHealth;
                Timing.CallDelayed(TantrumInterval, () =>
                {
                    PlaceTantrum(player);
                });
            }
        }
    }
}