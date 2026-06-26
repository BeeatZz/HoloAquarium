using UnityEngine;
using System.Collections;
using DG.Tweening;

public class NoveliteInkWarning : MonoBehaviour
{
    public SpriteRenderer warningCircle;
    public float targetScale = 2.4f;
    public float lifespan = 1.8f; // Should match the projectile's travel time

    private void Start()
    {
        if (warningCircle == null) warningCircle = GetComponentInChildren<SpriteRenderer>();

        if (warningCircle != null)
        {
            warningCircle.transform.localScale = Vector3.zero;
            warningCircle.color = new Color(1f, 0.1f, 0.1f, 0f);

            // Scale up instantly over a short window
            warningCircle.transform.DOScale(Vector3.one * targetScale, 0.25f);
            StartCoroutine(FlashRoutine());
        }

        // Automatically clean up when the projectile drops
        Destroy(gameObject, lifespan);
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < lifespan)
        {
            // Rapidly alternates visibility from 0 to 0.7 exactly like your Stomp script
            float alpha = Mathf.PingPong(elapsed * 12f, 1f);
            if (warningCircle != null)
            {
                warningCircle.color = new Color(1f, 0.1f, 0.1f, alpha * 0.65f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}