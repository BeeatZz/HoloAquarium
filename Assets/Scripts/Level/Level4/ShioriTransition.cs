using UnityEngine;
using DG.Tweening;

public class ShioriTransition : MonoBehaviour
{
    private Camera cam;

    [Header("Phase 2 Target Settings (LOCAL)")]
    public Vector3 targetLocalPosition;
    public Vector3 targetLocalRotation;
    public float targetFOV = 60f;
    public float duration = 2.5f;

    [Header("Visual Effects")]
    public CanvasGroup crackOverlay;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (crackOverlay != null) crackOverlay.alpha = 0f;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.is3DPerspectiveMode = false;
        }
    }

    public void StartPhaseTransition()
    {
        if (crackOverlay != null)
        {
            crackOverlay.alpha = 1f;
            transform.DOShakePosition(0.5f, 0.5f);
        }

        cam.orthographic = false;

        transform.DOLocalMove(targetLocalPosition, duration).SetEase(Ease.InOutQuint);

        transform.DOLocalRotate(targetLocalRotation, duration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuint)
            .OnComplete(() => {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.is3DPerspectiveMode = true;
                    if (ShioriHazardManager.Instance != null)
                    {
                        ShioriHazardManager.Instance.StartStandardAttacks();
                    }
                    Debug.Log("3D Mode Logic Activated Globally.");
                }
            });

        cam.DOFieldOfView(targetFOV, duration).SetEase(Ease.InOutQuint);
    }
}