using Exiled.API.Enums;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

using VendingMachine.Drinks;

namespace VendingMachine;

public sealed class Config : IConfig
{
    [Description("Whether the vending machine (SCP-294) should be enabled and spawn in the map.")]
    public bool IsEnabled { get; set; } = true;

    public bool Debug { get; set; } = false;

    [Description("Whether AudioAPI routines will write logs to debug.")]
    public bool AudioDebug { get; set; } = false;

    [Description("Whether AMERT interaction will log things like distance to interactable schematics.")]
    public bool AmertDebug { get; set; } = false;

    [Description("Whether or not probabilities should be additive (50 + 50 = 100) or not (50 + 50 = 2 seperate 50% chances).")]
    public bool AdditiveProbabilities { get; set; } = true;

    // TODO: This should default to any of the basic 'rooms' (not 'hallways') in Entrance Zone
    [Description("The rooms and positions where the vending machine can spawn.")]
    public Dictionary<RoomType, Vector3> SpawnPoints { get; set; } = new();

    // It's too difficult to make a list of drinks since CustomDrink can't be instantiated
    // So define them individually here

    [Description("A regular SCP-207.")]
    public Scp207 Scp207 { get; set; } = new();

    [Description("A regular AntiSCP-207.")]
    public AntiScp207 AntiScp207 { get; set; } = new();

    [Description("Gobby-Pop. Makes gobby blind. Configurable effect for others.")]
    public GobbyPop GobbyPop { get; set; } = new();

    [Description("Helium Hatch. Makes your voice high-pitched.")]
    public HeliumHatch HeliumHatch { get; set; } = new();

    [Description("Poopsi. Causes extreme diarrhea.")]
    public Poopsi Poopsi { get; set; } = new();

    [Description("Old Man Rye. Makes you ghostly (walk through doors) like old man.")]
    public OldManRye OldManRye { get; set; } = new();

    [Description("Random Rootbeer. Gives you a random effect from the config list.")]
    public RandomRootbeer RandomRootbeer { get; set; } = new();

    [Description("McDonald's Sprite. Gives you hypothermia.")]
    public McDonaldsSprite McDonaldsSprite { get; set; } = new();

    [Description("Panera Lemonade. Hypes you up with speed and DR, but has a chance to cause a heart attack afterwards.")]
    public PaneraLemonade PaneraLemonade { get; set; } = new();

    // NEW DRINK IDEAS:
    //  - (Pepe) FiftyFifty: either kills you or makes you invincible for a while
    //  - (Pepe) SmokeBomb: spit out a vase-style cloud and/or flashbang everyone around you
    //  - (Binky) PaneraLemonade: HeartAttack + speed boost (or something else that makes it unique from SCP207) -- override Scale to make it huge lmao
}