using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance;

    [Header("UI References")]
    // Changed from Slider to Image for custom rectangle bar
    public Image healthBarImage;
    public ShioriTransition transitionScript;

    [Header("Health Settings")]
    public float maxHealth = 1000f;
    private float currentHealth;
    private bool transitionTriggered = false;
    private bool isDead = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        // Initialize the bar to be full
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = 1f;
        }
    }

    void Update()
    {
        // Debug shortcut
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            Debug.Log("Debug: K pressed. Triggering 50% damage.");
            TakeDamage(maxHealth * 0.5f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the visual bar using fillAmount (0 to 1 range)
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }

        // Check phase transitions
        if (currentHealth <= maxHealth * 0.5f && !transitionTriggered)
        {
            TriggerPhaseTwo();
        }

        if (currentHealth <= 0 && !isDead)
        {
            TriggerEndurancePhase();
        }
    }

    private void TriggerPhaseTwo()
    {
        transitionTriggered = true;
        if (transitionScript != null)
        {
            transitionScript.StartPhaseTransition();
        }
        else
        {
            Debug.LogWarning("Transition Script missing from BossHealthBar!");
        }
    }

    private void TriggerEndurancePhase()
    {
        isDead = true;

        if (ShioriHazardManager.Instance != null)
        {
            ShioriHazardManager.Instance.EnterTantrum();
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartSurvivalTimer(30f);
        }
    }
}