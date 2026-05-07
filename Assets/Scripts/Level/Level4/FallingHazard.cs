using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FallingHazard : MonoBehaviour
{
    [Header("Settings")]
    public float fallDuration = 1.0f;
    public float lingerTime = 0.8f;
    public float spawnDepthOffset = -15f;

    [Header("Combat Settings")]
    public float damageRadius = 2.0f;
    public float damageAmount = 20f; // Set to 20 as requested

    [Header("References")]
    public SpriteRenderer warningShadow;
    public GameObject visualModel;

    private Vector3 targetGroundPos; 

    public void Setup(Vector3 groundPosition, float scale)
    {
        targetGroundPos = groundPosition; 
        transform.position = targetGroundPos;
        transform.localScale = Vector3.one * scale;

        warningShadow.transform.localPosition = new Vector3(0, 0, -0.05f);
        visualModel.transform.localPosition = new Vector3(0, 0, spawnDepthOffset);

        StartCoroutine(HazardSequence());
    }

    IEnumerator HazardSequence()
    {
        Color initialColor = warningShadow.color;
        initialColor.a = 0;
        warningShadow.color = initialColor;

        warningShadow.transform.localScale = Vector3.zero;
        warningShadow.DOFade(0.6f, 0.2f);
        warningShadow.transform.DOScale(Vector3.one * 3f, fallDuration).SetEase(Ease.OutBack);

        visualModel.transform.DOLocalMoveZ(0, fallDuration).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(fallDuration);

        warningShadow.DOFade(0, 0.1f);
        Camera.main.transform.DOShakePosition(0.2f, 0.3f);

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(targetGroundPos, damageRadius);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.TryGetComponent(out Gremurin grem))
            {
                grem.TakeDamage(damageAmount);
            }
        }

        yield return new WaitForSeconds(lingerTime);

        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}