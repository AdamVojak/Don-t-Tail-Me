using UnityEngine;

public class Kljucevi : MonoBehaviour
{
    public enum TipKljuca
    {
        Zuti = 0,
        Ljubicasti = 4
    }

    [SerializeField] public TipKljuca tip_kljuca;

    public MirandaInventory mirandaPlayerInventory;

    private void Start()
    {
        mirandaPlayerInventory = FindFirstObjectByType<MirandaInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            if (mirandaPlayerInventory != null)
            {
                mirandaPlayerInventory.CollectKey((int)tip_kljuca);
                Destroy(gameObject);
            }
            else
            {
                MirandaInventory foundInventory = other.GetComponentInParent<MirandaInventory>();
                if (foundInventory != null)
                {
                    foundInventory.CollectKey((int)tip_kljuca);
                    Destroy(gameObject);
                }
            }
        }
    }
}
