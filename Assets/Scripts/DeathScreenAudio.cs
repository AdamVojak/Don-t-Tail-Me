using System.Collections;
using UnityEngine;

public class DeathScreenAudio : MonoBehaviour
{
    [Header("Audio Source Reference (2D)")]
    [Tooltip("AudioSource za neprekidni TV Static loop")]
    [SerializeField] private AudioSource staticSource;

    [Tooltip("AudioSource za klikove, pilu i kosti")]
    [SerializeField] private AudioSource sfxSource;

    [Header("1. TV Static Postavke (Loop)")]
    [Tooltip("Ovdje stavi 2 varijacije TV statike")]
    [SerializeField] private AudioClip[] staticVariations;
    [Range(0f, 1f)][SerializeField] private float defaultStaticVolume = 0.5f;
    [SerializeField] private float volumeChangeSpeed = 10f; // Koliko glatko prelazi glasnoća na hover

    [Header("2. Zvukovi Gumba")]
    [SerializeField] private AudioClip buttonClickClip;
    [Range(0f, 1f)][SerializeField] private float buttonClickVolume = 1f;

    [Header("3. Zvukovi Kursora (Pila i Kost)")]
    [SerializeField] private AudioClip handSawClip;
    [Range(0f, 1f)][SerializeField] private float handSawVolume = 1f;

    [SerializeField] private AudioClip boneCrackClip;
    [Range(0f, 1f)][SerializeField] private float boneCrackVolume = 1f;

    private float targetStaticVolume = 0.5f;
    private Coroutine fadeInCoroutine;

    private void Awake()
    {
        // Automatsko pronalaženje ili kreiranje ako nedostaju
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            if (staticSource == null) staticSource = sources[0];
            if (sfxSource == null) sfxSource = sources[1];
        }
        else
        {
            if (staticSource == null) staticSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Postavke za 2D
        SetupSource(staticSource, isLoop: true);
        SetupSource(sfxSource, isLoop: false);

        targetStaticVolume = defaultStaticVolume;
    }

    private void SetupSource(AudioSource source, bool isLoop)
    {
        if (source == null) return;
        source.spatialBlend = 0f; // 2D zvuk
        source.playOnAwake = false;
        source.loop = isLoop;
    }

    private void Update()
    {
        // Glatko prilagođava glasnoću statike prema hover stanju
        if (staticSource != null && staticSource.isPlaying && fadeInCoroutine == null)
        {
            staticSource.volume = Mathf.MoveTowards(staticSource.volume, targetStaticVolume, volumeChangeSpeed * Time.deltaTime);
        }
    }

    // =========================================================================
    // 1. KONTROLA TV STATIKE (Loop, Fade-In i Gašenje)
    // =========================================================================

    public void StartStatic(int variationIndex = 0, bool useFadeIn = false, float fadeDuration = 0.5f)
    {
        if (staticSource == null || staticVariations == null || staticVariations.Length == 0) return;

        int index = Mathf.Clamp(variationIndex, 0, staticVariations.Length - 1);

        staticSource.clip = staticVariations[index];
        staticSource.loop = true;

        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);

        if (defaultStaticVolume <= 0.05f) defaultStaticVolume = 0.5f; // Sigurnosna provjera
        targetStaticVolume = defaultStaticVolume;

        if (useFadeIn)
        {
            fadeInCoroutine = StartCoroutine(FadeInStaticRoutine(fadeDuration));
        }
        else
        {
            staticSource.volume = defaultStaticVolume;
            staticSource.Play();
        }
    }

    private IEnumerator FadeInStaticRoutine(float duration)
    {
        staticSource.volume = 0f;
        staticSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            staticSource.volume = Mathf.Lerp(0f, defaultStaticVolume, elapsed / duration);
            yield return null;
        }

        staticSource.volume = defaultStaticVolume;
        fadeInCoroutine = null;
    }

    public void StopStatic()
    {
        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
        if (staticSource != null) staticSource.Stop();
    }

    // =========================================================================
    // 2. HOVER EFEKTI NA GUMBIMA (Promjena glasnoće statike)
    // =========================================================================

    /// <summary>
    /// Pozovi kad miš uđe iznad Exit gumba (+0.5 glasnije)
    /// </summary>
    public void OnHoverExitButton()
    {
        targetStaticVolume = Mathf.Clamp01(defaultStaticVolume + 0.01f);
    }

    /// <summary>
    /// Pozovi kad miš uđe iznad Restart gumba (-0.5 tiše)
    /// </summary>
    public void OnHoverRestartButton()
    {
        targetStaticVolume = Mathf.Clamp01(defaultStaticVolume - 0.01f);
    }

    /// <summary>
    /// Pozovi kad miš izađe izvan gumba (vraća se na normalu)
    /// </summary>
    public void OnHoverReset()
    {
        targetStaticVolume = defaultStaticVolume;
    }

    // =========================================================================
    // 3. JEDNOKRATNI ZVUČNI EFEKTI (SFX)
    // =========================================================================

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickClip != null)
        {
            sfxSource.PlayOneShot(buttonClickClip, buttonClickVolume);
        }
    }

    public void PlayHandSaw()
    {
        if (sfxSource != null && handSawClip != null)
        {
            sfxSource.PlayOneShot(handSawClip, handSawVolume);
        }
    }

    public void PlayBoneCrack()
    {
        if (sfxSource != null && boneCrackClip != null)
        {
            sfxSource.PlayOneShot(boneCrackClip, boneCrackVolume);
        }
    }
}