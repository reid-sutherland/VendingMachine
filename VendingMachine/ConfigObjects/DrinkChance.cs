using VendingMachine.Drinks;

namespace VendingMachine.ConfigObjects;

public class DrinkChance : IChanceObject
{
    public CustomItemType ItemType { get; set; }

    public double Chance { get; set; }

    public void Deconstruct(out CustomItemType itemType, out double chance)
    {
        itemType = ItemType;
        chance = Chance;
    }
}