using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotatingRay : MonoBehaviour
{
    public SpriteRenderer[] rayArms;
    public SpriteRenderer[] warningArms;

    public float warningDuration = 1.5f;

    public Color warningColor = new Color(1f, 0f, 0f, 0.4f);
    public Color activeColor = new Color(1f, 0.8f, 0f, 0.9f);

    public LayerMask hazardLayer;

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
        {
            foreach (SpriteRenderer arm in warningArms)
            {
                if (arm != null)
                {
                    arm.color = warningColor;
                }
            }
        }

        if (rayArms != null)
        {
            foreach (SpriteRenderer arm in rayArms)
            {
                if (arm != null)
                {
                    arm.color = new Color(1f, 0.8f, 0f, 0f);
                }
            }
        }

        yield return new WaitForSeconds(warningDuration);

        if (warningArms != null)
        {
            foreach (SpriteRenderer arm in warningArms)
            {
                if (arm != null)
                {
                    arm.DOFade(0f, 0.3f);
                }
            }
        }

        if (rayArms != null)
        {
            foreach (SpriteRenderer arm in rayArms)
            {
                if (arm != null)
                {
                    arm.DOFade(activeColor.a, 0.3f);
                }
            }
        }

        rotating = true;
        active = true;
    }

    private void Update()
    {
        if (!rotating)
        {
            return;
        }

        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        CheckCursorHit();
    }

    private void CheckCursorHit()
    {
        if (cursorDisabled || !active)
        {
            return;
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mouseScreen);

        RaycastHit2D hit = Physics2D.Raycast(
            worldPos,
            Vector2.zero,
            0f,
            hazardLayer
        );

        if (hit.collider != null)
        {
            StartCoroutine(DisableCursor());
        }
    }

    private IEnumerator DisableCursor()
    {
        cursorDisabled = true;

        if (PlayerInput.Instance != null)
        {
            PlayerInput.Instance.enabled = false;
        }

        CursorManager.Instance?.SetDisabled(true);

        yield return new WaitForSeconds(cursorDisableDuration);

        if (PlayerInput.Instance != null)
        {
            PlayerInput.Instance.enabled = true;
        }

        CursorManager.Instance?.SetDisabled(false);

        cursorDisabled = false;
    }

    private void OnDestroy()
    {
        if (PlayerInput.Instance != null)
        {
            PlayerInput.Instance.enabled = true;
        }

        CursorManager.Instance?.SetDisabled(false);
    }
}