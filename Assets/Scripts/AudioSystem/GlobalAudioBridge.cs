using System;
using UnityEngine;

public static class GlobalAudioBridge
{
    public static event Action<AudioClip> OnPlaySFX;

    public static void RaisePlaySFX(AudioClip clip)
    {
        OnPlaySFX?.Invoke(clip);
    }
}