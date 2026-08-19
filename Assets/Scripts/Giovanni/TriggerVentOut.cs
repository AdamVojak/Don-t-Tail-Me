using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    [SerializeField] private Ventilacija_Out_Giovanni mainScript;

    void Start()
    {
        if (mainScript == null)
        {
            mainScript = GetComponentInParent<Ventilacija_Out_Giovanni>();
            if (mainScript == null)
            {
                mainScript = FindFirstObjectByType<Ventilacija_Out_Giovanni>();
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (mainScript != null && other.CompareTag("Giovanni"))
        {
            mainScript.PlayerStayedInTrigger(other);
        }
    }
}