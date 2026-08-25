using UnityEngine;

public class WormAudio : MonoBehaviour
{
    [Header("3D Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Kretanje (Gmizanje)")]
    [SerializeField] private AudioClip[] moveClips;
    [Range(0f, 1f)][SerializeField] private float moveVolume = 0.5f;

    [Header("Udarci i Struja")]
    [SerializeField] private AudioClip[] pajserHitClips;
    [SerializeField] private AudioClip[] electricMeleeHitClips;
    [SerializeField] private AudioClip shockClip;
    [Range(0f, 1f)][SerializeField] private float hitVolume = 0.8f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponentInChildren<AudioSource>();
    }

    private void PlayRandomSFX(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    // --- METODE ZA ANIMACIJU (Kretanje) ---
    public void PlayMoveSound()
    {
        PlayRandomSFX(moveClips, moveVolume);
    }

    // --- METODE ZA KOD (Udarci) ---
    public void PlayPajserHit()
    {
        PlayRandomSFX(pajserHitClips, hitVolume);
    }

    public void PlayElectricMeleeHit()
    {
        PlayRandomSFX(electricMeleeHitClips, hitVolume);
    }

    public void PlayShockSound()
    {
        if (audioSource != null && shockClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shockClip, hitVolume);
        }
    }
}