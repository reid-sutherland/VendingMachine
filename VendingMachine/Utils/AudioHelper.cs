using System.IO;
using System.Collections.Generic;
using Exiled.API.Features;
using MapEditorReborn.API.Features.Objects;
using UnityEngine;

namespace VendingMachine.Utils;

public static class AudioHelper
{
    public static string AudioPath => Path.Combine(Paths.Exiled, "Audio", "SCP294");

    public static void LoadAudioClip(string audioFile)
    {
        string filepath = Path.Combine(AudioPath, audioFile);
        string name = audioFile.Replace(".ogg", "");
        Log.Debug($"-- loading audio clip: {name}");
        if (!AudioClipStorage.LoadClip(filepath, name))
        {
            Log.Error($"Failed to load clip: {filepath}");
        }
    }

    public static void LoadAudioClips(List<string> audioFiles)
    {
        foreach (string file in audioFiles)
        {
            LoadAudioClip(file);
        }
    }

    public static void PlayAudioClip(string playerName, string clip, MapEditorObject model)
    {
        // TODO: When SCP294 starts spawning in random places, we should consider increasing the ambient range to make it easier to find

        clip = clip.Replace(".ogg", "");
        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet(playerName, onIntialCreation: (player) =>
        {
            // Attach created audio player to player.
            player.transform.parent = model.transform;

            // This created speaker will be in 3D space.
            Speaker speaker = player.AddSpeaker("Main", isSpatial: true, minDistance: 1f, maxDistance: 15f);

            // Attach created speaker to player.
            speaker.transform.parent = model.transform;

            // Set local positino to zero to make sure that speaker is in player.
            speaker.transform.localPosition = Vector3.zero;
        });

        Log.Debug($"Adding clip: {clip} to model audio player at position: {audioPlayer.transform.position}");
        audioPlayer.AddClip(clip);
    }
}