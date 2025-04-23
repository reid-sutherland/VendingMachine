using System.IO;
using System.Collections.Generic;
using Exiled.API.Features;
using MapEditorReborn.API.Features.Objects;
using UnityEngine;

namespace VendingMachine.Utils;

public static class AudioHelper
{
    public static void LoadAudioClip(string audioDir, string audioFile)
    {
        string filepath = Path.Combine(audioDir, audioFile);
        string name = audioFile.Replace(".ogg", "");
        if (MainPlugin.Configs.AudioDebug)
        {
            Log.Debug($"-- loading audio clip: {name}");
        }
        if (!AudioClipStorage.LoadClip(filepath, name))
        {
            Log.Error($"Failed to load clip: {filepath}");
        }
    }

    public static void LoadAudioClips(string audioDir, List<string> audioFiles)
    {
        foreach (string file in audioFiles)
        {
            LoadAudioClip(audioDir, file);
        }
    }

    public static void PlayAudioClip(string audioPlayerName, string clip, MapEditorObject model)
    {
        // TODO: When SCP294 starts spawning in random places, we should consider increasing the ambient range to make it easier to find

        clip = clip.Replace(".ogg", "");
        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet(audioPlayerName, onIntialCreation: (audioPlayer) =>
        {
            // Attach created audio player to player.
            audioPlayer.transform.parent = model.transform;

            // This created speaker will be in 3D space.
            Speaker speaker = audioPlayer.AddSpeaker("Main", isSpatial: true, minDistance: 1.0f, maxDistance: 30.0f);

            // Attach created speaker to player.
            speaker.transform.parent = model.transform;

            // Set local positino to zero to make sure that speaker is in player.
            speaker.transform.localPosition = Vector3.zero;
        });

        if (MainPlugin.Configs.AudioDebug)
        {
            Log.Debug($"Adding clip: {clip} to model audio player at position: {audioPlayer.transform.position}");
        }
        audioPlayer.AddClip(clip);
    }
}