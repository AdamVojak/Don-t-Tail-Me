using UnityEngine;

public class MirandaAudio : MonoBehaviour
{
    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource wheelSource;

    [Header("1. Ključevi i Predmeti (Pickup & Use)")]
    [SerializeField] private AudioClip yellowKeyPickupClip;
    [SerializeField] private AudioClip purpleKeyPickupClip;
    [SerializeField] private AudioClip keyJiggleClip;
    [SerializeField] private AudioClip genericPickupClip;
    [SerializeField] private AudioClip yellowKeyUseClip;
    [SerializeField] private AudioClip purpleKeyUseClip;
    [SerializeField] private AudioClip keyUseClip;

    [Header("Error zvuk")]
    [SerializeField] private AudioClip errorClip;

    [Range(0f, 1f)] public float errorVolume = 0.8f;


    public float kljuceviVolume = 1.0f;

    [Header("2. Kretanje Kotača (Whoosh)")]
    [Tooltip("2 zvuka okretanja kotača koji se izmjenjuju")]
    [SerializeField] private AudioClip[] wheelWhooshClips;

    public float whooshVolume = 1f;

    [Header("3. Skok i Slijetanje")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    public float landVolume = 0.8f;
    public float jumpVolume = 0.5f;

    [Header("4. Robotska Ruka i Šaka")]
    [SerializeField] private AudioClip armActivateClip;
    [SerializeField] private AudioClip armDeactivateClip;
    [SerializeField] private AudioClip fistCloseClip;
    [SerializeField] private AudioClip fistOpenClip;
    [SerializeField] private AudioClip armErrorClip;

    public float armVolume = 0.7f;

    [Header("Postavke")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;

    [Header("5. Interactive Meni (Muzak & UI)")]
    [SerializeField] private AudioSource muzakSource;
    [SerializeField] private AudioClip muzakClip;
    [SerializeField] private AudioClip itemDropClip;
    [SerializeField] private AudioClip navClickClip;
    [SerializeField] private AudioClip confirmClickClip;

    public float interactiveVolume = 0.7f;
    public float muzakVolume = 0.9f;

    private void Awake()
    {
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (wheelSource == null) wheelSource = gameObject.AddComponent<AudioSource>();
        if (muzakSource == null) muzakSource = gameObject.AddComponent<AudioSource>();

        Setup2DSource(sfxSource);
        Setup2DSource(wheelSource);
        Setup2DSource(muzakSource);

        muzakSource.loop = true;
    }

    private void Setup2DSource(AudioSource source)
    {
        source.spatialBlend = 0f; // 2D zvuk
        source.playOnAwake = false;
        source.loop = false;
    }

    private void PlaySFX(AudioClip clip, float volume = 1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, volume * masterVolume);
    }

    // ==========================================
    // 1. PREDMETI I KLJUČEVI
    // ==========================================
    public void PlayYellowKeyPickup() => PlaySFX(yellowKeyPickupClip, kljuceviVolume);
    public void PlayPurpleKeyPickup() => PlaySFX(purpleKeyPickupClip, kljuceviVolume);
    public void PlayGenericPickup() => PlaySFX(genericPickupClip, kljuceviVolume);
    public void PlayYellowKeyUse() => PlaySFX(yellowKeyUseClip, kljuceviVolume);
    public void PlayPurpleKeyUse() => PlaySFX(purpleKeyUseClip, kljuceviVolume);
    public void PlayKeyUse() => PlaySFX(keyUseClip, kljuceviVolume);
    public void PlayKeyJiggle() => PlaySFX(keyJiggleClip, kljuceviVolume);
    public void PlayError() => PlaySFX(errorClip, errorVolume);

    // ==========================================
    // 2. KOTAČ (Whoosh)
    // ==========================================
    public void PlayWheelWhoosh()
    {
        if (wheelWhooshClips == null || wheelWhooshClips.Length == 0 || wheelSource == null) return;
        wheelSource.pitch = Random.Range(0.85f, 1.00f);
        wheelSource.PlayOneShot(wheelWhooshClips[Random.Range(0, wheelWhooshClips.Length)], whooshVolume);
    }

    // ==========================================
    // 3. FIZIKA (Skok i Slijetanje)
    // ==========================================
    public void PlayJump() => PlaySFX(jumpClip, jumpVolume);
    public void PlayLand() => PlaySFX(landClip, landVolume);

    // ==========================================
    // 4. ROBOTSKA RUKA
    // ==========================================
    public void PlayArmActivate() => PlaySFX(armActivateClip, armVolume);
    public void PlayArmDeactivate() => PlaySFX(armDeactivateClip, armVolume);
    public void PlayFistClose() => PlaySFX(fistCloseClip, armVolume);
    public void PlayFistOpen() => PlaySFX(fistOpenClip, armVolume);
    public void PlayArmError() => PlaySFX(armErrorClip, armVolume, 1f, 1f);

    // ==========================================
    // 5. INTERACTIVE STATE (Muzak & Meni)
    // ==========================================
    public void EnterInteractiveState()
    {
        if (muzakSource != null && muzakClip != null)
        {
            muzakSource.clip = muzakClip;
            muzakSource.volume = muzakVolume;
            muzakSource.Play();
        }
    }

    public void ExitInteractiveState()
    {
        if (muzakSource != null) muzakSource.Stop();
    }

    public void PlayNavClick() => PlaySFX(navClickClip, interactiveVolume, 0.95f, 1.05f);
    public void PlayConfirmClick() => PlaySFX(confirmClickClip, interactiveVolume);
    public void PlayItemDrop() => PlaySFX(itemDropClip, interactiveVolume);

    private void OnDisable()
    {
        ExitInteractiveState(); // Sigurnosno gašenje Muzaka
    }
}