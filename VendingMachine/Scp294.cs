using System.Linq;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
//using Scp559.Utilities.Components;
//using Scp559.Utilities.Voice;
using UnityEngine;
using Random = System.Random;
using Exiled.Events.EventArgs.Server;

namespace Scp294;

public class Scp294
{
    public Scp294()
    {
    }

    private MapEditorObject model;

    public void OnUsedItem(UsedItemEventArgs ev)
    {
        if (ev.Item.Type is not ItemType.Coin)
            return;

        // TODO: this needs to be Interacted or something instead, but where we can grab what the player is holding when they interact
    }

    public void OnInteracted(InteractedEventArgs ev)
    {
        var coin = ev.Player.Items.FirstOrDefault(item => item.Type == ItemType.Coin);
        if (coin is null)
            return;

        ev.Player.Items.Contains(Exiled.API.Features.Items)
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

    public void OnRoundStart()
    {
        Log.Debug("Round started with SCP-294");
        Timing.RunCoroutine(CakeSpawnHandler());
        Timing.RunCoroutine(SlicePickupIndicator());
    }

    public void OnEndingRound(EndingRoundEventArgs ev)
    {
        model = null;
    }

    private IEnumerator<float> CakeSpawnHandler()
    {
        //yield return Timing.WaitForSeconds(_entryPoint.Config.CakeConfig.FirstCakeSpawnDelay);
        //Debug.Log("spawning cake in " + _entryPoint.Config.CakeConfig.FirstCakeSpawnDelay + " seconds");
        while (true)
        {
            if (Round.IsEnded)
                yield break;

            Room room = Room.Get(GetRandomRoom());
            //Vector3 spawnPoint = _entryPoint.Config.CakeConfig.SpawnPoints[room.Type] + Vector3.down * 1.8f;

            yield return Timing.WaitForSeconds(5f);

            //model = ObjectSpawner.SpawnSchematic(_entryPoint.Config.CakeConfig.SchematicName, room.WorldPosition(spawnPoint), null, null, MapUtils.GetSchematicDataByName(_entryPoint.Config.CakeConfig.SchematicName));
            Debug.Log("cake spawned in:" + model.Position + model.CurrentRoom);
            ServerConsole.AddLog("cake spawned in:" + model.Position + model.CurrentRoom);

            //yield return Timing.WaitForSeconds(_entryPoint.Config.CakeConfig.DisappearDelay);

            model.Destroy();
            model = null;

            ServerConsole.AddLog("cake despawned");

            //yield return Timing.WaitForSeconds(_entryPoint.Config.CakeConfig.NormalSpawnDelay);
            Debug.Log("cake respawning");
        }
    }

    private IEnumerator<float> SlicePickupIndicator()
    {
        while (true)
        {
            if (Round.IsEnded)
                yield break;

            foreach (Player player in Player.List)
            {
                if (model is not null && !player.IsScp && Vector3.Distance(player.Position, model.Position) <= 2.5f)
                {
                    //player.ShowHint(_entryPoint.Config.CakeConfig.SlicePickupHint, 1.1f);
                }
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }

    private RoomType GetRandomRoom()
    {
        Random random = new();
        List<RoomType> roomNames = MainPlugin.Configs.SpawnPoints.Keys.ToList();
        return roomNames[random.Next(roomNames.Count)];
    }
}