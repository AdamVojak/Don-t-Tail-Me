using UnityEngine;

public class KucanjeSrca: MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvučni Efekti Srca")]
    [SerializeField] private AudioClip beat1;        // Prvi dio otkucaja ("Lub")
    [SerializeField] private AudioClip beat2;        // Drugi dio otkucaja ("Dub")
    [SerializeField] private AudioClip flatlineSound; // Zvuk smrti (pištanje)

    [Header("Postavke Dinamičkog Pulsa")]
    [Range(0f, 1f)][SerializeField] private float baseVolume = 0.8f; // Početna glasnoća na 3 HP
    [SerializeField] private float volumeIncreasePerLostHP = 0.1f;    // Povećanje po izgubljenom HP-u

    private float currentVolume;
    private bool isDead = false;
    private Animator heartAnimator;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        heartAnimator = GetComponent<Animator>();

        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f; // 2D zvuk za UI
            audioSource.playOnAwake = false;
        }

        currentVolume = baseVolume;
    }

    /// <summary>
    /// Poziva se iz SashaControllera pri primanju štete ili liječenju
    /// </summary>
    public void UpdateHeartRateByHealth(int currentHealth, int maxHealth = 3)
    {
        if (isDead) return;

        // Koliko je HP-a izgubljeno (npr. 3 - 2 = 1 izgubljen HP)
        int lostHP = Mathf.Max(0, maxHealth - currentHealth);

        // Dinamička glasnoća: 0.8 + (1 * 0.1) = 0.9f itd.
        currentVolume = Mathf.Clamp01(baseVolume + (lostHP * volumeIncreasePerLostHP));

        // Blago ubrzavamo i visinu tona i animaciju srca radi panike
        float speedMultiplier = 1.0f + (lostHP * 0.15f);

        if (audioSource != null) audioSource.pitch = speedMultiplier;
        if (heartAnimator != null) heartAnimator.speed = speedMultiplier;
    }

    // --- ANIMATION EVENTS POZIVAJU OVO ---
    public void PlayBeat1()
    {
        if (isDead || beat1 == null || audioSource == null) return;
        audioSource.PlayOneShot(beat1, currentVolume);
    }

    public void PlayBeat2()
    {
        if (isDead || beat2 == null || audioSource == null) return;
        audioSource.PlayOneShot(beat2, currentVolume);
    }

    // --- SMRT ---
    public void PlayFlatline()
    {
        if (audioSource == null || flatlineSound == null) return;

        isDead = true;
        audioSource.Stop();
        audioSource.pitch = 1f;

        if (heartAnimator != null) heartAnimator.speed = 1f;

        audioSource.clip = flatlineSound;
        audioSource.volume = baseVolume;
        audioSource.loop = false;
        audioSource.Play();
    }
}