using UnityEngine;
using DG.Tweening;

public class Crystal : MonoBehaviour
{
    [Header("Settings")]
    public Color defaultColor = Color.cyan;
    public Color warningColor = Color.red;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = defaultColor;
    }

    public void SetWarning(bool active)
    {
        if (sr == null) return;

        sr.DOKill();
        sr.DOColor(active ? warningColor : defaultColor, 0.2f);

        if (active)
            transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
    }
}