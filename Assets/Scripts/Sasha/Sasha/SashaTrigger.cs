using UnityEngine;

public class SashaTrigger : MonoBehaviour
{
    public GameObject objekt;

    private void Awake()
    {
        objekt.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            objekt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            objekt.SetActive(false);
        }
    }
}
