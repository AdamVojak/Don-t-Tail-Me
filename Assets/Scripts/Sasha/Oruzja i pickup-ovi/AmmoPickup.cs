using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    private int ammoAmount;

    public string gunTag = "Gun";

    private void Start()
    {
        ammoAmount = Random.Range(2, 5);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(gunTag))
        {
            Gun playerGun = other.GetComponent<Gun>();

            if (playerGun != null)
            {
                playerGun.AddAmmo(ammoAmount);
                Destroy(gameObject);
            }
        }
    }
}