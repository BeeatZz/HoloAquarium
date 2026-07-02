using UnityEngine;
using System.Collections;
using DG.Tweening;

public class NoveliteInkWarning : MonoBehaviour
{
    public SpriteRenderer warningCircle;
    public float targetScale = 2.4f;
    public float lifespan = 1.8f; 

    private void Start()
    {
        if (warningCircle == null) warningCircle = GetComponentInChildren<SpriteRenderer>();

        if (warningCircle != null)
        {
            warningCircle.transform.localScale = Vector3.zero;
            warningCircle.color = new Color(1f, 0.1f, 0.1f, 0f);

            
            warningCircle.transform.DOScale(Vector3.one * targetScale, 0.25f);
            StartCoroutine(FlashRoutine());
        }

        
        Destroy(gameObject, lifespan);
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < lifespan)
        {
            
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