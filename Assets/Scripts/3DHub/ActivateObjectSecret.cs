using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "ActivateObjectSecret", menuName = "Gremurin/Secrets/Activate Object")]
public class ActivateObjectSecret : GremSecretEffect
{
    [Header("Target Setup")]
    [Tooltip("The exact game object name in your scene hierarchy that you want to reveal.")]
    public string targetObjectName;

    [Header("Audio")]
    [Tooltip("The sound effect that plays when the secret object pops into existence.")]
    public AudioClip discoverySound;
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("Particles (Landing Style)")]
    [Tooltip("Drag a basic unlit particle material here (like Default-Particle).")]
    public Material particleMaterial;
    public int particleCount = 15;
    public float particleSpeed = 1.5f;
    public float particleLifetime = 0.6f;
    public float particleSize = 0.08f;
    public Color particleColor = new Color(0.75f, 0.68f, 0.55f, 0.6f);

    public override void TriggerSecret(HubGremurin grem, TrackData track)
    {
        if (string.IsNullOrEmpty(targetObjectName))
        {
            Debug.LogError($"[ActivateObjectSecret] Target Object Name is blank on asset '{name}'!", grem);
            return;
        }

        // 1. Find the object in the scene (even if it's currently inactive)
        GameObject target = FindInactiveObjectByName(targetObjectName);

        if (target != null)
        {
            // Activate the hidden object!
            target.SetActive(true);
            Debug.Log($"[SECRET REVEALED] {grem.gameObject.name} unlocked hidden object: '{targetObjectName}'!", target);

            // 2. Play the discovery sound directly at the object's spot
            if (discoverySound != null)
            {
                AudioSource.PlayClipAtPoint(discoverySound, target.transform.position, volume);
            }

            // 3. Spawn a custom "landing style" dust puff directly on it
            SpawnDustPuff(target.transform.position);
        }
        else
        {
            Debug.LogError($"[ActivateObjectSecret] Failed to find any scene object named '{targetObjectName}'. Is it spelled exactly right?", grem);
        }
    }

    /// <summary>
    /// Deep-searches the entire scene hierarchy for objects matching a name, 
    /// even if they started the game disabled (GameObject.Find skips inactive ones).
    /// </summary>
    private GameObject FindInactiveObjectByName(string nameToFind)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            // Ensure it belongs to the active gameplay world, not a prefab file asset inside project folders
            if (t.gameObject.hideFlags == HideFlags.None && t.name == nameToFind)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Generates a quick dust-puff particle burst dynamically at runtime 
    /// mirroring the HubGremurin landing effect settings.
    /// </summary>
    private void SpawnDustPuff(Vector3 spawnPosition)
    {
        GameObject psObj = new GameObject("Secret_DustPuff");
        psObj.transform.position = spawnPosition;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.startColor = particleColor;
        main.gravityModifier = 0.1f;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.2f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.radial = new ParticleSystem.MinMaxCurve(-0.3f);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // If you forget to assign a material in the inspector, it fallbacks gracefully to the engine sprite material
        renderer.material = particleMaterial != null ? particleMaterial : new Material(Shader.Find("Sprites/Default"));

        // Play and self-destruct the gameobject container once finished
        ps.Emit(particleCount);

        // Explicitly invoke Object level destruction since ScriptableObject context lacks an instance MonoBehaviour shortcut
        UnityEngine.Object.Destroy(psObj, particleLifetime + 0.1f);
    }
}