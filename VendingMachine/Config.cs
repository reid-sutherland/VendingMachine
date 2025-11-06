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

    [Description("Whether debug logs will show in the console.")]
    public bool Debug { get; set; } = false;

    [Description("Whether debug logs from AudioPlayerAPI routines show.")]
    public bool AudioDebug { get; set; } = false;

    [Description("Whether debug logs from random chance rolls will show.")]
    public bool RollDebug { get; set; } = false;

    [Description("Whether ambient audio should play from the vending machine.")]
    public bool AmbientAudioEnabled { get; set; } = true;

    [Description("Whether or not probabilities should be additive (50 + 50 = 100) or not (50 + 50 = 2 seperate 50% chances).")]
    public bool AdditiveProbabilities { get; set; } = true;

    // TODO: This should default to any of the basic 'rooms' (not 'hallways') in Entrance Zone
    [Description("The rooms and positions where the vending machine can spawn.")]
    public Dictionary<RoomType, Vector3> SpawnPoints { get; set; } = new();

    [Description("Whether or not card-skimming is enabled (using a keycard for a drink or grenade).")]
    public bool SkimmingEnabled { get; set; } = false;

    [Description("The minimum time between skim attempts in seconds.")]
    public double SkimmingCooldown { get; set; } = 1.0d;

    [Description("The fuse time on the grenade spawned on a bad skim.")]
    public float GrenadeFuseTime { get; set; } = 0.5f;

    [Description("Coin With A String. The ole' Mr. Krabs vending machine hack. Can spawn around the map.")]
    public CoinWithAString CoinWithAString { get; set; } = new();

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
    //  - (Reid) BajaBlast: similar to Jihad from TTT (Gmod) lol - maybe does a lot of SCP damage + friendly fire on or something
}