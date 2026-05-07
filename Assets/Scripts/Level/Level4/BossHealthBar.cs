using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance;

    [Header("UI References")]
    public Slider healthSlider;
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
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    void Update()
    {
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

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

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