using UnityEngine;

public class StaklenaStijena : MonoBehaviour
{
    [Header("Postavke")]
    [SerializeField] private GameObject glassBreakPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") || other.CompareTag("Pajser"))
        {
            RazbijStaklo();
        }
    }

    private void RazbijStaklo()
    {
        if (glassBreakPrefab != null)
        {
            Instantiate(glassBreakPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}