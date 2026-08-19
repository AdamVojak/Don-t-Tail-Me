using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private int healthAmount;
    public string playerTag = "Sasha";

    private void Start()
    {
        healthAmount = Random.Range(1, 3);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Sasha je dotaknula health pickup!");

            SashaController sasha = other.GetComponent<SashaController>();

            if (sasha == null) sasha = other.GetComponentInParent<SashaController>();

            if (sasha != null)
            {
                Debug.Log("SashaController pronađen. Trenutni život: " + sasha.zivot);

                if (sasha.zivot < 3)
                {
                    sasha.Heal(healthAmount);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Sasha je puna života (3), ne uzima paket.");
                }
            }
            else
            {
                Debug.Log("SashaController nije pronađen na objektu!");
            }
        }
    }
}