using System.Collections;
using UnityEngine;
using static GiovanniController;

public class GiovanniAudio : MonoBehaviour
{
    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource stingerSource;

    [Header("1. Koraci (Lijeva / Desna noga)")]
    [SerializeField] private AudioClip[] footstepClips; // 2 zvuka koraka (0 = Lijeva, 1 = Desna)
    [Range(0f, 1f)][SerializeField] private float footstepVolume = 0.6f;

    [Header("2. Svjetiljka")]
    [SerializeField] private AudioClip flashlightClickClip;
    [Range(0f, 1f)][SerializeField] private float flashlightVolume = 0.8f;

    [Header("3. ViperFish (Cim & Svjetlo)")]
    [Tooltip("2 zvuka prolijetanja ribe preko ekrana (Cim)")]
    [SerializeField] private AudioClip[] viperFlybyClips;
    [Tooltip("Zvuk kada svjetiljkom osvijetli i uplaši ViperFish")]
    [SerializeField] private AudioClip viperLightReactionClip;
    [Range(0f, 1f)][SerializeField] private float viperVolume = 1f;

    [Header("4. Oružje i Predmeti")]
    [SerializeField] private AudioClip pajserMetalHitClip;
    [SerializeField] private AudioClip itemPickupClip;
    [Range(0f, 1f)][SerializeField] private float itemVolume = 0.8f;

    [Header("5. Podvodni Ambijent (Brown Noise)")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip brownNoiseClip;
    [Range(0f, 1f)][SerializeField] private float brownNoiseVolume = 0.25f;

    [Tooltip("Trajanje fade-in efekta u sekundama (60s = 1 minuta za polagano uranjanje u dubinu)")]
    [SerializeField] private float brownNoiseFadeDuration = 60f; // Postavljeno na 60 sekundi!

    private Coroutine brownNoiseFadeCoroutine;

    private void Awake()
    {
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (footstepSource == null) footstepSource = gameObject.AddComponent<AudioSource>();
        if (stingerSource == null) stingerSource = gameObject.AddComponent<AudioSource>();
        if (ambientSource == null) ambientSource = gameObject.AddComponent<AudioSource>();

        Setup2D(sfxSource);
        Setup2D(footstepSource);
        Setup2D(stingerSource);
        Setup2D(ambientSource);

        ambientSource.loop = true;
    }

    private void Setup2D(AudioSource source)
    {
        source.spatialBlend = 0f; // 2D zvuk
        source.playOnAwake = false;
        source.loop = false;
    }

    // ==========================================
    // METODE ZA POZIVANJE
    // ==========================================

    public void PlayFootstep(int stepIndex)
    {
        if (footstepClips == null || footstepClips.Length == 0 || footstepSource == null) return;
        int index = Mathf.Clamp(stepIndex, 0, footstepClips.Length - 1);
        footstepSource.pitch = Random.Range(0.95f, 1.05f);
        footstepSource.PlayOneShot(footstepClips[index], footstepVolume);
    }

    public void PlayFlashlightClick()
    {
        if (sfxSource != null && flashlightClickClip != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(flashlightClickClip, flashlightVolume);
        }
    }

    public void PlayViperFlyby()
    {
        if (viperFlybyClips == null || viperFlybyClips.Length == 0 || stingerSource == null) return;
        int randomIndex = Random.Range(0, viperFlybyClips.Length);
        stingerSource.pitch = Random.Range(0.95f, 1.05f);
        stingerSource.PlayOneShot(viperFlybyClips[randomIndex], viperVolume);
    }

    public void PlayViperLightReaction()
    {
        if (stingerSource != null && viperLightReactionClip != null)
        {
            stingerSource.pitch = Random.Range(0.95f, 1.05f);
            stingerSource.PlayOneShot(viperLightReactionClip, viperVolume);
        }
    }

    public void PlayPajserHit()
    {
        if (sfxSource != null && pajserMetalHitClip != null)
            sfxSource.PlayOneShot(pajserMetalHitClip, itemVolume);
    }

    public void PlayItemPickup()
    {
        if (sfxSource != null && itemPickupClip != null)
            sfxSource.PlayOneShot(itemPickupClip, itemVolume);
    }

    public void StartBrownNoise()
    {
        if (ambientSource.isPlaying) return;

        if (brownNoiseFadeCoroutine != null) StopCoroutine(brownNoiseFadeCoroutine);
        brownNoiseFadeCoroutine = StartCoroutine(FadeInBrownNoiseRoutine(brownNoiseFadeDuration));
    }

    private IEnumerator FadeInBrownNoiseRoutine(float duration)
    {
        ambientSource.clip = brownNoiseClip;
        ambientSource.loop = true;
        ambientSource.volume = 0f;
        ambientSource.Play();

        float elapsed = 0f;

        if (duration <= 0f)
        {
            ambientSource.volume = brownNoiseVolume;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, brownNoiseVolume, elapsed / duration);
            yield return null;
        }

        ambientSource.volume = brownNoiseVolume;
        brownNoiseFadeCoroutine = null;
    }

    public void StopBrownNoise()
    {
        if (brownNoiseFadeCoroutine != null)
        {
            StopCoroutine(brownNoiseFadeCoroutine);
            brownNoiseFadeCoroutine = null;
        }

        if (ambientSource != null && ambientSource.isPlaying)
        {
            ambientSource.Stop();
            ambientSource.volume = 0f;
        }
    }

    private void OnDisable()
    {
        StopBrownNoise();
    }
}