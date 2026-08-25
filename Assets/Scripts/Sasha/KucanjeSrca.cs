using UnityEngine;

public class KucanjeSrca: MonoBehaviour
{
    [Header("Audio Source")]
    [Tooltip("Možeš ga ostaviti praznim ako je AudioSource na istom ovom UI objektu")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvučni Efekti Srca")]
    [SerializeField] private AudioClip beat1;
    [SerializeField] private AudioClip beat2;
    [SerializeField] private AudioClip flatlineSound;

    [SerializeField] private float razlikaGlasnoce = 2.5f;

    [Header("Glasnoća")]
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private bool isDead = false;

    private void Awake()
    {
        // Ako nisi ručno dohvatio AudioSource, skripta ga sama uzme
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Osiguravamo da je zvuk 100% 2D jer je ovo sučelje (UI)
        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f; // 2D zvuk
            audioSource.playOnAwake = false;
        }
    }

    public void PlayBeat1()
    {
        if (isDead || beat1 == null || audioSource == null) return;
        audioSource.PlayOneShot(beat1, volume);
    }


    public void PlayBeat2()
    {
        if (isDead || beat2 == null || audioSource == null) return;
        audioSource.PlayOneShot(beat2, volume);
    }


    public void PlayFlatline()
    {
        if (audioSource == null || flatlineSound == null) return;

        isDead = true;

        // Zaustavi bilo kakav zvuk koji trenutno svira
        audioSource.Stop();

        // Pusti zvuk pištanja
        audioSource.clip = flatlineSound;
        audioSource.volume = volume - razlikaGlasnoce;
        audioSource.loop = false; // Stavi na true ako imaš kratki audio file koji želiš da pišti u krug
        audioSource.Play();
    }
}