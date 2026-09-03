using UnityEngine;
using System.Collections;

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
    [SerializeField] private AudioClip computerRunningLoopClip;
    [Range(0f, 1f)][SerializeField] private float computerVolume = 1f;
    private Coroutine computerSequenceCoroutine;

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

    [Header("5. Zvukovi Gumbiju (Pritisak i Vraćanje)")]
    [Tooltip("Zvuk kada gumb ide unutra (Press)")]
    [SerializeField] private AudioClip buttonClickClip;

    [Tooltip("Zvuk kada gumb iskoči natrag van (Release)")]
    [SerializeField] private AudioClip buttonReleaseClip;

    [Header("6. Flicker / Treperenje Ekrana")]
    [SerializeField] private AudioClip confettiClip;
    [Range(0f, 1f)][SerializeField] private float confettiVolume = 0.8f;

    [Range(0f, 1f)][SerializeField] private float buttonClickVolume = 0.85f;
    [Range(0f, 1f)][SerializeField] private float buttonDeClickVolume = 0.85f;

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
    // 1. FUNKCIJE ZA KOMPJUTER (Startup -> Loop rada -> Shutdown prekid)
    // =========================================================================

    /// <summary>
    /// Pokreće zvuk paljenja i čim on završi, automatski pokreće loop rada računala
    /// </summary>
    public void PlayComputerStartup()
    {
        if (computerSource == null || computerStartupClip == null) return;

        // Ako se nešto već vrtjelo, prekidamo i krećemo ispočetka
        if (computerSequenceCoroutine != null) StopCoroutine(computerSequenceCoroutine);
        computerSequenceCoroutine = StartCoroutine(ComputerStartupSequenceRoutine());
    }

    private IEnumerator ComputerStartupSequenceRoutine()
    {
        // 1. KORAK: Puštamo zvuk paljenja (One-shot)
        computerSource.Stop();
        computerSource.loop = false;
        computerSource.clip = computerStartupClip;
        computerSource.volume = computerVolume;
        computerSource.Play();

        // Čekamo točno onoliko sekundi koliko traje tvoj startup audio zapis
        yield return new WaitForSeconds(computerStartupClip.length);

        // 2. KORAK: Čim paljenje završi, prebacujemo se na stalni loop rada računala
        if (computerRunningLoopClip != null)
        {
            computerSource.clip = computerRunningLoopClip;
            computerSource.loop = true;
            computerSource.Play();
        }

        computerSequenceCoroutine = null;
    }

    /// <summary>
    /// Trenutno prekida bilo startup bilo loop rada i pušta zvuk gašenja
    /// </summary>
    public void PlayComputerShutdown()
    {
        if (computerSource == null || computerShutdownClip == null) return;

        // Ako je sekvenca paljenja ili loop još u tijeku, trenutno ga zaustavi
        if (computerSequenceCoroutine != null)
        {
            StopCoroutine(computerSequenceCoroutine);
            computerSequenceCoroutine = null;
        }

        // 3. KORAK: Puštamo zvuk gašenja (prekida sve prethodno)
        computerSource.Stop();
        computerSource.loop = false;
        computerSource.clip = computerShutdownClip;
        computerSource.volume = computerVolume;
        computerSource.Play();
    }

    private void OnDisable()
    {
        // Sigurnosno gašenje ako se cijeli ekran ugasi
        if (computerSequenceCoroutine != null)
        {
            StopCoroutine(computerSequenceCoroutine);
            computerSequenceCoroutine = null;
        }

        if (computerSource != null && computerSource.isPlaying)
        {
            computerSource.Stop();
        }
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

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickClip != null)
        {
            sfxSource.pitch = Random.Range(0.97f, 1.03f); // Mala varijacija da zvuči prirodno
            sfxSource.PlayOneShot(buttonClickClip, buttonClickVolume);
        }
    }

    public void PlayConfetti()
    {
        if (sfxSource != null && confettiClip != null)
        {
            sfxSource.pitch = Random.Range(0.97f, 1.03f); // Mala varijacija da zvuči prirodno
            sfxSource.PlayOneShot(confettiClip, confettiVolume);
        }
    }

    public void PlayButtonRelease()
    {
        if (sfxSource != null && buttonReleaseClip != null)
        {
            sfxSource.pitch = Random.Range(0.97f, 1.03f);
            sfxSource.PlayOneShot(buttonReleaseClip, buttonDeClickVolume);
        }
    }
}