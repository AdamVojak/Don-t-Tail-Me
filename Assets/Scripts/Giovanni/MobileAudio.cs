using UnityEngine;

public class MobitelAudio : MonoBehaviour
{
    [Header("Audio Source (2D)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvučni Signali Mobitela")]
    [SerializeField] private AudioClip pingFwdClip;
    [SerializeField] private AudioClip pingLeftClip;
    [SerializeField] private AudioClip pingRightClip;
    [SerializeField] private AudioClip errorClip;

    [Range(0f, 1f)][SerializeField] private float errorVolume = 0.75f;

    [Header("Postavke Stereo Pan-a")]
    [Tooltip("Vrijednost koliko zvuk ide u lijevo/desno uho (0.65)")]
    [Range(0f, 1f)][SerializeField] private float sidePanAmount = 0.65f;
    [Range(0f, 1f)][SerializeField] private float volume = 0.85f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f; // 2D zvuk
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    /// <summary>
    /// Ravno naprijed (Sredina: pan = 0)
    /// </summary>
    public void PlayForwardPing()
    {
        if (audioSource == null || pingFwdClip == null) return;
        audioSource.panStereo = 0f; // Čisti centar
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(pingFwdClip, volume);
    }

    /// <summary>
    /// Skretanje ulijevo (Pan = -0.65)
    /// </summary>
    public void PlayLeftPing()
    {
        if (audioSource == null || pingLeftClip == null) return;
        audioSource.panStereo = -sidePanAmount; // -0.65 u lijevo uho
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(pingLeftClip, volume);
    }

    /// <summary>
    /// Skretanje udesno (Pan = +0.65)
    /// </summary>
    public void PlayRightPing()
    {
        if (audioSource == null || pingRightClip == null) return;
        audioSource.panStereo = sidePanAmount; // +0.65 u desno uho
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(pingRightClip, volume);
    }

    /// <summary>
    /// Error zvuk kada nema cilja (Sredina: pan = 0)
    /// </summary>
    public void PlayError()
    {
        if (audioSource == null || errorClip == null) return;
        audioSource.panStereo = 0f; // Čisti centar
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(errorClip, errorVolume);
    }
}