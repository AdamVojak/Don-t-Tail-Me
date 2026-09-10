using UnityEngine;

public class KljucanicaObicanSasha : MonoBehaviour
{
    [Header("Postavke Ključa")]
    [SerializeField] private int obicanKeyID = 6; // Običan ključ (ID 6)

    [Header("Meta za uništenje")]
    [Tooltip("Uvuci LASER iz HIERARCHYJA (ne iz Project mape!)")]
    public GameObject laserZaUnistiti;

    [Header("Sličice Hinta iznad glave")]
    [Tooltip("Sličica prekriženog ključa (prikazuje se kad NEMA ključ)")]
    public Sprite hintPrekrizeniKljuc;
    [Tooltip("Sličica tipke F (prikazuje se kad IMA ključ)")]
    public Sprite hintTipkaF;

    [Header("Vizualne Reference Brave")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite otkljucanObican;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip otkljucajZvuk;
    public AudioClip errorZvuk;

    private bool playerInside = false;
    private bool otkljucana = false;

    private SashaController sasha;
    private SashaInventory inventar;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            playerInside = true;
            sasha = other.GetComponentInParent<SashaController>();
            inventar = other.GetComponentInParent<SashaInventory>();

            // Čim uđe, odmah ažuriramo hint
            AzurirajHintIznadGlave();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha") && !otkljucana)
        {
            // Stalno osvježavamo hint (u slučaju da u međuvremenu dobije ključ)
            AzurirajHintIznadGlave();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            playerInside = false;

            if (sasha != null)
            {
                sasha.SakrijHint(this);
            }
        }
    }

    void Update()
    {
        if (playerInside && !otkljucana && Input.GetKeyDown(KeyCode.F))
        {
            if (inventar != null && inventar.HasItem(obicanKeyID))
            {
                OtkljucajIUnistiLaser();
            }
            else
            {
                if (audioSource != null && errorZvuk != null)
                {
                    audioSource.PlayOneShot(errorZvuk);
                }
                Debug.Log("<color=red>Sasha nema Običan ključ (ID 6)!</color>");
            }
        }
    }

    // =========================================================================
    // DINAMIČKI HINT: Prekriženi ključ VS Tipka F
    // =========================================================================
    private void AzurirajHintIznadGlave()
    {
        if (sasha == null || inventar == null || otkljucana) return;

        // A) Ako IMA ključ -> Pokaži mu tipku 'F' da zna da može otključati!
        if (inventar.HasItem(obicanKeyID))
        {
            if (hintTipkaF != null) sasha.PrikaziHint(this, hintTipkaF);
        }
        // B) Ako NEMA ključ -> Pokaži mu prekriženi ključić!
        else
        {
            if (hintPrekrizeniKljuc != null) sasha.PrikaziHint(this, hintPrekrizeniKljuc);
        }
    }

    private void OtkljucajIUnistiLaser()
    {
        otkljucana = true;

        if (audioSource != null && otkljucajZvuk != null)
        {
            audioSource.PlayOneShot(otkljucajZvuk);
        }

        if (spriteRenderer != null && otkljucanObican != null)
        {
            spriteRenderer.sprite = otkljucanObican;
        }

        // 1. ČISTIMO HINT
        if (sasha != null)
        {
            sasha.SakrijHint(this);
            sasha.PrisilnoUgasiSveHintove();
        }

        // 2. Gasimo collider brave
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. Oduzimamo ključ
        if (inventar != null)
        {
            inventar.RemoveItem(obicanKeyID);
        }

        // =========================================================================
        // 4. DVOSTRUKO SIGURNO BRISANJE LASERA!
        // =========================================================================
        if (laserZaUnistiti != null)
        {
            laserZaUnistiti.SetActive(false); // <--- ODMAH GA GASIMO (nema kašnjenja!)
            Destroy(laserZaUnistiti);          // <--- BRIŠEMO GA IZ MEMORIJE
            Debug.Log("<color=green>KLJUČANICA: Laser uspješno ugašen i uništen!</color>");
        }
        else
        {
            Debug.LogError("<color=red>GREŠKA: Laser Za Unistiti polje je PRAZNO na ključanici! Povuci laser iz Hierarchyja!</color>");
        }

        this.enabled = false;
    }
}