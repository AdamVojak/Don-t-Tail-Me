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

        if (inventar == null)
        {
            inventar = Object.FindFirstObjectByType<MirandaInventory>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Provjeravamo tko je dotaknuo predmet
        bool dodirTijelom = other.CompareTag("Miranda");
        bool dodirShakom = other.CompareTag("Shaka");

        // Ako nije ni Miranda ni njezina Šaka, ignoriraj sudar
        if (!dodirTijelom && !dodirShakom) return;

        // 2. LOGIKA ZA POSEBNE PREDMETE (ID 2 i ID 5)
        bool zahtijevaShaku = (itemTip == 2 /*|| itemTip == 5*/);

        // Ako predmet traži šaku, a dotaknut je samo tijelom -> nemoj ga pokupiti!
        if (zahtijevaShaku && !dodirShakom)
        {
            Debug.Log($"Predmet ID: {itemTip} je pretežak/poseban i može se pokupiti SAMO šakom!");
            return;
        }

        // 3. POKUPI PREDMET I SPREMI U INVENTAR
        if (inventar == null) inventar = Object.FindFirstObjectByType<MirandaInventory>();

        if (inventar != null)
        {
            inventar.CollectItem(itemTip);

            string nacinPokupio = dodirShakom ? "ŠAKOM" : "TIJELOM";
            Debug.Log($"Miranda je uspješno pokupila item ID: {itemTip} ({nacinPokupio})!");

            Destroy(gameObject); // Obriši s poda
        }
    }
}