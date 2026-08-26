using UnityEngine;

public class SpawnerAudio : MonoBehaviour
{
    [Header("Audio Source (3D)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [Tooltip("Zvuk kada se spawner aktivira / počne s radom")]
    [SerializeField] private AudioClip startClip;

    [Tooltip("Zvuk kada se spawner ugasi / deaktivira")]
    [SerializeField] private AudioClip stopClip;

    [Tooltip("Zvuk kada zombi ispadne / spawna se iz njega")]
    [SerializeField] private AudioClip spawnClip;

    [Header("Postavke Glasnoće")]
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        Setup3DSource(audioSource);
    }

    private void Setup3DSource(AudioSource source)
    {
        source.spatialBlend = 1f; // 3D zvuk
        source.playOnAwake = false;
        source.loop = false;
        source.minDistance = 2f;
        source.maxDistance = 25f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    public void PlayStartSound()
    {
        if (audioSource != null && startClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(startClip, volume);
        }
    }

    public void PlayStopSound()
    {
        if (audioSource != null && stopClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(stopClip, volume);
        }
    }

    public void PlaySpawnSound()
    {
        if (audioSource != null && spawnClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(spawnClip, volume);
        }
    }
}