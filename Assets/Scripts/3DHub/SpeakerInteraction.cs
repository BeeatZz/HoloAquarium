using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;






public class SpeakerInteraction : MonoBehaviour
{
    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("UI Prompt")]
    public GameObject promptRoot;      
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press E to switch track";

    private bool playerInRange;

    private void Start()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);

        if (promptText != null)
            promptText.text = promptMessage;
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (MusicManager.Instance != null)
                MusicManager.Instance.PlayNext();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;
        if (promptRoot != null)
            promptRoot.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }
}