using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Usables.Scp244.Hypothermia;
using MEC;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;

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

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 30.0f;

    [Description("How much health the player gets every 5 seconds of Hypothermia.")]
    public float HealthGiven { get; set; } = 25.0f;

    [Description("How often to give artificial health why Hypothermia is active.")]
    public float HealthInterval { get; set; } = 5.0f;

    [Description("How much to increase health given after each proc. This is done because Hypothermia damage ramps up over time.")]
    public float HealthIncrement { get; set; } = 10.0f;

    private float CurrentHealthGiven { get; set; }

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
        ev.Player.DisableEffect(EffectType.AntiScp207);
        Log.Debug($"{ev.Player.Nickname} used a custom item: {Name}");

        // Give them artificial health on an interval so they don't die from the hypothermia
        if (Duration <= 0.0f)
        {
            // This drink should never be infinite lol
            Duration = 30.0f;
        }
        CurrentHealthGiven = HealthGiven;

        AffectedUserId = ev.Player.UserId;
        ev.Player.EnableEffect(EffectType.Hypothermia, 255, Duration);
        Log.Info($"Enabling {Name} effect on player: {ev.Player.Nickname} for {Duration} seconds");
        GiveHealth(ev.Player);
        Timing.CallDelayed(Duration, () =>
        {
            AffectedUserId = "";
            ev.Player.DisableEffect(EffectType.Hypothermia);
            Log.Debug($"Disabling {Name} effect on player: {ev.Player.Nickname}");
        });

        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnDying(DyingEventArgs ev)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && ev.Player.UserId == AffectedUserId)
        {
            AffectedUserId = "";
            ev.Player.DisableEffect(EffectType.Hypothermia);
            Log.Debug($"Affected player {ev.Player.UserId} died - removing {Name} effect");
        }
    }

    private void GiveHealth(Exiled.API.Features.Player player)
    {
        Log.Debug($"Giving {CurrentHealthGiven} health to player: {player.UserId} - affected: {AffectedUserId}");
        if (!string.IsNullOrEmpty(AffectedUserId) && player.UserId == AffectedUserId)
        {
            // Note: HumeShield stat cannot be seen by player but it works well
            player.HumeShield += CurrentHealthGiven;
            CurrentHealthGiven += HealthIncrement;
            Timing.CallDelayed(HealthInterval, () =>
            {
                GiveHealth(player);
            });
        }
    }
}