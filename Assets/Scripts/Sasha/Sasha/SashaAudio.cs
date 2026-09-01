using System.Collections;
using UnityEngine;

public class SashaAudio : MonoBehaviour
{
    [Header("Audio Sources (2D)")]
    [Tooltip("Ako ostaviš prazno, skripta će ih sama stvoriti")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource pushSource;
    [SerializeField] private AudioSource muzakSource;

    [Header("1. Borba i Šteta")]
    [SerializeField] private AudioClip[] fistHitClips; // 2 zvuka udarca šakom
    [SerializeField] private AudioClip[] hurtClips;    // 2 zvuka boli
    [SerializeField] private AudioClip[] swingClips;   // 2 zvuka zamaha (Melee / Pajser)
    [SerializeField] private AudioClip wormBiteClip;

    public float hitVolume = 0.5f;
    public float hurtVolume = 0.75f;

    [Tooltip("Koliko kasni krik boli nakon udarca")]
    [Range(0.05f, 0.3f)][SerializeField] private float hurtDelay = 0.12f;
    private Coroutine hurtCoroutine;

    [Header("2. Oružja (Mijenjanje)")]
    [Tooltip("Stavi 4 zvuka redom kako se mijenjaju oružja")]
    [SerializeField] private AudioClip[] weaponSwitchClips;

    public float changeVolume = 0.75f;

    [Header("3. Guranje Objekata")]
    [SerializeField] private AudioClip pushStartClip;
    [SerializeField] private AudioClip pushLoopClip;
    [SerializeField] private AudioClip pushStopClip;

    public float pushingVolume = 0.75f;

    [Header("4. Interactive State (Muzak & UI)")]
    [SerializeField] private AudioClip muzakClip;
    [SerializeField] private AudioClip itemDropClip;
    [SerializeField] private AudioClip navClickClip;
    [SerializeField] private AudioClip confirmClickClip;

    public float interactiveVolume = 0.75f;
    public float muzakVolume = 0.8f;

    [Header("Postavke Glasnoće")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    private void Awake()
    {
        // Automatsko kreiranje AudioSource-ova ako nisu dodijeljeni
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (pushSource == null) pushSource = gameObject.AddComponent<AudioSource>();
        if (muzakSource == null) muzakSource = gameObject.AddComponent<AudioSource>();

        // Postavljanje na 2D zvuk (0)
        Setup2DSource(sfxSource, false);
        Setup2DSource(pushSource, true); // Loop za guranje
        Setup2DSource(muzakSource, true); // Loop za muzak
    }

    private void Setup2DSource(AudioSource source, bool isLoop)
    {
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.loop = isLoop;
    }

    // --- POMOĆNA METODA ZA NASUMIČNE ZVUKOVE ---
    private void PlayRandomSFX(AudioClip[] clips, float volume, float pitchMin = 0.95f, float pitchMax = 1.05f)
    {
        if (clips == null || clips.Length == 0) return;
        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    private void PlaySingleSFX(AudioClip clip, float volume, float pitch = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    // ==========================================
    // 1. BORBA I ŠTETA
    // ==========================================

    public void PlayFistHit()
    {
        PlayRandomSFX(fistHitClips, hitVolume);
    }

    public void PlayWormBite()
    {
        PlaySingleSFX(wormBiteClip, Random.Range(0.9f, 1.1f));
    }

    public void PlaySwing()
    {
        PlayRandomSFX(swingClips, 0.9f, 1.1f); // Malo veća varijacija pitcha za zamah
    }

    public void PlayHurtDelayed()
    {
        if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
        hurtCoroutine = StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(hurtDelay);
        PlayRandomSFX(hurtClips, hurtVolume, 0.95f, 1.05f);
        hurtCoroutine = null;
    }

    // ==========================================
    // 2. ORUŽJA
    // ==========================================

    public void PlayWeaponSwitch(int weaponIndex)
    {
        if (weaponSwitchClips != null && weaponIndex >= 0 && weaponIndex < weaponSwitchClips.Length)
        {
            PlaySingleSFX(weaponSwitchClips[weaponIndex], changeVolume);
        }
    }

    // ==========================================
    // 3. GURANJE OBJEKATA
    // ==========================================

    public void StartPushing()
    {
        PlaySingleSFX(pushStartClip, pushingVolume); // Zvuk hvatanja/početka

        if (pushSource != null && pushLoopClip != null)
        {
            pushSource.clip = pushLoopClip;
            pushSource.volume = pushingVolume;
            pushSource.Play(); // Kreće loop struganja
        }
    }

    public void StopPushing()
    {
        if (pushSource != null) pushSource.Stop(); // Gasi loop
        PlaySingleSFX(pushStopClip, pushingVolume); // Zvuk puštanja/zaustavljanja
    }

    // ==========================================
    // 4. INTERACTIVE STATE (Muzak & UI)
    // ==========================================

    public void EnterInteractiveState()
    {
        if (muzakSource != null && muzakClip != null)
        {
            muzakSource.clip = muzakClip;
            muzakSource.volume = masterVolume * muzakVolume; // Muzak malo tiši da ne probije uši
            muzakSource.Play();
        }
    }

    public void ExitInteractiveState()
    {
        if (muzakSource != null) muzakSource.Stop();
    }

    public void PlayItemDrop()
    {
        PlaySingleSFX(itemDropClip, interactiveVolume);
    }

    public void PlayNavClick()
    {
        // Mali random pitch da klikanje lijevo-desno zvuči dinamično
        PlaySingleSFX(navClickClip, interactiveVolume, Random.Range(0.95f, 1.05f));
    }

    public void PlayConfirmClick()
    {
        PlaySingleSFX(confirmClickClip, interactiveVolume, 1f);
    }

    public void StopAllLoops()
    {
        if (pushSource != null && pushSource.isPlaying) pushSource.Stop();
        if (muzakSource != null && muzakSource.isPlaying) muzakSource.Stop();
    }

    private void OnDisable()
    {
        StopAllLoops();
    }
}