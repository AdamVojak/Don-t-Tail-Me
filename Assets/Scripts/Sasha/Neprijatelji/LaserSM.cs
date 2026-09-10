using UnityEngine;

public class LaserSM : MonoBehaviour
{
    [Header("Fizička Prepreka")]
    public GameObject solidColliderDijete;

    [Header("Postavke Strujnog Udara")]
    public Sprite sashaSokiraniSprite;
    public float vrijemeStrujnogUdara = 1.0f;
    public float snagaOdbacivanjaY = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip strujaZvuk;
    [Range(0f, 1f)] public float strujaVolume = 0.8f;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (solidColliderDijete != null) solidColliderDijete.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Čim Sasha dotakne laser -> STRUJNI UDAR I ODBACIVANJE!
        SashaController sasha = other.GetComponent<SashaController>();

        if (sasha != null)
        {
            if (audioSource != null && strujaZvuk != null)
            {
                audioSource.PlayOneShot(strujaZvuk, strujaVolume);
            }

            // Šaljemo šok i odbacivanje
            sasha.PrimijeniStrujniUdar(snagaOdbacivanjaY, vrijemeStrujnogUdara, sashaSokiraniSprite);
        }
    }
}