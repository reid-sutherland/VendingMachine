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

        AffectedUserId = ev.Player.UserId;
        Player.VoiceChatting += OnVoiceChatting;
        Log.Info($"Enabling Helium voice effect on player: {ev.Player.Nickname}");
        if (Duration > 0)
        {
            Log.Debug($"Disabling effects in {Duration} seconds...");
            Timing.CallDelayed(Duration, () =>
            {
                Log.Debug($"Disabling Helium voice effect on player: {ev.Player.Nickname}");
                Player.VoiceChatting -= OnVoiceChatting;
            });
        }

        ev.Player.RemoveItem(ev.Player.CurrentItem);
    }

    public void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && ev.Player.UserId == AffectedUserId)
        {
            ev.VoiceMessage = VoicePitchUtils.SetVoicePitch(ev.VoiceMessage);
        }
    }

    public void OnDying(DyingEventArgs ev)
    {
        if (!string.IsNullOrEmpty(AffectedUserId) && ev.Player.UserId == AffectedUserId)
        {
            Log.Debug($"Affected player {ev.Player.UserId} died - removing Helium voice effect");
            Player.VoiceChatting -= OnVoiceChatting;
        }
    }
}