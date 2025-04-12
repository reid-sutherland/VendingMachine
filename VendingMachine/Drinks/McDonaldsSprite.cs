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

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 30.0f;

    [Description("How much health the player gets every 5 seconds of Hypothermia.")]
    public float HealthGiven { get; set; } = 25.0f;

    [Description("How often to give artificial health why Hypothermia is active.")]
    public float HealthInterval { get; set; } = 5.0f;

    [Description("How much to increase health given after each proc. This is done because Hypothermia damage ramps up over time.")]
    public float HealthIncrement { get; set; } = 10.0f;

    private float CurrentHealthGiven { get; set; }

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

        // Give them artificial health on an interval so they don't die from the hypothermia
        if (Duration <= 0.0f)
        {
            // This drink should never be infinite lol
            Duration = 30.0f;
        }
        CurrentHealthGiven = HealthGiven;

        AffectedUserIds.Add(ev.Player.UserId, true);
        ev.Player.EnableEffect(EffectType.Hypothermia, 255, Duration);
        Log.Info($"Enabling {Name} effect on player: {ev.Player.Nickname} for {Duration} seconds");
        GiveHealth(ev.Player);
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

    private void GiveHealth(Exiled.API.Features.Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            if (affectedUserStatus.Value)
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

    protected override void DisableEffects(Exiled.API.Features.Player player)
    {
        player.DisableEffect(EffectType.Hypothermia);
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            AffectedUserIds[affectedUserStatus.Key] = false;
        }
    }
}