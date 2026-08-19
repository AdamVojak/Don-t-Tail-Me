using System.Collections;
using UnityEngine;

public class KeyItemPickup : MonoBehaviour
{

    public bool isPickedUp = false;
    private SashaController sasha;

    private SFX sfx;
    private AudioSource pickup;


    void Start()
    {

        if (sfx == null)
        {
            sfx = FindFirstObjectByType<SFX>();
        }
        else
        {
            Debug.LogWarning(gameObject.name + " ne može pronaći SashaController u sceni!");
        }

        if (sasha == null)
        {
            sasha = FindFirstObjectByType<SashaController>();
        }
        else
        {
            Debug.LogWarning(gameObject.name + " ne može pronaći SashaController u sceni!");
        }

        pickup = sfx.ZvukPraznogKlika;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPickedUp && other.CompareTag("Sasha"))
        {
            SashaInventory inventar = other.GetComponent<SashaInventory>();

            if (inventar != null)
            {
                pickup.Play();
                inventar.CollectItem(0);
                isPickedUp = true;

                Debug.Log("Sasha je pokupio žuti kljuc!");
                Destroy(gameObject);
            }
        }
    }
}