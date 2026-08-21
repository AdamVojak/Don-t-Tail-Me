using UnityEngine;

public class MirandaPickup : MonoBehaviour
{
    public int itemTip;
    private MirandaInventory inventar;

    private void Start()
    {
        inventar = FindFirstObjectByType<MirandaInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            MirandaInventory inv = other.GetComponent<MirandaInventory>();
            if (inv != null)
            {
                // Šaljemo bilo koji itemTip (0, 1, 2, 3, 4, 5) direktno u MirandaInventory
                inv.CollectItem(itemTip);

                Debug.Log($"Miranda je uspješno pokupila item ID: {itemTip}");
                Destroy(gameObject); // Obriši s poda
            }
        }
    }
}