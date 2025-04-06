using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
using UnityEngine;
using VendingMachine.Drinks;
using VendingMachine.Utils;

namespace VendingMachine;

public class Scp294
{
    public Scp294()
    {
        Log.Debug($"Loading audio clips from directory: {AudioHelper.AudioPath}");
        AudioHelper.LoadAudioClip(audioDispenseEffect);
        AudioHelper.LoadAudioClips(audioAmbient);
    }

    private MapEditorObject model;

    public Vector3 WorldPosition { get; private set; } = Vector3.zero;

    public Quaternion WorldRotation { get; private set; } = Quaternion.identity;

    public static readonly string SchematicName = "SCP294";

    public const string ControlPanelName = "SCP294ControlPanel";

    public const string DrawerName = "SCP294Drawer";

    public int DrawerCount { get; private set; } = 0;

    public string AudioPlayerName => GetType().Name;

    private readonly string audioDispenseEffect = "DispenseDrink.ogg";
    private readonly List<string> audioAmbient = new() { "song1.ogg", "song2.ogg", "song3.ogg" };

    private CoroutineHandle coroutineHandle;

    public void OnRoundStarted()
    {
        Log.Debug("Round started: spawning vending machine");

        int maxTries = 5;
        for (int i = 0; i < maxTries; i++)
        {
            var spawn = GetRandomSpawnPoint();
            try
            {
                Room room = Room.Get(spawn.Key);
                Vector3 offset = spawn.Value.Item1;
                Vector3 eulerRotation = spawn.Value.Item2;
                Log.Debug($"Randomly selected room: {spawn.Key} - offset: {offset} - euler rotation: {eulerRotation}");

                eulerRotation += room.Rotation.eulerAngles;
                Log.Debug($"Adding room rotation ({room.Rotation.eulerAngles}) to rotation offset (above) -> {eulerRotation}");

                Vector3 position = room.WorldPosition(offset);
                model = ObjectSpawner.SpawnSchematic(
                    SchematicName,
                    position,
                    Quaternion.Euler(eulerRotation),
                    null,
                    MapUtils.GetSchematicDataByName(SchematicName)
                );
                if (model is not null)
                {
                    WorldPosition = model.Position;
                    Log.Info($"Vending machine spawned in room: {model.CurrentRoom}");
                    Log.Debug($"-- room position: {room.Position} - room euler rotation: {room.Rotation.eulerAngles}");
                    Log.Debug($"-- model position: {model.Position} - model euler rotation: {model.Rotation.eulerAngles}");

                    // Play ambient music indefinitely
                    coroutineHandle = Timing.RunCoroutine(AmbientAudioCoroutine());
                }
                else
                {
                    Log.Error($"Model with schematic name {SchematicName} was not found!");
                }
                return;
            }
            catch (Exception ex)
            {
                Log.Warn($"Random room: {spawn.Key} does not exist, trying new room");
                Log.Debug(ex);
            }
        }
        Log.Error($"No valid rooms after {maxTries} tries, aborting spawn");
    }

    public void OnRoundEnded(RoundEndedEventArgs ev)
    {
        Log.Debug("Round ended");
        Timing.KillCoroutines(coroutineHandle);
        model?.Destroy();
        model = null;
    }

    public void OnControlPanelInteracted(Player player)
    {
        if (model is null)
        {
            return;
        }

        try
        {
            Log.Debug($"Player {player.Nickname} interacted with the vending machine control panel");
            if (player.IsScp)
            {
                Log.Debug("Player was SCP");
                return;
            }
            if (player.CurrentItem is null || player.CurrentItem.Type != ItemType.Coin)
            {
                Log.Debug($"Player was NOT holding a coin");
                return;
            }

            player.RemoveItem(player.CurrentItem);
            DrawerCount++;

            AudioHelper.PlayAudioClip(AudioPlayerName, audioDispenseEffect, model);
        }
        catch (Exception ex)
        {
            Log.Error($"Oh fuck there's an exception: {ex}");
        }
    }

