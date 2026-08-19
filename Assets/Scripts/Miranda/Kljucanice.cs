using UnityEngine;

public class Kljucanice : MonoBehaviour
{
    public enum PotrebanTipKljuca
    {
        Zuti = 0,
        Ljubicasti = 4
    }

    [SerializeField] private PotrebanTipKljuca potrebniKljuc;

    [SerializeField] private SpriteRenderer spriteRenderer; // Referenca na SpriteRenderer komponentu
    [SerializeField] private Sprite otkljucanaZuta; // Sprite za otključanu žutu ključanicu
    [SerializeField] private Sprite otkljucanaLjub; // Sprite za otključanu ljubičastu ključanicu

    [SerializeField] private Vrata vrata;

    public bool otkljucana = false;

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer komponenta nije pronađena na ključanici " + gameObject.name);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!otkljucana && other.CompareTag("Miranda"))
        {
            MirandaInventory mirandaInventory = other.GetComponentInParent<MirandaInventory>();
            if (mirandaInventory != null)
            {
                if (mirandaInventory.HasKey((int)potrebniKljuc))
                {
                    UnlockDoor(mirandaInventory);
                }
            }
        }
    }

    private void UnlockDoor(MirandaInventory mirandaInventory)
    {
        otkljucana = true;
        mirandaInventory.RemoveKey((int)potrebniKljuc);
        Debug.Log(gameObject.name + " je otključana!");

        if (potrebniKljuc == PotrebanTipKljuca.Zuti && otkljucanaZuta != null)
        {
            spriteRenderer.sprite = otkljucanaZuta;
        }
        else if (potrebniKljuc == PotrebanTipKljuca.Ljubicasti && otkljucanaLjub != null)
        {
            spriteRenderer.sprite = otkljucanaLjub;
        }

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
