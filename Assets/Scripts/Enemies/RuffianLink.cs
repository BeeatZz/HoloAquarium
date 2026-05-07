using UnityEngine;

public class RuffianLink : MonoBehaviour
{
    private RuffianEnemy ruffian;
    private LineRenderer line;
    
    [Header("Visuals")]
    public GameObject shieldOverlay;
    void Awake()
    {
        ruffian = GetComponent<RuffianEnemy>();
        line = gameObject.AddComponent<LineRenderer>();
        
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.cyan;
        line.endColor = Color.magenta;
        line.positionCount = 2;
        line.enabled = false;

        if (shieldOverlay != null) shieldOverlay.SetActive(false);
    }

    void Update()
    {
        if (!ruffian.isLinked || ruffian.partner == null)
        {
            line.enabled = false;
            if (shieldOverlay != null) shieldOverlay.SetActive(false);
            return;
        }

        {
            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, ruffian.partner.transform.position);
        }

        if (ruffian.role == RuffianRole.Ward)
        {
            if (shieldOverlay != null) shieldOverlay.SetActive(true);
        }
    }
}