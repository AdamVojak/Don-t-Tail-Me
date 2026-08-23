using UnityEngine;

public class Kljucanice : MonoBehaviour
{
    public enum PotrebanTipKljuca
    {
        Zuti = 0,        // ID 0 u unificiranom inventaru
        Ljubicasti = 4   // ID 4 u unificiranom inventaru
    }

    [Header("Postavke Ključanice")]
    [SerializeField] private PotrebanTipKljuca potrebniKljuc;

    [Header("Vizuali")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite otkljucanaZuta;
    [SerializeField] private Sprite otkljucanaLjub;

    [Header("Vrata")]
    [SerializeField] private Vrata vrata;

    [HideInInspector]
    public bool otkljucana = false;

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer nije pronađen na ključanici: " + gameObject.name);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (otkljucana) return;

        // Prepoznajemo i ako Miranda priđe tijelom i ako pruži ruku ("Shaka")
        if (other.CompareTag("Miranda") || other.CompareTag("Shaka"))
        {
            MirandaInventory inv = other.GetComponentInParent<MirandaInventory>();
            if (inv == null) inv = Object.FindFirstObjectByType<MirandaInventory>();

            if (inv != null)
            {
                // Provjeravamo ima li odgovarajući ključ u unificiranom inventaru
                bool imaKljuc = false;
                if (potrebniKljuc == PotrebanTipKljuca.Zuti) imaKljuc = inv.ImaZutiKljuc;
                else if (potrebniKljuc == PotrebanTipKljuca.Ljubicasti) imaKljuc = inv.ImaLjubicastiKljuc;

                if (imaKljuc)
                {
                    UnlockDoor(inv);
                }
            }
        }
    }

    private void UnlockDoor(MirandaInventory mirandaInventory)
    {
        otkljucana = true;

        // UNIFICIRANO: Koristimo standardnu RemoveItem metodu s ID-jem (0 ili 4)
        mirandaInventory.RemoveItem((int)potrebniKljuc);
        Debug.Log($"Ključanica {gameObject.name} je uspješno OTKLJUČANA!");

        // Promjena spritea
        if (potrebniKljuc == PotrebanTipKljuca.Zuti && otkljucanaZuta != null)
        {
            spriteRenderer.sprite = otkljucanaZuta;
        }
        else if (potrebniKljuc == PotrebanTipKljuca.Ljubicasti && otkljucanaLjub != null)
        {
            spriteRenderer.sprite = otkljucanaLjub;
        }

        // Obavijesti vrata
        if (vrata != null)
        {
            vrata.ProvjeriKljucanice();
        }
        else
        {
            Debug.LogWarning("Vrata nisu dodijeljena na ključanici " + gameObject.name);
        }
    }
}