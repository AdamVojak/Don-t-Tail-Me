using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    [SerializeField] private Worms_Spawner spawner;

    private void Start()
    {
        if (spawner == null)
        {
            spawner = GetComponentInParent<Worms_Spawner>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            if (spawner != null)
            {
                spawner.ActivateSpawner();
            }
        }
    }
}
