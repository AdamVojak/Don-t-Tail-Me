using UnityEngine;

public class Akvarij : MonoBehaviour
{
    public GameObject prefab;
    public GameObject prePrefab;
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
        // Stvaramo krhotine
        Instantiate(prefab, transform.position, transform.rotation);

        // Uništavamo akvarij. Ovo će automatski okinuti OnDisable() u ClickHintu
        // i ClickHint će sam ugasiti Sashin hint!
        if (prePrefab != null) Destroy(prePrefab.gameObject);
        else Destroy(gameObject);
    }
}