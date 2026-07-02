using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class VaultTrigger : MonoBehaviour
{


    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("UI Prompt")]
    public GameObject promptsRoot;
    public TextMeshProUGUI promptsText;
    public string promptsMessage = "Press E to go back to hub";

    private bool playerInRange;

    private void Start()
    {
        if (promptsRoot != null)
            promptsRoot.SetActive(false);

        if (promptsText != null)
            promptsText.text = promptsMessage;
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SceneFader.Instance.FadeToScene("MainMenu");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;
        if (promptsRoot != null)
            promptsRoot.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        if (promptsRoot != null)
            promptsRoot.SetActive(false);
    }
}
