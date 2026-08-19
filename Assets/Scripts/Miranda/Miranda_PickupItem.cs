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
            MirandaInventory inventar = other.GetComponent<MirandaInventory>();
            if (inventar != null)
            {
                if (itemTip == 0)
                {
                    inventar.CollectKey((int)Kljucevi.TipKljuca.Zuti);
                    Destroy(gameObject);
                }
                else if ((itemTip == 1 || itemTip == 2 || itemTip == 3))
                {
                    inventar.CollectItem(itemTip);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Miranda je našla oružje, ali njen inventar trenutno prima samo ključeve!");
                }
            }
        }
    }
}