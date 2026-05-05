using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BeamHazard : MonoBehaviour
{
    [Header("Settings")]
    public float warningDuration = 1.5f;
    public float beamDuration = 0.4f;
    public float beamWidth = 0.15f;
    public float beamDamage = 1f;
    public Color warningColor = new Color(1f, 0.3f, 0.3f, 0.4f);
    public Color beamColor = new Color(1f, 0.9f, 0.2f, 1f);

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.sortingOrder = 5;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
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
        lineRenderer.startColor = warningColor;
        lineRenderer.endColor = warningColor;
        lineRenderer.enabled = true;

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            float alpha = Mathf.PingPong(elapsed * 3f, 1f);
            Color c = warningColor;
            c.a = alpha * 0.6f;
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        lineRenderer.startColor = beamColor;
        lineRenderer.endColor = beamColor;
        lineRenderer.startWidth = beamWidth * 2f;
        lineRenderer.endWidth = beamWidth * 2f;

        for (int i = 0; i < positions.Length - 1; i++)
            DamageAlongSegment(positions[i], positions[i + 1]);

        yield return new WaitForSeconds(beamDuration);

        foreach (Crystal c in crystals)
            c.SetWarning(false);

        lineRenderer.enabled = false;
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;

        Destroy(gameObject);
    }

    private void DamageAlongSegment(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            start, beamWidth, direction, distance
        );

        foreach (RaycastHit2D hit in hits)
        {
            Gremurin grem = hit.collider.GetComponent<Gremurin>();
            if (grem != null && !grem.isDead)
                grem.TakeDamage(beamDamage);
        }
    }
}