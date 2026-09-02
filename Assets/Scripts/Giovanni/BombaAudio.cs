using UnityEngine;

public class BombaAudio : MonoBehaviour
{
    [Header("Audio Source (3D)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvukovi Lanca (3 Varijacije)")]
    [Tooltip("Ubaci svoja 3 zvuka dodira/zveckanja lanca")]
    [SerializeField] private AudioClip[] chainRattleClips;

    [Header("Postavke")]
    [Range(0f, 1f)][SerializeField] private float volume = 0.85f;

    private int zadnjiIndex = -1; // Pamti zadnji odsvirani zvuk da se ne ponavlja

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f; // 100% 3D zvuk
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 25f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>
    /// Pušta 1 od 3 zvuka lanca uz garanciju da se isti zvuk ne ponavlja zaredom
    /// </summary>
    public void PlayChainHit()
    {
        if (chainRattleClips == null || chainRattleClips.Length == 0 || audioSource == null) return;

        int noviIndex;

        // Ako ima više od 1 zvuka, biraj dok ne nađeš različit od zadnjeg
        if (chainRattleClips.Length > 1)
        {
            do
            {
                noviIndex = Random.Range(0, chainRattleClips.Length);
            } while (noviIndex == zadnjiIndex);
        }
        else
        {
            noviIndex = 0;
        }

        zadnjiIndex = noviIndex;

        audioSource.pitch = Random.Range(0.95f, 1.05f); // Mala prirodna varijacija
        audioSource.PlayOneShot(chainRattleClips[noviIndex], volume);
    }
}