using UnityEngine;

public class Akvarij : MonoBehaviour
{
    public GameObject prefab;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Pajser") || other.CompareTag("Melee")) && !hasTriggered)
        {
            hasTriggered = true;
            Die();
        }
    }

    private void Die()
    {
        GameObject razbijeni = Instantiate(prefab, transform.position, transform.rotation);
        razbijeni.transform.SetParent(transform.parent);
        Destroy(gameObject);
    }
}