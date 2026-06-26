using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HubInteraction : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public float interactRange = 4f;
    public Image crosshair;
    public Color defaultCrosshairColor = Color.white;
    public Color highlightCrosshairColor = Color.yellow;

    private HubGremurin currentLookTarget;

    private void Update()
    {
        if (HubGremInfoPopup.Instance != null && HubGremInfoPopup.Instance.isVisible)
            return;

        DetectLookTarget();

        if (Mouse.current.leftButton.wasPressedThisFrame && currentLookTarget != null)
            OpenInfo(currentLookTarget);
    }

    private void DetectLookTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            HubGremurin grem = hit.collider.GetComponentInParent<HubGremurin>();
            if (grem != null)
            {
                currentLookTarget = grem;
                SetCrosshair(true);
                return;
            }
        }

        currentLookTarget = null;
        SetCrosshair(false);
    }

    private void SetCrosshair(bool highlighted)
    {
        if (crosshair == null) return;
        crosshair.color = highlighted ? highlightCrosshairColor : defaultCrosshairColor;
    }

    private void OpenInfo(HubGremurin grem)
    {
        HubGremInfoPopup.Instance?.Show(grem.data);
        Cursor.lockState = CursorLockMode.None;
    }
}