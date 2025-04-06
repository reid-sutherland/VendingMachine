using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using YamlDotNet.Serialization;
using VendingMachine.Utils.Voice;
using Player = Exiled.Events.Handlers.Player;

namespace VendingMachine.Drinks;

[CustomItem(ItemType.AntiSCP207)]
public class HeliumHatch : CustomDrink
{
    [YamlIgnore]
    public override uint Id { get; set; } = 3;

    [YamlIgnore]
    public override string Name { get; set; } = "Helium Hatch";

    [YamlIgnore]
    public override string Description { get; set; } = "Drink it to get a huff of Helium";

    [YamlIgnore]
    public override float Weight { get; set; } = 1.0f;

    [Description("How long the effect lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 150.0f;

    private string AffectedUserId { get; set; } = "";

    private VoicePitchUtilities VoicePitchUtils { get; set; } = new();

    // TODO: The voice effect is currently scuffed.
    // It works great when one person uses one.
    // If another player uses one, instead of "chipmunk" they get the "demon voice", and the first user immediately loses the chipmunk voice.
    // This continues until all of the effects wear off, then the next use will work again, but the cycle repeats.

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

        AffectedUserId = ev.Player.UserId;
        Player.VoiceChatting += OnVoiceChatting;
        Log.Info($"Enabling {Name} effect on player: {ev.Player.Nickname} for {Duration} seconds");
        if (Duration > 0)
        {
            Timing.CallDelayed(Duration, () =>
            {
                AffectedUserId = "";
                Player.VoiceChatting -= OnVoiceChatting;
                Log.Debug($"Disabling {Name} effect on player: {ev.Player.Nickname}");
            });
        }
        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnDying(DyingEventArgs ev)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && ev.Player.UserId == AffectedUserId)
        {
            AffectedUserId = "";
            Player.VoiceChatting -= OnVoiceChatting;
            Log.Debug($"Affected player {ev.Player.UserId} died - disabling {Name} effect");
        }
    }

    public void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && ev.Player.UserId == AffectedUserId)
        {
            ev.VoiceMessage = VoicePitchUtils.SetVoicePitch(ev.VoiceMessage);
        }
    }
}