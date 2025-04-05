using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
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

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 90.0f;

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
        Log.Debug($"{ev.Player.Nickname} used a custom item: {ev.Item}");
        ev.Player.DisableEffect(EffectType.AntiScp207);

        Log.Info($"Enabling OldManRye effect on player: {ev.Player.Nickname} for {Duration} seconds");
        ev.Player.EnableEffect(EffectType.Ghostly, Duration);
        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }
}