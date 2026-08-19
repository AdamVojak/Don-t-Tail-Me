using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [Header("Postavke")]
    [SerializeField] private GameObject HeadlessPrefab;
    [SerializeField] private GameObject Enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        if (HeadlessPrefab != null)
        {
            Instantiate(HeadlessPrefab, transform.position, transform.rotation);
        }

        Destroy(Enemy);
    }
}