    public void OnDrawerInteracted(Player player)
    {
        if (model is null)
        {
            return;
        }

        try
        {
            Log.Debug($"Player interacted with the vending machine drawer");
            if (player.IsScp)
            {
                Log.Debug("Player was SCP");
                return;
            }
            if (DrawerCount == 0)
            {
                Log.Debug("Drawer is empty");
                return;
            }
            if (player.IsInventoryFull)
            {
                Log.Debug("Player's inventory is full");
                return;
            }

            Log.Debug($"{DrawerCount} drinks in the drawer, dispensing");
            bool success = GetRandomDrink(out CustomDrink randomDrink);
            if (success)
            {
                Log.Info($"Dispensing random drink: {randomDrink.Name} to player: {player.Nickname}");
                randomDrink.Give(player);
                DrawerCount--;

                // TODO: Play a sound for the drawer
            }
            else
            {
                Log.Error("Failed to get random drink :(");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Oh fuck there's an exception: {ex}");
        }
    }

    private bool GetRandomDrink(out CustomDrink outDrink)
    {
        int roll = RollHelper.RollChanceFromConfig(MainPlugin.Configs);
        Log.Debug($"GetRandomDrink(): rolled: {roll}");

        outDrink = null;
        foreach (PropertyInfo pInfo in MainPlugin.Configs.GetType().GetProperties())
        {
            if (typeof(CustomDrink).IsAssignableFrom(pInfo.PropertyType))
            {
                var drink = pInfo.GetValue(MainPlugin.Configs) as CustomDrink;
                if (drink is not null)
                {
                    Log.Debug($"-- current roll: {roll} - current chance: {drink.Chance} for drink: {drink.Name}");
                    if (roll <= drink.Chance)
                    {
                        outDrink = drink;
                        return true;
                    }

                    if (MainPlugin.Configs.AdditiveProbabilities)
                    {
                        roll -= drink.Chance;
                    }
                }
            }
        }
        return false;
    }

    private KeyValuePair<RoomType, Tuple<Vector3, Vector3>> GetRandomSpawnPoint()
    {
        List<RoomType> roomTypes = SpawnPoints.Keys.ToList();
        RoomType room = roomTypes[MainPlugin.Random.Next(roomTypes.Count)];
        return SpawnPoints.First(kvp => kvp.Key == room);
    }

    // TODO: Move this to Config.SpawnPoints
    // Tuple values are <offset, rotation> where rotation is the euler angles of the Quaternion
    private static readonly Dictionary<RoomType, Tuple<Vector3, Vector3>> SpawnPoints = new()
    {
        //[RoomType.EzCurve] = Tuple.Create(new Vector3(0.894f, 0.565f, 1.638f), new Vector3(0.0f, 225.0f, 0.0f)),    // this works when room rotation is (0, 0, 0, -1)
        [RoomType.EzIntercom] = Tuple.Create(new Vector3(-0.176f, 0.547f, -4.437f), new Vector3(0.0f, -90.0f, 0.0f)),

        //[RoomType.EzCheckpointHallwayA] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzCheckpointHallwayB] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzConference] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzPcs] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzDownstairsPcs] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzUpstairsPcs] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzSmallrooms] = new Vector3(0.0f, 0.0f, 0.0f),
        //[RoomType.EzGateA] = Tuple.Create(new Vector3(2.834f, 0.553f, -3.926f), Quaternion.Euler(0.0f, 90f, 0.0f)),
        //[RoomType.EzGateB] = new Vector3(0.0f, 0.0f, 0.0f),
    };

    private IEnumerator<float> AmbientAudioCoroutine()
    {
        for (;;)
        {
            int rand = MainPlugin.Random.Next(audioAmbient.Count);
            string clip = audioAmbient[rand];
            AudioHelper.PlayAudioClip(AudioPlayerName, clip, model);

            // Note: Each song is currently about 30s
            int waitTime = MainPlugin.Random.Next(60, 120);
            Log.Debug($"Waiting {waitTime} seconds before playing next ambient audio");
            yield return Timing.WaitForSeconds(waitTime);
        }
    }
}