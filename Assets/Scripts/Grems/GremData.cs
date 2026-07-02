using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New GremData", menuName = "Gremurin/GremData")]
public class GremData : ScriptableObject
{
    [Header("Identity")]
    public string gremName;
    public Sprite sprite;
    public GameObject speciesPrefab;

    [Header("Animations")]
    [Tooltip("The unique runtime animator override for this specific Gremurin variant in the Hub scene.")]
    public AnimatorOverrideController hubAnimatorOverride;

    [Header("Pokedex Entry")]
    [TextArea(2, 4)]
    public string flavorText;
    public List<GremSkill> skills;

    [Header("Health")]
    public float maxHealth = 3f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    public float hungerRate = 2f;

    [Header("Output")]
    public float currencyOutputRate = 5f;
    public float currencyOutputAmount = 1f;

    [Header("Movement")]
    public float wanderRadius = 1.5f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;
    public float moveSpeed = 1.5f;

    [Header("Role")]
    public GremRole role;

    [Header("Audio Profile")]
    public AudioPack audioPack;

    [Header("Music Preference")]
    [Tooltip("Exact track match that triggers the vibing state and one-shot secret discovery event.")]
    public TrackData favoriteTrack;

    [Header("Secret Visuals & Mood")]
    public Color spotlightColor = Color.magenta;
    [Range(0f, 1f)]
    [Tooltip("How dark the main room light gets during the show (e.g., 0.3 means it dims down to 30% brightness).")]
    public float danceShowIntensity = 0.3f;

    [Header("Custom Light Show Signature")]
    [Tooltip("If true, spotlights follow the Gremurin's movement. If false, they look towards a designated scene anchor.")]
    public bool trackGremurin = true;

    [Tooltip("The tag of the scene object the lights should target if Track Gremurin is disabled (e.g., 'StageCenter').")]
    public string sceneAnchorTag = "StageCenter";

    [Tooltip("How fast the spotlights wander around the target center point using Perlin Noise.")]
    public float spotlightOrbitSpeed = 2f;

    [Tooltip("How wide the spotlights can randomly swing outward from the center target point.")]
    public float spotlightRadiusScale = 4f;

    [Tooltip("The total time this specific dramatic light show performance lasts.")]
    public float showTotalDuration = 2.0f;

    [Tooltip("How many seconds into the show before the secret effect actually pops.")]
    public float secretActivationDelay = 0.8f;

    [Header("Secret Execution")]
    [Tooltip("The unique scriptable object action asset that runs when this specific Grem's secret triggers.")]
    public GremSecretEffect secretEffect;
}

[System.Serializable]
public class GremSkill
{
    public string skillName;
    [TextArea(1, 3)]
    public string skillDescription;
}

public enum GremRole
{
    Producer,
    Fighter,
    Breeder,
    Support,
    Specialist
}