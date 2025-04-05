using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using VendingMachine.Drinks;

namespace VendingMachine.Utils;

public static class RollHelper
{
    public static int RollChance(IDictionary<string, CustomDrink> chances)
    {
        double rolledChance = MainPlugin.Random.NextDouble();

        if (MainPlugin.Configs.AdditiveProbabilities)
        {
            rolledChance *= chances.Sum(kvp => kvp.Value.Chance);
        }
        else
        {
            rolledChance *= 100;
        }
        return (int)rolledChance;
    }

    public static int RollChanceFromConfig(Config config)
    {
        double rolledChance = MainPlugin.Random.NextDouble();

        if (MainPlugin.Configs.AdditiveProbabilities)
        {
            // kinda jank but it's readable :)
            double sum = 0.0;
            foreach (PropertyInfo pInfo in config.GetType().GetProperties())
            {
                if (typeof(CustomDrink).IsAssignableFrom(pInfo.PropertyType))
                {
                    CustomDrink d = pInfo.GetValue(config) as CustomDrink;
                    if (d is not null)
                    {
                        sum += d.Chance;
                    }
                }
            }
            if (sum <= 0)
            {
                throw new Exception($"Chances sum is {sum}: check that there are drinks in the config!");
            }

            rolledChance *= sum;
        }
        else
        {
            rolledChance *= 100;
        }
        return (int)rolledChance;
    }
}