using Unity.VisualScripting;
using UnityEngine;

public class Bomba : MonoBehaviour
{
    [SerializeField] private int warning = 3;
    private GiovanniController giovanniControllerRef;
    
    void Start()
    {
        if (giovanniControllerRef == null)
        {
            giovanniControllerRef = FindFirstObjectByType<GiovanniController>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Giovanni"))
        {
            if (warning > 0)
            {
                warning--;
                Debug.Log("Warning! " + warning + " more hits before explosion.");
            }
            else
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        Debug.Log("Boom! The bomb has exploded.");
        
        giovanniControllerRef.currentState = GiovanniController.GiovanniState.Dead;

        Destroy(gameObject);
    }
}
