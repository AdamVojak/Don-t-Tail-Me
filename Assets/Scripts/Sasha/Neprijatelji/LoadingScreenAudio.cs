using UnityEngine;

public class LoadingScreenAudio : MonoBehaviour
{
    [Header("Audio Source Reference (2D)")]
    [Tooltip("AudioSource posvećen samo zvuku paljenja/gašenja")]
    [SerializeField] private AudioSource computerSource;

    [Tooltip("AudioSource za mjehuriće, klikove i akord")]
    [SerializeField] private AudioSource sfxSource;

    [Header("1. Zvukovi Kompjutera")]
    [SerializeField] private AudioClip computerStartupClip;
    [SerializeField] private AudioClip computerShutdownClip;
    [Range(0f, 1f)][SerializeField] private float computerVolume = 1f;

    [Header("2. Zvukovi Odabira Igrača")]
    [Tooltip("Brzi 'klik' zvuk pri prelasku s jednog lika na drugog")]
    [SerializeField] private AudioClip playerSwitchClickClip;
    [Range(0f, 1f)][SerializeField] private float clickVolume = 0.7f;

    [Tooltip("Goofy akord kada se uspješno odabere igrač")]
    [SerializeField] private AudioClip playerSelectedChordClip;
    [Range(0f, 1f)][SerializeField] private float chordVolume = 1f;

    [Header("3. Zvukovi Mjehurića (Tranzicija)")]
    [SerializeField] private AudioClip[] transitionBubblesClips;
    [Range(0f, 1f)][SerializeField] private float bubblesVolume = 0.85f;

    [Header("4. Flicker / Treperenje Ekrana")]
    [SerializeField] private AudioClip flickerClip;
    [Range(0f, 1f)][SerializeField] private float flickerVolume = 0.8f;

    private void Awake()
    {
        // Ako nisi ručno dodijelio AudioSource-ove, skripta ih sama kreira/poveže
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length >= 2)
        {
            if (computerSource == null) computerSource = sources[0];
            if (sfxSource == null) sfxSource = sources[1];
        }
        else
        {
            if (computerSource == null) computerSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Osiguravamo da su oba 100% 2D zvukovi za UI
        Setup2DSource(computerSource);
        Setup2DSource(sfxSource);
    }

    public void PlayFlicker()
    {
        if (sfxSource == null || flickerClip == null) return;
        sfxSource.PlayOneShot(flickerClip, flickerVolume);
    }
    private void Setup2DSource(AudioSource source)
    {
        if (source == null) return;
        source.spatialBlend = 0f; // 2D
        source.playOnAwake = false;
        source.loop = false;
    }

    // =========================================================================
    // 1. FUNKCIJE ZA KOMPJUTER (Međusobno se prekidaju)
    // =========================================================================

    /// <summary>
    /// Pali zvuk paljenja kompjutera i PREKIDA zvuk gašenja ako još traje
    /// </summary>
    public void PlayComputerStartup()
    {
        if (computerSource == null || computerStartupClip == null) return;

        computerSource.Stop(); // Zaustavlja gašenje ako je u tijeku
        computerSource.clip = computerStartupClip;
        computerSource.volume = computerVolume;
        computerSource.Play();
    }

    /// <summary>
    /// Pali zvuk gašenja kompjutera i PREKIDA zvuk paljenja ako još traje
    /// </summary>
    public void PlayComputerShutdown()
    {
        if (computerSource == null || computerShutdownClip == null) return;

        computerSource.Stop(); // Zaustavlja paljenje ako je u tijeku
        computerSource.clip = computerShutdownClip;
        computerSource.volume = computerVolume;
        computerSource.Play();
    }

    // =========================================================================
    // 2. FUNKCIJE ZA ODABIR LIKA
    // =========================================================================

    /// <summary>
    /// Mali klik zvuk dok se lista/prebacuje između likova
    /// </summary>
    public void PlayPlayerSwitchClick()
    {
        if (sfxSource == null || playerSwitchClickClip == null) return;

        // Mali nasumični pitch za klikove da zvuče dinamičnije dok se brzo vrte
        sfxSource.pitch = Random.Range(0.96f, 1.04f);
        sfxSource.PlayOneShot(playerSwitchClickClip, clickVolume);
    }

    /// <summary>
    /// Goofy akord kad se dođe do željenog igrača i izbor se zaključa
    /// </summary>
    public void PlayPlayerSelectedChord()
    {
        if (sfxSource == null || playerSelectedChordClip == null) return;

        sfxSource.pitch = 1f; // Vraćamo standardni pitch za akord
        sfxSource.PlayOneShot(playerSelectedChordClip, chordVolume);
    }

    // =========================================================================
    // 3. FUNKCIJE ZA TRANZICIJU (Mjehurići)
    // =========================================================================

    /// <summary>
    /// Zvuk mjehurića kada se zid diže na kraju (bira nasumično 1 od 2 zvuka)
    /// </summary>
    public void PlayTransitionBubbles()
    {
        if (sfxSource == null || transitionBubblesClips == null || transitionBubblesClips.Length == 0) return;

        int randomIndex = Random.Range(0, transitionBubblesClips.Length);
        AudioClip selectedBubbles = transitionBubblesClips[randomIndex];

        if (selectedBubbles != null)
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(selectedBubbles, bubblesVolume);
        }
    }
}