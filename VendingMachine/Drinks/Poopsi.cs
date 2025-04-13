using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using MEC;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

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

    [Description("How long the drink's effects lasts for. A value of 0 means infinite.")]
    public override float Duration { get; set; } = 90.0f;

    [Description("How much artificial health the player gets when pooping.")]
    public float TantrumArtificialHealth { get; set; } = 25.0f;

    [Description("How many seconds between poops")]
    public float TantrumInterval { get; set; } = 10.0f;

    protected override void EnableEffects(Player player)
    {
        PlaceTantrum(player);
    }

    protected override void DisableEffects(Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            AffectedUserIds[affectedUserStatus.Key] = false;
        }
    }

    private void PlaceTantrum(Player player)
    {
        var affectedUserStatus = AffectedUserIds.Where(kvp => kvp.Key == player.UserId).FirstOrDefault();
        if (affectedUserStatus.Key != null)
        {
            if (affectedUserStatus.Value)
            {
                Log.Debug($"Placing tantrum and adding health for player: {player.Nickname}");
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