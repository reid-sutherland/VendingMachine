using System.Linq;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using UnityEngine;
using Random = System.Random;
using InventorySystem.Items.Pickups;

namespace VendingMachine;

public class Scp294
{
    public static readonly string SchematicName = "SCP-249";

    public Vector3 WorldPosition { get; private set; } = Vector3.zero;

    private MapEditorObject model;

    public Scp294()
    {
    }

    // We're cheesing the SearchingPickup Event so that the vending machine technically behaves like an item you can pickup..
    // .. but with the illusion of it just being interacted with, hence the name.
    public void OnInteracted(SearchingPickupEventArgs ev)
    {
        Player player = ev.Player;
        if (ev.Pickup.GameObject.name.Equals("SCP-294"))
        {
            if (player.CurrentItem.Type != ItemType.Coin)
            {
                return;
            }

            player.RemoveItem(player.CurrentItem);

            // TODO: Add some sounds

            // TODO: need to use the rotation to make sure it always drops in "front"
            Vector3 dropPosition = model.Position + (Vector3.one * 0.25f);
            Log.Debug($"- Player position: {player.Position}");
            Log.Debug($"- Machine position: {model.Position}");
            Log.Debug($"- Drop position: {dropPosition}");

            // TODO: pick a random drink by chance from the config, instead of just SCP-207
            //      - look at how CommonUtils picks random things
            //Random random = new();

            ItemType itemType = ItemType.SCP207;
            PickupSyncInfo info = new(itemType, 1.0f, 6969);
            //var ipb = InventoryExtensions.ServerCreatePickup(Base, info, position, rotation ?? Quaternion.identity);
            //Base.OnRemoved(ipb);
            //ipb.State = Base._primed ? Scp244State.Active : Scp244State.Idle;
            //Pickup pickup = Pickup.Get(ipb);
            //if (spawn)
            //    pickup.Spawn();
        }
    }

    public void OnRoundStart()
    {
        Log.Debug("Round started: spawning vending machine");

        var spawn = GetRandomSpawnPoint();
        Room room = Room.Get(spawn.Key);
        Vector3 offset = spawn.Value + (Vector3.up * 1.0f);
        Log.Debug($"Randomly spawning in room: {room.RoomName} - with offset: {offset}");

        WorldPosition = room.WorldPosition(offset);
        model = ObjectSpawner.SpawnSchematic(
            SchematicName,
            WorldPosition,
            null,
            null,
            MapUtils.GetSchematicDataByName(SchematicName)
        );
        Log.Debug($"Vending machine spanwed in room: {model.CurrentRoom} - at position: {model.Position}");
    }

    public void OnEndingRound(EndingRoundEventArgs ev)
    {
        //model.Destroy();
        model = null;
    }

    public void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        // TODO: It'd be funny to add a Helium soda that just makes your voice high pitched lol

        //if (!ev.Player.GameObject.TryGetComponent(out Scp559SizeEffect _))
        //    return;
        //ev.VoiceMessage = VoicePitchUtilities.SetVoicePitch(ev.VoiceMessage);
    }

    public void OnDying(DyingEventArgs ev)
    {
        // TODO: Use this example for adding custom new sodas

        //if (!ev.Player.GameObject.TryGetComponent(out Scp559SizeEffect scp559Effect))
        //    return;

        //Object.Destroy(scp559Effect);
        //ev.Player.Scale = Vector3.one;
    }

    private KeyValuePair<RoomType, Vector3> GetRandomSpawnPoint()
    {
        Random random = new();
        List<RoomType> roomTypes = SpawnPoints.Keys.ToList();
        RoomType room = SpawnPoints.Keys.ToList()[random.Next(roomTypes.Count)];
        return SpawnPoints.First(kvp => kvp.Key == room);
    }

    // TODO: Make this configurable
    public static readonly Dictionary<RoomType, Vector3> SpawnPoints = new()
    {
        //[RoomType.LczClassDSpawn] = new Vector4(-19f, 0f, 0f, 1f),
        //[RoomType.LczToilets] = new Vector4(-5.3f, 0.95f, -6.5f, 0f),
        //[RoomType.LczGreenhouse] = new Vector4(0f, 0f, -5.5f, 1f),
        //[RoomType.LczComputerRoom] = new Vector4(6f, 0f, 4.5f, 1f),
        //[RoomType.LczGlassroom] = new Vector4(8.3f, 1.05f, -5.9f, 0f),
        //[RoomType.LczArmory] = new Vector4(4.14f, 0.82f, -0.95f, 0f),
        //[RoomType.Lcz330] = new Vector4(2f, 0.91f, 0.7f, 0f),
        //[RoomType.Lcz173] = new Vector4(-2.56f, 12.32f, -4.67f, 0f),
        //[RoomType.Lcz914] = new Vector4(1.1f, 1.011f, -7.16f, 0f),
        //[RoomType.LczAirlock] = new Vector4(0.65f, 0f, -4.6f, 1f),
        //[RoomType.HczServers] = new Vector4(2f, 0f, -0.35f, 1f),
        //[RoomType.HczWarhead] = new Vector4(0.65f, 291.89f, 10.51f, 0f),
        //[RoomType.HczArmory] = new Vector4(0.89f, 0.88f, -1.44f, 0f),
        //[RoomType.HczMicroHID] = new Vector4(2.3f, 0.92f, -5.27f, 0f),
        //[RoomType.Hcz049] = new Vector4(-5.65f, 192.44f, -1.75f, 1f),
        //[RoomType.Hcz079] = new Vector4(1.8f, -3.33f, -6.3f, 1f),
        //[RoomType.Hcz096] = new Vector4(-4.4f, 0f, 1f, 1f),
        //[RoomType.Hcz106] = new Vector4(24.38f, 0.86f, -15.54f, 0f),
        //[RoomType.Hcz939] = new Vector4(0.53f, 1.05f, 2.9f, 0f),
        //[RoomType.HczTestroom] = new Vector4(0.915f, 0.74f, -4.7f, 0f),
        //[RoomType.HczCheckpointToEntranceZone] = new Vector4(-6.07f, 0f, -3.35f, 1f),
        [RoomType.EzOfficeStoried] = new Vector3(-2.26f, 0.89f, 0.4f),
        [RoomType.EzOfficeLarge] = new Vector3(1.14f, 0.89f, 0.29f),
        [RoomType.EzOfficeSmall] = new Vector3(6f, -0.54f, 2.7f),
        [RoomType.EzIntercom] = new Vector3(-6.9f, -5.04f, -2.8f),
        [RoomType.EzGateA] = new Vector3(2.9f, 0.91f, 1.19f),
        [RoomType.EzGateB] = new Vector3(3.38f, 0.91f, -2.8f),
    };
}