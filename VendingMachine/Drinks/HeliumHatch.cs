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

using VendingMachine.Utils.Voice;

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

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public float Duration { get; set; } = 300.0f;

    // TODO: The voice effect is currently scuffed.
    // It works great when one person uses one.
    // If another player uses one, instead of "chipmunk" they get the "demon voice", and the first user immediately loses the chipmunk voice.
    // This continues until all of the effects wear off, then the next use will work again, but the cycle repeats.
    private VoicePitchUtilities VoicePitchUtils { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Player.UsedItem += OnItemUsed;
        Player.Dying += OnDying;
        Player.VoiceChatting += OnVoiceChatting;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Player.UsedItem -= OnItemUsed;
        Player.Dying -= OnDying;
        Player.VoiceChatting -= OnVoiceChatting;

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

        AffectedUserIds.Add(ev.Player.UserId, true);
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

    public void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == ev.Player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            if (affectedUserStatus.Value)
            {
                ev.VoiceMessage = VoicePitchUtils.SetVoicePitch(ev.VoiceMessage);
            }
        }
    }

    protected override void DisableEffects(Exiled.API.Features.Player player)
    {
        // TODO: This might be completely un-necessary - maybe nothing needs doing here
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            AffectedUserIds[affectedUserStatus.Key] = false;
        }
    }
}