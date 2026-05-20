using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    public Image cursorImage;
    public Sprite normalCursorSprite;
    public Sprite disabledCursorSprite;

    public Color normalColor = Color.white;
    public Color disabledColor = new Color(1f, 0.2f, 0.2f, 0.8f);

    private bool isDisabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (cursorImage != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            cursorImage.transform.position = mousePos;
        }
    }

    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;

        if (cursorImage == null) return;

        cursorImage.sprite = disabled ? disabledCursorSprite : normalCursorSprite;
        cursorImage.color = disabled ? disabledColor : normalColor;
    }
}