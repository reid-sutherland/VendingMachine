# Vending Machine

This plugin creates an in-game SCP-294 (VendingMachine) device that offers a number of CustomItem drinks in exchange for Coins.

This plugin is still a work in progress, it is very usable for my own server but is not yet fully configurable for any potential users. The assets for the VendingMachine schematic are also currently not on this repo but feel free to DM me on discord.

## Requirements/Dependencies

- [ProjectMER](https://github.com/Michal78900/ProjectMER): For loading the MER (MapEditorReborn) schematic in-game, created with Unity Editor.
- [AdvancedMERTools](https://github.com/reid-sutherland/AdvancedMERtools): For interacting with the schematic's Unity object.
- [AudioPlayerApi](https://github.com/Killers0992/AudioPlayerApi): For the ambient audio and sound effects.

## Features

### Vending machine

The plugin spawns an interactable SCP-294 (VendingMachine) device where custom drinks can be acquired at the cost of coins.
- By default this spawns next to Intercom in Entrance Zone. In the future I would like to enable more configurable spawn points.
- To use:
  - While holding a coin, interact with the Control Panel to put the coin into the machine and dispense a random drink to the Drawer.
  - Then, interact with the Drawer to grab the drink.
  - Don't have a coin? Who knows, maybe somebody left a drink in there!
- Also plays ambient music from the machine and has sound effects.

### Custom Drinks

The machine will randomly dispense drinks from the base game (SCP-207 and Anti SCP-207) as well as my own CustomItem drinks:
- SCP-207: Base game. Drains health but gives a considerable speed boost while moving.
- Anti SCP-207: Base game. Gives AP (green health bar that ticks down) but slows speed while moving.
- Helium Hatch: Affects the voice of the player to create a chipmunk-voice effect for a long period of time.
- Poopsi: Drops Peanut filth (SCP-173 Tantrum ability) at a regular interval for a short time, but gives the player extra AP each time.
- Old Man Rye: Allows the player to walk through doors for a time (SCP-106 passive ability).
- B.A.R.Q.S.: The Random Rootbeer. Gives random status effect to player for a time. Good effects have a higher chance but some bad effects can also be given.
- McDonald's Sprite: Makes the player chilly for a time but gives an invisible HUME shield to offset the damage from the chill effect.
- Panera Lemonade: Provides an incredible speed boost to the player for a short time. But beware, there is a 50/50 chance of cardiac arrest when the effect ends.
- Gobby Pop: Gives RainbowTaste (negative status effect resistance). Special effect applies for configured gobby-equivalents (trouble makers who deserve some silliness).

Most of these effects (their durations, intensities, chances, etc.) are configurable.

### Other Items

- Coin With A String: A regular coin but with a string attached, for cheating the vending machine out of free drinks! However, the string is not very strong and will break after a few uses.
