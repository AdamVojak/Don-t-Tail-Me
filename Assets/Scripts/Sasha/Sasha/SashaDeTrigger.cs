using UnityEngine;

public class SashaDeTrigger : MonoBehaviour
{
    public GameObject objekt;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            objekt.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            objekt.SetActive(true);
        }
    }
}
