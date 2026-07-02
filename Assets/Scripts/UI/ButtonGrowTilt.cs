using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ButtonGrowTilt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum TiltSide { Left, Right }

    [Header("Tilt Settings")]
    public TiltSide tiltDirection = TiltSide.Left;

    [Range(0f, 45f)]
    public float tiltStrength = 10f;

    [Header("Scale Settings")]
    [Range(1f, 2f)]
    public float scaleStrength = 1.1f;

    [Header("Animation")]
    public float animationSpeed = 0.15f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Vector3 targetScale;
    private Quaternion targetRotation;

    private Coroutine animCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        float angle = tiltDirection == TiltSide.Left ? tiltStrength : -tiltStrength;

        targetScale = originalScale * scaleStrength;
        targetRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);

        StartAnim(targetScale, targetRotation);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartAnim(originalScale, originalRotation);
    }

    private void StartAnim(Vector3 scale, Quaternion rotation)
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(AnimateTo(scale, rotation));
    }

    private System.Collections.IEnumerator AnimateTo(Vector3 scale, Quaternion rotation)
    {
        Vector3 startScale = rectTransform.localScale;
        Quaternion startRotation = rectTransform.localRotation;

        float t = 0f;
        while (t < animationSpeed)
        {
            t += Time.unscaledDeltaTime;
            float lerpT = Mathf.Clamp01(t / animationSpeed);

            lerpT = 1f - Mathf.Pow(1f - lerpT, 3f);

            rectTransform.localScale = Vector3.Lerp(startScale, scale, lerpT);
            rectTransform.localRotation = Quaternion.Slerp(startRotation, rotation, lerpT);

            yield return null;
        }

        rectTransform.localScale = scale;
        rectTransform.localRotation = rotation;
    }
}