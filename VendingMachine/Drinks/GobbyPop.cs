using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using Player = Exiled.Events.Handlers.Player;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.SCP207)]
public class GobbyPop : CustomItem
{
    public CustomItemType CustomItemType { get; private set; } = CustomItemType.GobbyPop;

    public override uint Id { get; set; } = 1;

    public override string Name { get; set; } = "Gobby-Pop";

    public override string Description { get; set; } = "Drink it to get the Gobby treatment :)";

    public override float Weight { get; set; } = 1f;

    public override SpawnProperties SpawnProperties { get; set; }
    //public override SpawnProperties SpawnProperties { get; set; } = new()
    //{
    //    Limit = 1,
    //    DynamicSpawnPoints = new List<DynamicSpawnPoint>
    //    {
    //        new()
    //        {
    //            Chance = 100,
    //            Location = SpawnLocationType.Inside096,
    //        },
    //    },
    //};

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 15f;

    [Description("The effect applied on use. Depends on the player.")]
    private EffectType SelectedEffect { get; set; } = EffectType.None;

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
        // checks if item is a CustomItem
        if (!Check(ev.Player.CurrentItem))
        {
            return;
        }
        Log.Debug($"{ev.Player.Nickname} used a custom item: {ev.Item} - type: {ev.Item.Type}");
        Log.Debug($"Player id: {ev.Player.NetId} -- {ev.Player.NetworkIdentity}");
        ev.Player.DisableEffect(EffectType.Scp207);

        Timing.CallDelayed(1.5f, () =>
        {
            if (ev.Player.Nickname == "jorts fetish" || ev.Player.Nickname == "blanky")
            {
                SelectedEffect = EffectType.CardiacArrest;
            }
            else
            {
                SelectedEffect = EffectType.Invisible;
            }
            ev.Player.EnableEffect(SelectedEffect);
        });

        if (Duration > 0)
        {
            Timing.CallDelayed(Duration, () =>
            {
                ev.Player.DisableEffect(SelectedEffect);
            });
        }

        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }
}