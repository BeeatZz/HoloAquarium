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

        
        GameObject target = FindInactiveObjectByName(targetObjectName);

        if (target != null)
        {
            
            target.SetActive(true);
            Debug.Log($"[SECRET REVEALED] {grem.gameObject.name} unlocked hidden object: '{targetObjectName}'!", target);

            
            if (discoverySound != null)
            {
                AudioSource.PlayClipAtPoint(discoverySound, target.transform.position, volume);
            }

            
            SpawnDustPuff(target.transform.position);
        }
        else
        {
            Debug.LogError($"[ActivateObjectSecret] Failed to find any scene object named '{targetObjectName}'. Is it spelled exactly right?", grem);
        }
    }

    
    
    
    
    private GameObject FindInactiveObjectByName(string nameToFind)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            
            if (t.gameObject.hideFlags == HideFlags.None && t.name == nameToFind)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    
    
    
    
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

        
        renderer.material = particleMaterial != null ? particleMaterial : new Material(Shader.Find("Sprites/Default"));

        
        ps.Emit(particleCount);

        
        UnityEngine.Object.Destroy(psObj, particleLifetime + 0.1f);
    }
}