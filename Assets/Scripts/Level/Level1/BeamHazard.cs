using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BeamHazard : MonoBehaviour
{
    [Header("Settings")]
    public float warningDuration = 3f;
    public float beamDuration = 0.4f;
    public float beamWidth = 0.15f;
    public float beamDamage = 1f;

    [Header("Visuals")]
    public Color warningColor = new Color(1f, 0.3f, 0.3f, 0.4f);
    // Deep purple for the outer edges
    public Color beamEdgeColor = new Color(0.5f, 0f, 1f, 0.8f);
    // Near-white purple for the "hot" center core
    public Color beamCenterColor = new Color(0.9f, 0.8f, 1f, 1f);

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.sortingOrder = 5;

        // Using the "Additive" shader makes colors "pop" and glow against the background
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        lineRenderer.enabled = false;
    }

    public IEnumerator FireBeam(List<Crystal> crystals)
    {
        if (crystals == null || crystals.Count < 2) yield break;

        Vector3[] positions = new Vector3[crystals.Count];
        for (int i = 0; i < crystals.Count; i++)
            positions[i] = crystals[i].transform.position;

        foreach (Crystal c in crystals)
            c.SetWarning(true);

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
        lineRenderer.enabled = true;

        // Warning phase (Flickering Red)
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            float alpha = Mathf.PingPong(elapsed * 6f, 1f);
            lineRenderer.startColor = lineRenderer.endColor = new Color(warningColor.r, warningColor.g, warningColor.b, alpha * 0.4f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- PURPLE BEAM BLAST ---
        SetupBeamGradient();
        lineRenderer.startWidth = beamWidth * 2.5f;
        lineRenderer.endWidth = beamWidth * 2.5f;

        for (int i = 0; i < positions.Length - 1; i++)
            DamageAlongSegment(positions[i], positions[i + 1]);

        // Jitter/Flicker effect during the blast
        float blastElapsed = 0f;
        while (blastElapsed < beamDuration)
        {
            lineRenderer.widthMultiplier = UnityEngine.Random.Range(0.9f, 1.2f); // Makes the beam vibrate
            blastElapsed += Time.deltaTime;
            yield return null;
        }

        foreach (Crystal c in crystals)
            c.SetWarning(false);

        lineRenderer.enabled = false;
        Destroy(gameObject);
    }

    private void SetupBeamGradient()
    {
        // This creates a "glow" look by fading the color at the ends 
        // and using the center color for the middle of the line
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(beamEdgeColor, 0.0f),
                new GradientColorKey(beamCenterColor, 0.5f),
                new GradientColorKey(beamEdgeColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(1.0f, 0.5f),
                new GradientAlphaKey(0.8f, 1.0f)
            }
        );
        lineRenderer.colorGradient = gradient;
    }

    private void DamageAlongSegment(Vector3 start, Vector3 end)
    {
        int hitboxLayer = LayerMask.GetMask("GremHitbox");
        Vector2 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            start, beamWidth, direction, distance, hitboxLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            Gremurin grem = hit.collider.GetComponentInParent<Gremurin>();
            if (grem != null && !grem.isDead)
                grem.TakeDamage(beamDamage);
        }
    }
}