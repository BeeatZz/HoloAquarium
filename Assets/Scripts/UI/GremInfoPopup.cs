using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GremInfoPopup : MonoBehaviour
{
    public static GremInfoPopup Instance { get; private set; }

    [Header("Panel")]
    public RectTransform panelRect;
    public Vector2 screenOffset = new Vector2(0f, 80f);

    [Header("Tabs")]
    public GameObject entryTab;
    public GameObject statusTab;
    public Button entryTabButton;
    public Button statusTabButton;
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Entry Tab")]
    public Image gremPhoto;
    public TextMeshProUGUI gremNameText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI flavorText;
    public TextMeshProUGUI skillsText;
    public TextMeshProUGUI baseStatsText;

    [Header("Status Tab")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI outputText;
    public Slider healthSlider;
    public Slider hungerSlider;

    private Gremurin currentGrem;
    private Canvas canvas;
    public bool isVisible;
    private bool isEntryTab = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        canvas = GetComponentInParent<Canvas>();

        entryTabButton?.onClick.AddListener(() => SwitchTab(true));
        statusTabButton?.onClick.AddListener(() => SwitchTab(false));

        Hide();
    }

    private void Update()
    {
        if (!isVisible || currentGrem == null) return;

        if (!isEntryTab)
            UpdateStatusTab();

    }

    public void Show(Gremurin grem)
    {
        Time.timeScale = 0f;
        currentGrem = grem;
        isVisible = true;
        gameObject.SetActive(true);

        PopulateEntryTab();
        UpdateStatusTab();
        SwitchTab(true);
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        currentGrem = null;
        isVisible = false;
        gameObject.SetActive(false);
    }

    public bool IsShowing(Gremurin grem)
    {
        return isVisible && currentGrem == grem;
    }

    private void SwitchTab(bool showEntry)
    {
        Debug.Log($"SwitchTab called — showEntry: {showEntry}, entryTab null: {entryTab == null}, statusTab null: {statusTab == null}");
        isEntryTab = showEntry;
        entryTab.SetActive(showEntry);
        statusTab.SetActive(!showEntry);

        if (entryTabButton != null)
        {
            ColorBlock eb = entryTabButton.colors;
            eb.normalColor = showEntry ? activeTabColor : inactiveTabColor;
            entryTabButton.colors = eb;
        }

        if (statusTabButton != null)
        {
            ColorBlock sb = statusTabButton.colors;
            sb.normalColor = !showEntry ? activeTabColor : inactiveTabColor;
            statusTabButton.colors = sb;
        }
    }

    private void PopulateEntryTab()
    {
        if (currentGrem == null || currentGrem.data == null) return;

        GremData d = currentGrem.data;

        if (gremPhoto != null && d.sprite != null)
            gremPhoto.sprite = d.sprite;

        if (gremNameText != null)
            gremNameText.text = d.gremName;

        if (roleText != null)
            roleText.text = d.role.ToString();

        if (flavorText != null)
            flavorText.text = string.IsNullOrEmpty(d.flavorText)
                ? "No entry yet."
                : d.flavorText;

        if (skillsText != null)
        {
            if (d.skills == null || d.skills.Count == 0)
            {
                skillsText.text = "No skills.";
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (GremSkill skill in d.skills)
                    sb.AppendLine($"<b>{skill.skillName}</b>\n{skill.skillDescription}\n");
                skillsText.text = sb.ToString().TrimEnd();
            }
        }

        if (baseStatsText != null)
        {
            baseStatsText.text =
                $"Max Health: {d.maxHealth}\n" +
                $"Hunger Rate: {d.hungerRate}/s\n" +
                $"Output: {d.currencyOutputAmount} every {d.currencyOutputRate}s\n" +
                $"Move Speed: {d.moveSpeed}";
        }
    }

    private void UpdateStatusTab()
    {
        if (currentGrem == null || currentGrem.data == null) return;

        float healthPct = currentGrem.currentHealth / currentGrem.data.maxHealth;
        float hungerPct = currentGrem.currentHunger / currentGrem.data.maxHunger;

        string healthColor = healthPct > 0.6f ? "green" :
                             healthPct > 0.3f ? "yellow" : "red";

        string hungerColor = hungerPct > 0.6f ? "green" :
                             hungerPct > 0.3f ? "yellow" : "red";

        if (healthText != null)
            healthText.text =
                $"<color={healthColor}>{currentGrem.currentHealth:F1} / {currentGrem.data.maxHealth:F1}</color>";

        if (hungerText != null)
            hungerText.text =
                $"<color={hungerColor}>{hungerPct * 100f:F0}%</color>";

        if (outputText != null)
        {
            bool producing = currentGrem.currentHunger > 0;
            outputText.text = producing
                ? $"Producing every {currentGrem.data.currencyOutputRate:F1}s"
                : "<color=red>Starving — not producing</color>";
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = currentGrem.data.maxHealth;
            healthSlider.value = currentGrem.currentHealth;
        }

        if (hungerSlider != null)
        {
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = currentGrem.data.maxHunger;
            hungerSlider.value = currentGrem.currentHunger;
        }
    }
    public bool IsClickInsidePopup(Vector2 screenPosition)
    {
        if (!isVisible) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(
            panelRect,
            screenPosition,
            canvas.worldCamera
        );
    }

}