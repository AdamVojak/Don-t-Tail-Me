using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Postavke predmeta")]
    [Tooltip("Ruka = 3, Gun = 1, Minigun = 2, Mobitel = 10")]
    public int itemID;

    public void Collect(GiovanniInventory inventory)
    {
        if (inventory != null)
        {
            inventory.CollectItem(itemID);

            // Ovdje se može pokrenuti lokalni 3D zvuk skupljanja, npr:
            // AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}