using UnityEngine;

[CreateAssetMenu(fileName = "New TrackData", menuName = "Audio/Track Data")]
public class TrackData : ScriptableObject
{
    [Header("Identity")]
    public string trackName;
    public AudioClip clip;
    public Sprite coverArt;

    [Header("Classification")]
    public MusicGenre genre;
    public float bpm = 120f;

    [Header("Unlock")]
    public bool unlockedByDefault = true; 
}

public enum MusicGenre
{
    Chill,
    Upbeat,
    Electronic,
    Orchestral,
    Jazz,
    Rock
}