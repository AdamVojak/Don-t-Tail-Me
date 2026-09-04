using UnityEngine;

public class LaserBarijera : MonoBehaviour
{
    [Header("Fizička Prepreka i Animacija")]
    public GameObject solidColliderDijete;
    public Animator laserAnimator;
    public string imeAnimacijeUgasen = "Laser_Ugasen";

    [Header("Hint Objekti (Djeca)")]
    [Tooltip("Dijete objekt koji prikazuje zabranu pištolja")]
    public GameObject hintZabranaObjekt;
    [Tooltip("Dijete objekt koji prikazuje zelenu kvačicu")]
    public GameObject hintKvacicaObjekt;

    [Header("Postavke Strujnog Udara")]
    public Sprite sashaSokiraniSprite;
    public float vrijemeStrujnogUdara = 1.0f;
    public float snagaOdbacivanjaY = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip strujaZvuk;
    [Range(0f, 1f)] public float strujaVolume = 0.8f;

    public AudioClip potvrdniZvuk2D;
    [Range(0f, 1f)] public float potvrdaVolume = 1.0f;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (solidColliderDijete != null) solidColliderDijete.SetActive(true);
        if (laserAnimator != null) laserAnimator.SetBool("nemaGun", false);

        if (hintZabranaObjekt != null) hintZabranaObjekt.SetActive(true);
        if (hintKvacicaObjekt != null) hintKvacicaObjekt.SetActive(false);
    }

    // VRATILI SMO ČISTI OnTriggerEnter KOJI JE SAVRŠENO RADIO ŠOK!
    private void OnTriggerEnter(Collider other)
    {
        SashaController sasha = other.GetComponent<SashaController>();
        SashaInventory inventar = other.GetComponent<SashaInventory>();

        if (sasha != null && inventar != null)
        {
            // =========================================================
            // SLUČAJ 1: SASHA IMA GUN (Strujni udar)
            // =========================================================
            if (inventar.imaGun)
            {
                if (solidColliderDijete != null) solidColliderDijete.SetActive(true);
                if (laserAnimator != null) laserAnimator.SetBool("nemaGun", false);

                // PALIMO ZABRANU, GASIMO KVAČICU:
                if (hintZabranaObjekt != null) hintZabranaObjekt.SetActive(true);
                if (hintKvacicaObjekt != null) hintKvacicaObjekt.SetActive(false);

                if (audioSource != null && strujaZvuk != null)
                {
                    audioSource.PlayOneShot(strujaZvuk, strujaVolume);
                }

                // Šaljemo šok i odbacivanje koje je radilo vrhunski
                sasha.PrimijeniStrujniUdar(snagaOdbacivanjaY, vrijemeStrujnogUdara, sashaSokiraniSprite);
            }
            // =========================================================
            // SLUČAJ 2: SASHA NEMA GUN (Prolaz odobren)
            // =========================================================
            else
            {
                if (solidColliderDijete != null) solidColliderDijete.SetActive(false);

                if (laserAnimator != null)
                {
                    laserAnimator.SetBool("nemaGun", true);
                    if (!string.IsNullOrEmpty(imeAnimacijeUgasen))
                    {
                        laserAnimator.Play(imeAnimacijeUgasen);
                    }
                }

                if (audioSource != null && potvrdniZvuk2D != null)
                {
                    audioSource.PlayOneShot(potvrdniZvuk2D, potvrdaVolume);
                }

                // GASIMO ZABRANU, PALIMO KVAČICU:
                if (hintZabranaObjekt != null) hintZabranaObjekt.SetActive(false);
                if (hintKvacicaObjekt != null) hintKvacicaObjekt.SetActive(true);

                Debug.Log("<color=green>LASER: Sasha nema Gun! Pristup odobren.</color>");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }
}