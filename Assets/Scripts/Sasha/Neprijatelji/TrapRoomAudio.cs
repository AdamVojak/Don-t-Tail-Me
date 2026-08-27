using UnityEngine;

public class TrapRoomAudio : MonoBehaviour
{
    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource sirenSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip sirenLoopClip;
    [SerializeField] private AudioClip powerOutageClip;

    [Header("Postavke Glasnoće")]
    [Range(0f, 1f)][SerializeField] private float sirenVolume = 0.4f;
    [Range(0f, 1f)][SerializeField] private float outageVolume = 0.8f;

    private void Awake()
    {
        // Automatsko kreiranje ako nisu dodijeljeni
        if (sirenSource == null) sirenSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        Setup2DSource(sirenSource, true);  // Loop = true
        Setup2DSource(sfxSource, false); // Loop = false
    }

    private void Setup2DSource(AudioSource source, bool isLoop)
    {
        source.spatialBlend = 0f; // 2D zvuk
        source.playOnAwake = false;
        source.loop = isLoop;
    }

    /// <summary>
    /// Pali se čim igrač pokupi predmet i zamka se aktivira.
    /// </summary>
    public void StartSiren()
    {
        if (sirenSource != null && sirenLoopClip != null)
        {
            sirenSource.clip = sirenLoopClip;
            sirenSource.volume = sirenVolume;
            sirenSource.Play();
        }
    }
    public void StopSirenAndPlayOutage()
    {
        // 1. Gasi sirenu
        if (sirenSource != null && sirenSource.isPlaying)
        {
            sirenSource.Stop();
        }

        // 2. Pušta zvuk nestanka struje (Power Outage)
        if (sfxSource != null && powerOutageClip != null)
        {
            sfxSource.PlayOneShot(powerOutageClip, outageVolume);
        }
    }

    public void StopSiren()
    {
        if (sirenSource != null && sirenSource.isPlaying)
        {
            sirenSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (sirenSource != null && sirenSource.isPlaying)
        {
            sirenSource.Stop();
        }
    }
}