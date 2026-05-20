using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;

public class RotatingRay : MonoBehaviour
{
    public SpriteRenderer[] rayArms;
    public SpriteRenderer[] warningArms;

    public float warningDuration = 1.5f;
    public Color warningColor = new Color(1f, 0f, 0f, 0.4f);
    public Color activeColor = new Color(1f, 0.8f, 0f, 0.9f);

    private float rotateSpeed;
    private float cursorDisableDuration;
    private bool active;
    private bool cursorDisabled;
    private bool rotating;

    public void Init(float speed, float disableDuration)
    {
        rotateSpeed = speed;
        cursorDisableDuration = disableDuration;
        StartCoroutine(RaySequence());
    }

    private IEnumerator RaySequence()
    {
        if (warningArms != null)
            foreach (SpriteRenderer arm in warningArms)
                if (arm != null) arm.color = warningColor;

        if (rayArms != null)
            foreach (SpriteRenderer arm in rayArms)
                if (arm != null) arm.color = new Color(1f, 0.8f, 0f, 0f);

        yield return new WaitForSeconds(warningDuration);

        if (warningArms != null)
            foreach (SpriteRenderer arm in warningArms)
                if (arm != null) arm.DOFade(0f, 0.3f);

        if (rayArms != null)
            foreach (SpriteRenderer arm in rayArms)
                if (arm != null) arm.DOFade(0.9f, 0.3f);

        rotating = true;
        active = true;
    }

    private void Update()
    {
        if (!rotating) return;

        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        CheckCursorHit();
    }

    private void CheckCursorHit()
    {
        if (cursorDisabled) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mouseScreen);

        if (rayArms == null) return;

        foreach (SpriteRenderer arm in rayArms)
        {
            if (arm == null) continue;
            Vector2 rayDir = arm.transform.up.normalized;
            Vector2 toMouse = worldPos - (Vector2)transform.position;

            float dot = Vector2.Dot(toMouse.normalized, rayDir);
            float dist = toMouse.magnitude;

            if (dot > 0.95f && dist < 4f)
            {
                StartCoroutine(DisableCursor());
                return;
            }
        }
    }

    private IEnumerator DisableCursor()
    {
        cursorDisabled = true;

        if (PlayerInput.Instance != null)
            PlayerInput.Instance.enabled = false;

        CursorManager.Instance?.SetDisabled(true);

        yield return new WaitForSeconds(cursorDisableDuration);

        if (PlayerInput.Instance != null)
            PlayerInput.Instance.enabled = true;

        CursorManager.Instance?.SetDisabled(false);

        cursorDisabled = false;
    }

    private void OnDestroy()
    {
        if (PlayerInput.Instance != null)
            PlayerInput.Instance.enabled = true;

        CursorManager.Instance?.SetDisabled(false);
    }
}