using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Postavke uništenja")]
    [SerializeField] private GameObject brokenVersionPrefab;

    [Header("Precizna interakcija (Opcionalno)")]
    public Collider specificTargetCollider;

    [Header("Audio (3D Zvuk Udarca Pajserom / Loma)")]
    [Tooltip("Ubaci 1 ili više zvukova loma/udarca pajserom")]
    [SerializeField] private AudioClip[] hitClips;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    void Start()
    {
        if (brokenVersionPrefab != null)
        {
            brokenVersionPrefab.SetActive(false);
        }
    }

    public void DestroyAndReplace()
    {
        if (brokenVersionPrefab != null)
        {
            // 1. Odvajamo razbijenu verziju i palimo je u sceni
            brokenVersionPrefab.transform.parent = null;
            brokenVersionPrefab.SetActive(true);

            // 2. STAVLJAMO 3D ZVUK NA RAZBIJENU VERZIJU (Jer ona ostaje živa u sceni!):
            AudioSource audio = brokenVersionPrefab.GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = brokenVersionPrefab.AddComponent<AudioSource>();
                audio.spatialBlend = 1f; // 100% 3D zvuk na toj točki
                audio.playOnAwake = false;
                audio.minDistance = 2f;
                audio.maxDistance = 20f;
                audio.rolloffMode = AudioRolloffMode.Logarithmic;
            }

            // 3. Puštamo zvuk udarca
            if (hitClips != null && hitClips.Length > 0)
            {
                AudioClip clip = hitClips[Random.Range(0, hitClips.Length)];
                audio.pitch = Random.Range(0.95f, 1.05f); // Mala varijacija da svaki udarac zvuči unikatno
                audio.PlayOneShot(clip, volume);
            }
        }

        // 4. Stari objekt se sigurno briše, a zvuk na novom objektu nesmetano svira do kraja!
        Destroy(gameObject);
    }
}