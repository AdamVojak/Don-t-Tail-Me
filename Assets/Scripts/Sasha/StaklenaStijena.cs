using UnityEngine;

public class StaklenaStijena : MonoBehaviour
{
    [Header("Postavke")]
    [SerializeField] private GameObject glassBreakPrefab;
    private SashaController sashaRef;

    private void Start()
    {
        if (sashaRef == null)
        {
            sashaRef = FindFirstObjectByType<SashaController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            RazbijStaklo();
        }

        if (other.CompareTag("Pajser"))
        {
            sashaRef.TakeDamage(1, 1, true);
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