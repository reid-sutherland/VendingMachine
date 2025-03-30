using System.Linq;
using System.Collections.Generic;
using VendingMachine.ConfigObjects;

namespace VendingMachine;

public static class Utils
{
    public static int RollChance(IEnumerable<IChanceObject> chances)
    {
        double rolledChance = MainPlugin.Random.NextDouble();

        if (MainPlugin.Configs.AdditiveProbabilities)
        {
            rolledChance *= chances.Sum(x => x.Chance);
        }
        else
        {
            rolledChance *= 100;
        }
        return (int)rolledChance;
    }
}