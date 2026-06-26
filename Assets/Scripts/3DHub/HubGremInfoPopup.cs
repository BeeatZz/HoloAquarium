using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubGremInfoPopup : MonoBehaviour
{
    public static HubGremInfoPopup Instance { get; private set; }

    [Header("References")]
    public GameObject panel;
    public Image gremPhoto;
    public TextMeshProUGUI gremNameText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI flavorText;
    public TextMeshProUGUI skillsText;
    public Button closeButton;

    public bool isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        closeButton?.onClick.AddListener(Hide);
        Hide();
    }

    public void Show(GremData data)
    {
        if (data == null) return;

        isVisible = true;
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        if (gremPhoto != null && data.sprite != null)
            gremPhoto.sprite = data.sprite;

        if (gremNameText != null)
            gremNameText.text = data.gremName;

        if (roleText != null)
            roleText.text = data.role.ToString();

        if (flavorText != null)
            flavorText.text = string.IsNullOrEmpty(data.flavorText)
                ? "No entry yet."
                : data.flavorText;

        if (skillsText != null)
        {
            if (data.skills == null || data.skills.Count == 0)
            {
                skillsText.text = "No skills.";
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (GremSkill skill in data.skills)
                    sb.AppendLine($"<b>{skill.skillName}</b>\n{skill.skillDescription}\n");
                skillsText.text = sb.ToString().TrimEnd();
            }
        }
    }

    public void Hide()
    {
        isVisible = false;
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
}