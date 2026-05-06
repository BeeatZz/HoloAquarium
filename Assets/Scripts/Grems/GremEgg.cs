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
    private GameObject gremPrefab;
    private bool hatched;
    private Vector3 basePosition;
    private SpriteRenderer sr;

    public void Init(GremData data, GameObject prefab)
    {
        pendingData = data;
        gremPrefab = prefab;
        hatched = false;
        basePosition = transform.position;
        sr = GetComponent<SpriteRenderer>();

        // Spawn pop
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

    private void OnMouseDown()
    {
        // Handled by PlayerInput
    }

    public void Hatch()
    {
        if (hatched) return;
        hatched = true;

        StartCoroutine(HatchSequence());
    }

    private IEnumerator HatchSequence()
    {
        // Change sprite immediately on click
        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        // Hop up
        Vector3 startPos = basePosition;
        Vector3 hopPeak = basePosition + new Vector3(0, 0.4f, 0);

        transform.DOKill();

        // Up
        transform.DOMove(hopPeak, 0.15f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.15f);

        // Squish on the way down
        transform.DOMove(startPos, 0.15f).SetEase(Ease.InQuad);
        transform.DOScaleY(0.6f, 0.15f);
        yield return new WaitForSeconds(0.15f);

        // Unsquish
        transform.DOScaleY(1f, 0.1f);
        yield return new WaitForSeconds(0.1f);

        // TODO: smoke effect here

        // Spawn grem
        if (gremPrefab != null)
        {
            GameObject gremObj = Instantiate(gremPrefab, basePosition, Quaternion.identity);
            Gremurin grem = gremObj.GetComponent<Gremurin>();
            if (grem != null)
                grem.data = pendingData;
        }

        // Pop out
        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}