using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

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

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 300.0f;

    // TODO: The voice effect is currently scuffed.
    // It works great when one person uses one.
    // If another player uses one, instead of "chipmunk" they get the "demon voice", and the first user immediately loses the chipmunk voice.
    // This continues until all of the effects wear off, then the next use will work again, but the cycle repeats.
    private VoicePitchUtilities VoicePitchUtils { get; set; } = new();

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;

        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;

        base.UnsubscribeEvents();
    }

    public void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        // For performance, first check the count of affected players (since this handler is called when any player chats)
        if (AffectedUserIds.Count <= 0)
        {
            return;
        }

        // Check that the player is affected before applying voice pitch
        if (AffectedUserIds.ContainsKey(ev.Player.UserId))
        {
            ev.VoiceMessage = VoicePitchUtils.SetVoicePitch(ev.VoiceMessage);
        }
    }

    protected override void EnableEffects(Player player)
    {
        // Nothing to do, OnVoiceChatting will take care of checking that the player is currently affected
        return;
    }

    protected override void DisableEffects(Player player)
    {
        // Nothing to do, Disable will remove player from affected users
        return;
    }
}