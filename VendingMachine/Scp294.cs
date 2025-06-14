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
using System.Reflection;
using UnityEngine;

using VendingMachine.Drinks;
using VendingMachine.Utils;

namespace VendingMachine;

public class Scp294
{
    private SchematicObject model = null;

    public static readonly string SchematicName = "SCP294";

    public const string ControlPanelName = "SCP294ControlPanel";

    public const string DrawerName = "SCP294Drawer";

    public string AudioPath => Path.Combine(Paths.Exiled, "Audio", "SCP294");

    public string AudioPlayerName => GetType().Name;

    public static readonly string AudioDispenseEffect = "DispenseDrink.ogg";

    public static readonly List<string> AudioAmbient = new() { "cod-quick-revive.ogg", "cod-speed-cola.ogg", "cod-juggernog-soda.ogg", "nipper-house-trimmed.ogg" };

    public uint DrawerCount { get; private set; } = 0;

    private DateTime LastSkimTime { get; set; } = DateTime.Now;

    private CoroutineHandle ambientAudioHandle;

    public void OnRoundStarted()
    {
        Log.Debug("Round started: spawning vending machine");
        var spawn = GetRandomSpawnPoint();
        RoomType roomType = spawn.Key;
        Vector3 positionOffset = spawn.Value.Item1;
        Vector3 rotationOffset = spawn.Value.Item2;
        Log.Debug($"Randomly selected room: {roomType}");
        try
        {
            Room room = Room.Get(roomType);
            Log.Debug($"Adding local position offset {positionOffset} to room position: {room.Position}");
            Vector3 position = room.WorldPosition(positionOffset);
            Log.Debug($"Adding local rotation offset {rotationOffset} to room euler rotation: {room.Rotation.eulerAngles}");
            Quaternion rotation = Quaternion.Euler(room.Rotation.eulerAngles + rotationOffset);

            model = ObjectSpawner.SpawnSchematic(SchematicName, position, rotation);
            if (model is not null)
            {
                Log.Info($"Vending machine spawned in room: {room}");
                Log.Debug($"-- model position: {model.Position} - model euler rotation: {model.Rotation.eulerAngles}");

                // Play ambient music indefinitely
                ambientAudioHandle = Timing.RunCoroutine(AmbientAudioCoroutine());
            }
            else
            {
                Log.Error($"Model with schematic name {SchematicName} failed to spawn!");
            }

            // Register events to the AMERT Interactable Objects - delay to make sure they load firstwd
            Timing.CallDelayed(2.0f, () =>
            {
                Log.Debug($"Registering handlers to AMERT IOs");
                foreach (var io in AdvancedMERTools.AdvancedMERTools.Singleton.InteractableObjects)
                {
                    Log.Debug($"-- found AMERT InteractableObject: {io.gameObject.name}");
                    if (io.gameObject.name == "SCP294ControlPanel")
                    {
                        io.PlayerIOInteracted += OnControlPanelInteracted;
                    }
                    else if (io.gameObject.name == "SCP294Drawer")
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
            if (io.gameObject.name == "SCP294ControlPanel")
            {
                io.PlayerIOInteracted -= OnControlPanelInteracted;
            }
            else if (io.gameObject.name == "SCP294Drawer")
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
            if (io.gameObject.name == "SCP294ControlPanel")
            {
                io.PlayerIOInteracted -= OnControlPanelInteracted;
            }
            else if (io.gameObject.name == "SCP294Drawer")
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
            else if (MainPlugin.Configs.SkimmingEnabled && player.CurrentItem.IsKeycard && player.CurrentItem.Type != ItemType.KeycardGuard)
            {
                DateTime now = DateTime.Now;
                if (now - LastSkimTime >= TimeSpan.FromSeconds(MainPlugin.Configs.SkimmingCooldown))
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
                        grenadeItem.FuseTime = MainPlugin.Configs.GrenadeFuseTime;
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
                if (MainPlugin.Configs.CoinWithAString.Check(player))
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
            Log.Error($"Oh fuck there's an exception: {ex}");
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
            if (player.IsInventoryFull)
            {
                Log.Debug("-- player's inventory is full");
                return;
            }
            if (DrawerCount == 0)
            {
                Log.Debug("-- drawer is empty");
                return;
            }

            GiveRandomDrink(player);
        }
        catch (Exception ex)
        {
            Log.Error($"Oh fuck there's an exception: {ex}");
        }
    }

    private void DispenseDrink()
    {
        DrawerCount++;
        AudioHelper.PlayAudioClip(AudioPlayerName, AudioDispenseEffect, model.gameObject);
    }

    private bool GiveRandomDrink(Player player)
    {
        if (DrawerCount == 0)
        {
            return false;
        }

        Log.Info($"Giving random drink to player: {player.Nickname}");
        int roll = RollHelper.RollChanceFromConfig(MainPlugin.Configs);
        if (MainPlugin.Configs.RollDebug)
        {
            Log.Debug($"-- true roll: {roll}");
        }

        bool success = false;
        foreach (PropertyInfo pInfo in MainPlugin.Configs.GetType().GetProperties())
        {
            if (typeof(CustomDrink).IsAssignableFrom(pInfo.PropertyType))
            {
                var randomDrink = pInfo.GetValue(MainPlugin.Configs) as CustomDrink;
                if (randomDrink is not null)
                {
                    if (MainPlugin.Configs.RollDebug)
                    {
                        Log.Debug($"-- current roll: {roll} - current chance: {randomDrink.Chance} for drink: {randomDrink.Name}");
                    }
                    if (roll <= randomDrink.Chance)
                    {
                        if (MainPlugin.Configs.RollDebug)
                        {
                            Log.Debug($"-- success: giving {randomDrink.Name} to player: {player.Nickname}");
                        }
                        randomDrink.Give(player);
                        success = true;
                        break;
                    }

                    if (MainPlugin.Configs.AdditiveProbabilities)
                    {
                        roll -= randomDrink.Chance;
                    }
                }
            }
        }
        if (success)
        {
            // TODO: Play a sound for the drawer
            DrawerCount--;
            Log.Debug($"-- {DrawerCount} drinks left in the drawer");
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
            int rand = MainPlugin.Random.Next(AudioAmbient.Count);
            string clip = AudioAmbient[rand];
            AudioHelper.PlayAudioClip(AudioPlayerName, clip, model.gameObject);

            // Note: Each song is currently about 30s
            int waitTime = MainPlugin.Random.Next(60, 120);
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

    // TODO: Move this to Config.SpawnPoints
    // Tuple values are <offset, rotation> where rotation is the euler angles of the Quaternion
    private static readonly Dictionary<RoomType, Tuple<Vector3, Vector3>> SpawnPoints = new()
    {
#pragma warning disable SA1025  // Code should not contain multiple whitespace characters in a row.
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
#pragma warning restore SA1025
    };
}