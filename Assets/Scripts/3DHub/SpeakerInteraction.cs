using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Attach to the speaker GameObject alongside its trigger collider (tag "Speaker").
/// Detects the PLAYER (not Grems) entering/exiting, shows a "Press E" prompt,
/// and switches to the next track in the playlist on key press.
/// </summary>
public class SpeakerInteraction : MonoBehaviour
{
    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("UI Prompt")]
    public GameObject promptRoot;      // parent object of the prompt (enabled/disabled)
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