using UnityEngine;

public class MirandaPickup : MonoBehaviour
{
    [Header("Postavke Predmeta")]
    [Tooltip("ID predmeta: npr. 2 i 5 se mogu pokupiti isključivo šakom")]
    public int itemTip;

    private MirandaInventory inventar;

    private void Start()
    {
        inventar = Object.FindFirstObjectByType<MirandaInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Provjeravamo tko je dotaknuo predmet
        bool dodirTijelom = other.CompareTag("Miranda");
        bool dodirShakom = other.CompareTag("Shaka");

        if (!dodirTijelom && !dodirShakom) return;

        // 2. LOGIKA ZA POSEBNE PREDMETE (ID 2 i ID 5)
        bool zahtijevaShaku = (itemTip == 2 /*|| itemTip == 5*/);

        if (zahtijevaShaku && !dodirShakom)
        {
            Debug.Log($"Predmet ID: {itemTip} se može pokupiti SAMO šakom!");
            return;
        }

        // 3. POKUPI PREDMET I SPREMI U INVENTAR
        if (inventar == null) inventar = Object.FindFirstObjectByType<MirandaInventory>();

        if (inventar != null)
        {
            inventar.CollectItem(itemTip);

            string nacinPokupio = dodirShakom ? "ŠAKOM" : "TIJELOM";
            Debug.Log($"Miranda je uspješno pokupila item ID: {itemTip} ({nacinPokupio})!");

            // =========================================================================
            // NOVO: POTPUNO ČIŠĆENJE HINTA PRIJE UNIŠTAVANJA PREDMETA!
            // =========================================================================
            OcistiHintIGasi(other);

            // 4. Tek sada brišemo predmet s poda!
            Destroy(gameObject);
        }
    }

    private void OcistiHintIGasi(Collider other)
    {
        // A) Gasimo ClickHint skriptu na ovom predmetu ako postoji
        ClickHintMiranda hintSkripta = GetComponent<ClickHintMiranda>();
        if (hintSkripta != null)
        {
            hintSkripta.enabled = false;
        }

        // B) Gasimo Collider predmeta da spriječimo bilo kakva daljnja okidanja
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // C) Pronalazimo Mirandu (preko tijela ili preko šake) i prisilno gasimo hint
        MirandaController mc = other.GetComponentInParent<MirandaController>();
        if (mc == null) mc = Object.FindFirstObjectByType<MirandaController>();

        if (mc != null)
        {
            mc.PrisilnoUgasiSveHintove();
        }
    }
}