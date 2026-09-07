using UnityEngine;

public class KljucanicaObican : MonoBehaviour
{
    [Header("Postavke")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite otkljucanObican;
    [SerializeField] private HorizontalnaVrataMiranda vrata; // Poveznica na horizontalna vrata

    private int obicanKeyID = 6; // ID za običan ključ
    private bool otkljucana = false;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (otkljucana) return;

        if (other.CompareTag("Miranda") || other.CompareTag("Shaka"))
        {
            MirandaInventory inv = other.GetComponentInParent<MirandaInventory>();
            if (inv == null) inv = Object.FindFirstObjectByType<MirandaInventory>();

            if (inv != null && inv.HasItem(obicanKeyID))
            {
                UnlockDoor();
            }
        }
    }

    private void UnlockDoor()
    {
        otkljucana = true;

        // OVDJE NE POZIVAMO RemoveItem - ključ ostaje u inventaru!
        Debug.Log("Ključanica otključana običnim ključem (ključ ostaje u inventaru).");

        if (spriteRenderer != null && otkljucanObican != null)
        {
            spriteRenderer.sprite = otkljucanObican;
        }

        // Obavijesti horizontalna vrata da su otključana
        if (vrata != null)
        {
            vrata.ProvjeriKljucanicu();
        }
    }
}