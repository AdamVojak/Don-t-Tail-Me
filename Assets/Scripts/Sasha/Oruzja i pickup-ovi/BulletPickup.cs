using Unity.VisualScripting;
using UnityEngine;

public class BulletPickup : MonoBehaviour
{
    private int bulletAmount;
    public string gunTag = "Minigun";
    private Minigun playerMinigun;
    public string playerTag = "Sasha";
    private SashaInventory SashaInventoryRef;
    private bool imaGa;

    private bool punAmmo;

    private void Start()
    {
        if (SashaInventoryRef == null)
        {
            SashaInventoryRef = FindFirstObjectByType<SashaInventory>();
        }

        if (playerMinigun == null)
        {
            playerMinigun = FindFirstObjectByType<Minigun>();
        }

        bulletAmount = Random.Range(15, 30);
    }

    private void Update()
    {
        if (SashaInventoryRef != null)
        {
            imaGa = SashaInventoryRef.imaMinigun;
        }
        
        if (playerMinigun != null)
        {
            punAmmo = playerMinigun.punAmmo;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && SashaInventoryRef != null && imaGa && !punAmmo)
        {
            Minigun playerMinigun = other.GetComponentInChildren<Minigun>(true);

            if (playerMinigun != null)
            {
                playerMinigun.AddAmmo(bulletAmount);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Igrač je pokupio metke, ali skripta 'Minigun' nije pronađena na njemu ili njegovoj djeci!");
            }
        }
    }
}