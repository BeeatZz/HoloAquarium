using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GremEgg : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Animation Settings")]
    public float bounceSpeed = 1.2f;
    public float bounceHeight = 0.08f;
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 8f;

    private GremData pendingData;
    private bool hatched;
    private Vector3 basePosition;
    private SpriteRenderer sr;

    public void Init(GremData data)
    {
        pendingData = data;
        hatched = false;
        basePosition = transform.position;
        sr = GetComponent<SpriteRenderer>();

        if (sr != null && closedSprite != null)
            sr.sprite = closedSprite;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void Update()
    {
        if (hatched) return;
        HandleBounceAndShake();
    }

    private void HandleBounceAndShake()
    {
        float bounce = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed)) * bounceHeight;
        float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;

        transform.position = new Vector3(
            basePosition.x + shake,
            basePosition.y + bounce,
            basePosition.z
        );
    }

    public void Hatch()
    {
        if (hatched || pendingData == null) return;
        hatched = true;

        StartCoroutine(HatchSequence());
    }

    private IEnumerator HatchSequence()
    {
        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        transform.DOKill();

        Vector3 hopPeak = basePosition + new Vector3(0, 0.4f, 0);
        transform.DOMove(hopPeak, 0.15f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.15f);

        transform.DOMove(basePosition, 0.15f).SetEase(Ease.InQuad);
        transform.DOScaleY(0.6f, 0.15f);
        yield return new WaitForSeconds(0.15f);

        transform.DOScaleY(1f, 0.1f);
        yield return new WaitForSeconds(0.1f);

        if (pendingData.speciesPrefab != null)
        {
            GameObject gremObj = Instantiate(pendingData.speciesPrefab, basePosition, Quaternion.identity);
            Gremurin grem = gremObj.GetComponent<Gremurin>();
            if (grem != null)
            {
                grem.data = pendingData;
            }
        }
        else
        {
            Debug.LogError($"No Species Prefab assigned in GremData: {pendingData.gremName}");
        }

        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}