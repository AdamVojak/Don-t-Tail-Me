using System.Collections;
using UnityEngine;
using static GiovanniController;

public class GiovanniAudio : MonoBehaviour
{
    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource stingerSource;

    [SerializeField] private AudioClip bombExplosionClip; // 2D Zvuk eksplozije
    [Range(0f, 1f)][SerializeField] private float bombVolume = 0.8f;

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
    [Range(0f, 1f)][SerializeField] private float brownNoiseVolume = 0.5f;

    [Header("Trajanje Prelaza (Fade)")]
    [Tooltip("Koliko sekundi traje postepeno pojačavanje kad prebaciš na Giovannija (3 - 5s je idealno)")]
    [SerializeField] private float fadeInDuration = 4.0f;

    [Tooltip("Koliko sekundi traje postepeno stišavanje kad napustiš Giovannija")]
    [SerializeField] private float fadeOutDuration = 2.0f;

    private enum AmbientState { Stopped, FadingIn, Playing, FadingOut }
    private AmbientState ambientState = AmbientState.Stopped;

    private Coroutine ambientFadeCoroutine;

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
        if (ambientSource == null || brownNoiseClip == null) return;

        // AKO VEĆ SVIRA ILI JE FADE-IN VEĆ U TIJEKU -> NE DIRAJ NIŠTA I PUSTI GA DA SE POJAČA!
        if (ambientState == AmbientState.FadingIn || ambientState == AmbientState.Playing)
            return;

        if (ambientFadeCoroutine != null) StopCoroutine(ambientFadeCoroutine);

        ambientState = AmbientState.FadingIn;
        ambientFadeCoroutine = StartCoroutine(FadeAmbientRoutine(targetVol: brownNoiseVolume, duration: fadeInDuration, stopOnEnd: false));
    }

    /// <summary>
    /// Pokreće glatki Fade-Out ambijenta do nule i zatim ga gasi
    /// </summary>
    public void StopBrownNoise(bool instant = false)
    {
        if (ambientSource == null) return;

        // Ako je već ugašen ili već traje Fade-Out, ne radi ništa
        if (instant == false && (ambientState == AmbientState.Stopped || ambientState == AmbientState.FadingOut))
            return;

        if (ambientFadeCoroutine != null)
        {
            StopCoroutine(ambientFadeCoroutine);
            ambientFadeCoroutine = null;
        }

        // Instantno gašenje (npr. pri gašenju objekta)
        if (instant || !gameObject.activeInHierarchy)
        {
            ambientState = AmbientState.Stopped;
            ambientSource.Stop();
            ambientSource.volume = 0f;
            return;
        }

        // Glatko stišavanje
        ambientState = AmbientState.FadingOut;
        ambientFadeCoroutine = StartCoroutine(FadeAmbientRoutine(targetVol: 0f, duration: fadeOutDuration, stopOnEnd: true));
    }

    private IEnumerator FadeAmbientRoutine(float targetVol, float duration, bool stopOnEnd)
    {
        if (!ambientSource.isPlaying)
        {
            ambientSource.clip = brownNoiseClip;
            ambientSource.loop = true;
            ambientSource.volume = 0f;
            ambientSource.Play();
        }

        float startVol = ambientSource.volume;
        float elapsed = 0f;

        if (duration <= 0.05f)
        {
            ambientSource.volume = targetVol;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ambientSource.volume = Mathf.Lerp(startVol, targetVol, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }

        ambientSource.volume = targetVol;

        if (stopOnEnd)
        {
            ambientSource.Stop();
            ambientState = AmbientState.Stopped;
        }
        else
        {
            ambientState = AmbientState.Playing; // Uspješno postignut puni volumen!
        }

        ambientFadeCoroutine = null;
    }

    private void OnDisable()
    {
        StopBrownNoise(instant: true);
    }

    public void PlayBombExplosion()
    {
        if (sfxSource != null && bombExplosionClip != null)
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(bombExplosionClip, bombVolume);
        }
    }
}