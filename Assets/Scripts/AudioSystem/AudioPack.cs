using UnityEngine;

[CreateAssetMenu(fileName = "New AudioPack", menuName = "Audio/Audio Pack")]
public class AudioPack : ScriptableObject
{
    [Header("Standard Sounds")]
    public AudioClip[] spawnClips;
    public AudioClip[] attackClips;
    public AudioClip[] hitClips;
    public AudioClip[] deathClips;

    [Header("Special Sounds (Optional)")]
    public AudioClip[] specialSkillClips;

    public void PlaySpawn() => PlayRandom(spawnClips);
    public void PlayAttack() => PlayRandom(attackClips);
    public void PlayHit() => PlayRandom(hitClips);
    public void PlayDeath() => PlayRandom(deathClips);
    public void PlaySpecial() => PlayRandom(specialSkillClips);

    private void PlayRandom(AudioClip[] array)
    {
        if (array == null || array.Length == 0) return;

        // Pick a random sound for variations
        AudioClip randomClip = array[UnityEngine.Random.Range(0, array.Length)];

        // Radio it out to the global bridge
        GlobalAudioBridge.RaisePlaySFX(randomClip);
    }
}