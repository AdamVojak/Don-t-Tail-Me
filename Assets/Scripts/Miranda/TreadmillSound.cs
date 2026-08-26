using UnityEngine;

public class TreadmillAudio : MonoBehaviour
{
    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Audio Clips")]
    [Tooltip("Zvuk pokretanja trake (Inicijalni klik / pokret)")]
    [SerializeField] private AudioClip startClip;

    [Tooltip("Zvuk koji se vrti u loopu dok Miranda trči i puni struju")]
    [SerializeField] private AudioClip loopClip;

    [Tooltip("Zvuk zaustavljanja / usporavanja trake")]
    [SerializeField] private AudioClip stopClip;

    [Header("Postavke Glasnoće")]
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

    private void Awake()
    {
        // Automatsko kreiranje ako nisu dodijeljeni u Inspectoru
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (loopSource == null) loopSource = gameObject.AddComponent<AudioSource>();

        Setup2DSource(sfxSource, false);
        Setup2DSource(loopSource, true); // Loop = true za neprekidni rad
    }

    private void Setup2DSource(AudioSource source, bool isLoop)
    {
        source.spatialBlend = 0f; // 2D zvuk
        source.playOnAwake = false;
        source.loop = isLoop;
    }

    public void StartRunning()
    {
        // 1. Jednokratni zvuk pokretanja
        if (sfxSource != null && startClip != null)
        {
            sfxSource.PlayOneShot(startClip, volume);
        }

        // 2. Pokretanje loopa vrtnje
        if (loopSource != null && loopClip != null)
        {
            loopSource.clip = loopClip;
            loopSource.volume = volume;
            loopSource.Play();
        }
    }

    public void StopRunning()
    {
        // 1. Gašenje loopa
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }

        // 2. Jednokratni zvuk zaustavljanja (Outro)
        if (sfxSource != null && stopClip != null)
        {
            sfxSource.PlayOneShot(stopClip, volume);
        }
    }

    private void OnDisable()
    {
        // Sigurnosno gašenje ako se level ugasi ili prebaci lik
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }
}