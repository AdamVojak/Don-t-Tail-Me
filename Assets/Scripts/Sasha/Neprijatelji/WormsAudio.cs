using UnityEngine;

public class WormsAudio : MonoBehaviour
{
    [Header("Audio Sources (3D)")]
    [SerializeField] private AudioSource crawlSource;
    [SerializeField] private AudioSource rockSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Kretanje (Paralelni Slojevi)")]
    [SerializeField] private AudioClip[] crawlClips; // 2 zvuka gmizanja
    [SerializeField] private AudioClip[] rockClips;  // 2 zvuka struganja kamenja
    [Range(0f, 1f)][SerializeField] private float moveVolume = 0.7f;

    [Header("Udarci i Struja")]
    [SerializeField] private AudioClip[] pajserHitClips;
    [SerializeField] private AudioClip[] electricMeleeHitClips;
    [SerializeField] private AudioClip shockClip;
    [Range(0f, 1f)][SerializeField] private float hitVolume = 0.8f;

    [Header("Spawnanje Crva")]
    [SerializeField] private AudioClip spawnWormsClip;
    [Range(0f, 1f)][SerializeField] private float spawnVolume = 1f;

    private void Awake()
    {
        // Automatsko kreiranje ako nisu dodijeljeni
        if (crawlSource == null) crawlSource = gameObject.AddComponent<AudioSource>();
        if (rockSource == null) rockSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        Setup3DSource(crawlSource);
        Setup3DSource(rockSource);
        Setup3DSource(sfxSource);
    }

    private void Setup3DSource(AudioSource source)
    {
        source.spatialBlend = 1f; // 3D zvuk
        source.playOnAwake = false;
        source.minDistance = 2f;
        source.maxDistance = 25f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void PlayRandomSFX(AudioSource source, AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || source == null) return;
        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    // ==========================================
    // 1. KRETANJE (Paralelno)
    // ==========================================
    public void PlayMoveSound()
    {
        // Pušta oba sloja istovremeno s malim varijacijama pitcha
        PlayRandomSFX(crawlSource, crawlClips, moveVolume);
        PlayRandomSFX(rockSource, rockClips, moveVolume);
    }

    // ==========================================
    // 2. UDARCI I STRUJA
    // ==========================================
    public void PlayPajserHit()
    {
        PlayRandomSFX(sfxSource, pajserHitClips, hitVolume);
    }

    public void PlayElectricMeleeHit()
    {
        PlayRandomSFX(sfxSource, electricMeleeHitClips, hitVolume);
    }

    public void PlayShockSound()
    {
        if (sfxSource != null && shockClip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(shockClip, hitVolume);
        }
    }

    // ==========================================
    // 3. SPAWNANJE CRVA
    // ==========================================
    public void PlaySpawnSound()
    {
        if (sfxSource != null && spawnWormsClip != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(spawnWormsClip, spawnVolume);
        }
    }
}