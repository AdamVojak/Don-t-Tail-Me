using UnityEngine;

public class DoorAudio : MonoBehaviour
{
    [Header("Audio Sources (3D)")]
    [SerializeField] private AudioSource loopSource; // Za struganje metala
    [SerializeField] private AudioSource hitSource;  // Za udarac na kraju

    [Header("Audio Clips")]
    [SerializeField] private AudioClip metalScrapeLoop;
    [SerializeField] private AudioClip doorSlamHit;

    [Header("Postavke Glasnoće")]
    [Range(0f, 1f)][SerializeField] private float scrapeVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float slamVolume = 1f;

    private void Awake()
    {
        // Automatsko kreiranje ako nisu dodijeljeni
        if (loopSource == null) loopSource = gameObject.AddComponent<AudioSource>();
        if (hitSource == null) hitSource = gameObject.AddComponent<AudioSource>();

        Setup3DSource(loopSource, true);  // Loop = true
        Setup3DSource(hitSource, false);  // Loop = false
    }

    private void Setup3DSource(AudioSource source, bool isLoop)
    {
        source.spatialBlend = 1f; // 3D zvuk
        source.playOnAwake = false;
        source.loop = isLoop;
        source.minDistance = 2f;
        source.maxDistance = 20f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>
    /// Poziva se u trenutku kada se vrata počnu pomicati.
    /// </summary>
    public void StartMoving()
    {
        if (loopSource != null && metalScrapeLoop != null)
        {
            if (!loopSource.isPlaying)
            {
                loopSource.clip = metalScrapeLoop;
                loopSource.volume = scrapeVolume;
                loopSource.Play();
            }
        }
    }
    public void StopMoving()
    {
        // 1. Gasi loop struganja
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }

        // 2. Pušta glasan udarac (Slam)
        if (hitSource != null && doorSlamHit != null)
        {
            hitSource.pitch = Random.Range(0.95f, 1.05f); // Mala varijacija da ne zvuči uvijek identično
            hitSource.PlayOneShot(doorSlamHit, slamVolume);
        }
    }

    private void OnDisable()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }
}