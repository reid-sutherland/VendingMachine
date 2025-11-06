using CommonUtils.Core;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Server;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VendingMachine.Drinks;

namespace VendingMachine;

public class Scp294
{
    public static readonly string Identifier = "SCP-294";

    public static readonly string SchematicName = "SCP294";

    public static readonly string ControlPanelName = "ControlPanel";

    public static readonly string DrawerName = "Drawer";

    public string AudioPath => Path.Combine(Paths.Exiled, "Audio", Identifier);

    public static readonly string DispenseEffectAudio = "dispense-drink";

    public static readonly List<string> AmbientAudio = new() { "cod-quick-revive", "cod-speed-cola", "cod-juggernog-soda", "nipper-house-trimmed" };

    public AudioPlayer AudioPlayer { get; set; } = null;

    public uint DrawerCount { get; private set; } = 0;

    private Config Config => MainPlugin.Configs;

    private DateTime LastSkimTime { get; set; } = DateTime.Now;

    private SchematicObject model = null;

    private CoroutineHandle ambientAudioHandle;

    public void OnRoundStarted()
    {
        Log.Debug("Round started: spawning vending machine");
        var spawn = GetRandomSpawnPoint();
        RoomType roomType = spawn.Key;
        Vector3 localPosition = spawn.Value.Item1;
        Vector3 localRotation = spawn.Value.Item2;

        Room room = Room.Get(roomType);
        Vector3 position = room.WorldPosition(localPosition);
        Quaternion rotation = Quaternion.Euler(room.Rotation.eulerAngles + localRotation);
        Log.Debug($"-- randomly selected room: {roomType}");
        Log.Debug($"-- added local position offset {localPosition} to room position {room.Position} => {position}");
        Log.Debug($"-- added local rotation offset {localRotation} to room euler rotation {room.Rotation.eulerAngles} => {rotation}");

        try
        {
            model = ObjectSpawner.SpawnSchematic(SchematicName, position, rotation);
            if (model is null)
            {
                Log.Error($"Model with schematic name {SchematicName} failed to spawn!");
                return;
            }

            Log.Info($"Vending machine spawned in room: {room}");
            Log.Debug($"-- model position: {model.Position} - model euler rotation: {model.Rotation.eulerAngles}");

            // Create the AudioPlayer for ambient music and sound effects
            AudioPlayer = AudioHelper.GetAudioPlayer(
                $"{Identifier}-AP",
                parent: model.gameObject,
                speakerVolume: 1.0f,
                speakerCount: 1,
                minDistance: 3.0f,
                maxDistance: 30.0f,
                log: Config.AudioDebug
            );
            if (AudioPlayer is not null)
            {
                Log.Info($"Created audio player for spawned {Identifier} with name: {AudioPlayer.Name}");

                if (Config.AmbientAudioEnabled)
                {
                    // Play ambient music indefinitely
                    ambientAudioHandle = Timing.RunCoroutine(AmbientAudioCoroutine());
                }
            }
            else
            {
                Log.Error($"Failed to create audio player for spawned {Identifier}");
            }

            // Register events to the AMERT Interactable Objects - delay to make sure they load first
            Timing.CallDelayed(2.0f, () =>
            {
                Log.Debug($"Registering handlers to AMERT IOs");
                foreach (var io in AdvancedMERTools.AdvancedMERTools.Singleton.InteractableObjects)
                {
                    Log.Debug($"-- found AMERT InteractableObject: {io.gameObject.name}");
                    if (io.gameObject.name.Contains(ControlPanelName))
                    {
                        io.PlayerIOInteracted += OnControlPanelInteracted;
                    }
                    else if (io.gameObject.name.Contains(DrawerName))
                    {
                        io.PlayerIOInteracted += OnDrawerInteracted;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error($"Exception occurred while trying to spawn the vending machine in room: {spawn.Key}");
            Log.Error(ex.Message);
            Log.Debug(ex.StackTrace);
        }
    }

    public void OnRoundEnded(RoundEndedEventArgs ev)
    {
        Log.Debug("Round ended");
        // clear AMERT IO events
        foreach (var io in AdvancedMERTools.AdvancedMERTools.Singleton.InteractableObjects)
        {
            if (io.gameObject.name.Contains(ControlPanelName))
            {
                io.PlayerIOInteracted -= OnControlPanelInteracted;
            }
            else if (io.gameObject.name.Contains(DrawerName))
            {
                io.PlayerIOInteracted -= OnDrawerInteracted;
            }
        }
        // clean up vending machine
        Timing.KillCoroutines(ambientAudioHandle);
        model?.Destroy();
        model = null;
    }

    public void OnRestartingRound()
    {
        Log.Debug("Restarting round");
        // clear AMERT IO events
        foreach (var io in AdvancedMERTools.AdvancedMERTools.Singleton.InteractableObjects)
        {
            if (io.gameObject.name.Contains(ControlPanelName))
            {
                io.PlayerIOInteracted -= OnControlPanelInteracted;
            }
            else if (io.gameObject.name.Contains(DrawerName))
            {
                io.PlayerIOInteracted -= OnDrawerInteracted;
            }
        }
        // clean up vending machine
        Timing.KillCoroutines(ambientAudioHandle);
        model?.Destroy();
        model = null;
    }

    public void OnControlPanelInteracted(AdvancedMERTools.InteractableObject.PlayerIOInteractedEventArgs ev)
    {
        if (model is null)
        {
            return;
        }
        Player player = ev.Player;

        try
        {
            Log.Debug($"Player {player.Nickname} interacted with the VM control panel");
            if (player.IsScp)
            {
                Log.Debug("-- player was SCP");
            }
            else if (player.CurrentItem is null)
            {
                Log.Debug("-- player was holding nothing");
            }
            else if (Config.SkimmingEnabled && player.CurrentItem.IsKeycard && player.CurrentItem.Type != ItemType.KeycardGuard)
            {
                // keycard skimming
                DateTime now = DateTime.Now;
                if (now - LastSkimTime >= TimeSpan.FromSeconds(Config.SkimmingCooldown))
                {
                    int rand = MainPlugin.Random.Next(2);
                    if (rand > 0)
                    {
                        // free drink :)
                        Log.Debug($"-- player was holding keycard: rand={rand} - dispensing drink");
                        DispenseDrink();
                    }
                    else
                    {
                        // spawn a grenade
                        Log.Debug($"-- player was holding keycard: rand={rand} - grenade time");
                        // TODO: Figure out how to make this "belong" to the player so it can't friendly fire
                        var grenadeItem = (Exiled.API.Features.Items.ExplosiveGrenade)Exiled.API.Features.Items.Item.Create(ItemType.GrenadeHE);
                        grenadeItem.FuseTime = 0.5f;
                        grenadeItem.FuseTime = Config.GrenadeFuseTime;
                        var grenade = grenadeItem.SpawnActive(player.Position);
                    }
                }
                else
                {
                    Log.Debug("-- skimming is on cooldown");
                }
                LastSkimTime = now;
            }
            else if (player.CurrentItem.Type != ItemType.Coin)
            {
                Log.Debug($"-- player was NOT holding a coin");
            }
            else
            {
                bool removeCoin = true;
                if (Config.CoinWithAString.Check(player))
                {
                    Log.Debug($"-- player was holding a CWAS - dispensing drink");
                    bool check = CustomItem.TryGet(player, out CustomItem customItem);
                    if (check)
                    {
                        CoinWithAString coin = (CoinWithAString)customItem;
                        removeCoin = coin.Use(player);
                    }
                }
                else
                {
                    Log.Debug($"-- player was holding a regular coin - dispensing drink");
                }
                if (removeCoin)
                {
                    player.RemoveItem(player.CurrentItem);
                }

                DispenseDrink();
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(OnControlPanelInteracted)} Exception: {ex}");
        }
    }

    public void OnDrawerInteracted(AdvancedMERTools.InteractableObject.PlayerIOInteractedEventArgs ev)
    {
        if (model is null)
        {
            return;
        }
        Player player = ev.Player;

        try
        {
            Log.Debug($"Player interacted with the vending machine drawer");
            if (player.IsScp)
            {
                Log.Debug("-- player was SCP");
                return;
            }
            if (DrawerCount == 0)
            {
                Log.Debug("-- drawer is empty");
                return;
            }
            if (player.IsInventoryFull)
            {
                Log.Debug("-- player's inventory is full");
                return;
            }

            GiveRandomDrink(player);
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(OnDrawerInteracted)} Exception: {ex}");
        }
    }

    private void DispenseDrink()
    {
        DrawerCount++;
        if (AudioPlayer is not null)
        {
            AudioPlayer.AddClip(DispenseEffectAudio);
        }
    }

    private bool GiveRandomDrink(Player player)
    {
        if (DrawerCount == 0)
        {
            return false;
        }

        bool success = false;
        int roll = ChanceHelper.RollChance(MainPlugin.CustomDrinkList, additiveProbabilities: Config.AdditiveProbabilities);
        Log.Debug($"-- random drink roll: {roll}");
        foreach (CustomDrink randomDrink in MainPlugin.CustomDrinkList)
        {
            Log.Debug($"-- current roll: {roll} with chance: {randomDrink.Chance} for drink: {randomDrink.Name}", print: Config.RollDebug);
            if (roll <= randomDrink.Chance)
            {
                Log.Info($"Giving random drink {randomDrink.Name} to player: {player.Nickname}");
                randomDrink.Give(player);
                success = true;
                break;
            }
            if (Config.AdditiveProbabilities)
            {
                roll -= randomDrink.Chance;
            }
        }

        if (success)
        {
            // TODO: Play a sound for the drawer
            DrawerCount--;
            Log.Debug($"Drinks left in the drawer: {DrawerCount}");
        }
        else
        {
            Log.Error("Failed to get random drink :(");
        }
        return success;
    }

    private IEnumerator<float> AmbientAudioCoroutine()
    {
        for (; ; )
        {
            if (AudioPlayer is not null && AmbientAudio.TryGetRandomItem(out string clip))
            {
                AudioPlayer.AddClip(clip);
                Log.Debug($"AmbientAudioCoroutine: Added random ambient audio clip '{clip}' to {AudioPlayer.Name}", print: Config.AudioDebug);
            }
            else
            {
                Log.Debug($"AmbientAudioCoroutine: Failed to get random ambient audio clip or AudioPlayer is null", print: Config.AudioDebug);
            }

            // Note: Each song is currently about 30s
            int waitTime = MainPlugin.Random.Next(60, 120);
            Log.Debug($"AmbientAudioCoroutine: Waiting {waitTime} seconds before playing next ambient audio clip", print: Config.AudioDebug);
            yield return Timing.WaitForSeconds(waitTime);
        }
    }

    private KeyValuePair<RoomType, Tuple<Vector3, Vector3>> GetRandomSpawnPoint()
    {
        if (SpawnPoints.Count == 0)
        {
            throw new Exception("No SpawnPoints were defined for vending machine");
        }
        foreach (var kvp in SpawnPoints.OrderBy(_ => MainPlugin.Random.Next()))
        {
            if (Room.Get(kvp.Key) is not null)
            {
                return kvp;
            }
            else
            {
                Log.Warn($"SpawnPoint with RoomType {kvp.Key} was selected but it does not exist on the map");
            }
        }
        throw new Exception("None of the SpawnPoints for vending machine exist on the map");
    }

#pragma warning disable SA1025  // Code should not contain multiple whitespace characters in a row.
    // TODO: Move this to Config.SpawnPoints
    // Tuple values are <offset, rotation> where rotation is the euler angles of the Quaternion
    private static readonly Dictionary<RoomType, Tuple<Vector3, Vector3>> SpawnPoints = new()
    {
        [RoomType.EzIntercom] =             Tuple.Create(new Vector3(-0.176f, 0.547f, -4.437f), new Vector3(0.0f, -90.0f, 0.0f)),
        //[RoomType.EzCurve] =                Tuple.Create(new Vector3(0.894f, 0.565f, 1.638f), new Vector3(0.0f, 225.0f, 0.0f)),    // this works when room rotation is (0, 0, 0, -1)
        //[RoomType.EzCheckpointHallwayA] =   Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzCheckpointHallwayB] =   Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzConference] =           Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzPcs] =                  Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzDownstairsPcs] =        Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzUpstairsPcs] =          Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzSmallrooms] =           Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
        //[RoomType.EzGateA] =                Tuple.Create(new Vector3(2.834f, 0.553f, -3.926f), new Vector3(0.0f, 90.0f, 0.0f)),
        //[RoomType.EzGateB] =                Tuple.Create(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f)),
    };
#pragma warning restore SA1025
}