using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using YamlDotNet.Serialization;
using Player = Exiled.Events.Handlers.Player;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.SCP207)]
public class GobbyPop : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 0;

    [YamlIgnore]
    public override string Name { get; set; } = "Gobby-Pop";

    [YamlIgnore]
    public override string Description { get; set; } = "Drink it to get the Gobby treatment :)";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 15.0f;

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
        if (!Check(ev.Item))
        {
            // TODO: take this out (spam)
            Log.Debug($"{ev.Player.Nickname} used a NON-CUSTOM item: {ev.Item}");
            return;
        }
        Log.Debug($"{ev.Player.Nickname} used a custom item: {ev.Item}");
        ev.Player.DisableEffect(EffectType.Scp207);

        if (ev.Player.UserId == "76561198076399181@steam" || ev.Player.UserId == "76561198033598362@steam")
        {
            // the gobby effect is reserved for gobby-equivalents
            SelectedEffect = EffectType.CardiacArrest;
        }
        else
        {
            SelectedEffect = EffectType.RainbowTaste;
        }
        Log.Debug($"Enabling effect: {SelectedEffect} on player: {ev.Player.Nickname}");
        ev.Player.EnableEffect(SelectedEffect);

        if (Duration > 0)
        {
            Log.Debug($"Disabling effects in {Duration} seconds...");
            Timing.CallDelayed(Duration, () =>
            {
                Log.Debug($"Disabling effect: {SelectedEffect} on player: {ev.Player.Nickname}");
                ev.Player.DisableEffect(SelectedEffect);
            });
        }

        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        // TODO: It'd be funny to add a Helium soda that just makes your voice high pitched lol

        //if (!ev.Player.GameObject.TryGetComponent(out Scp559SizeEffect _))
        //    return;
        //ev.VoiceMessage = VoicePitchUtilities.SetVoicePitch(ev.VoiceMessage);
    }

    public void OnDying(DyingEventArgs ev)
    {
        // TODO: Use this example for adding custom new sodas

        //if (!ev.Player.GameObject.TryGetComponent(out Scp559SizeEffect scp559Effect))
        //    return;

        //Object.Destroy(scp559Effect);
        //ev.Player.Scale = Vector3.one;
    }
}