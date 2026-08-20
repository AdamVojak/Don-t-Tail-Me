using UnityEngine;

public class Bomba : MonoBehaviour
{
    [Header("Lokalni Cooldown")]
    [Tooltip("Koliko ova specifična bomba mora čekati prije nego opet registrira dodir")]
    [SerializeField] private float lokalniCooldown = 1.0f;
    private float zadnjiDodir = -1f;

    private MinskoPolje minskoPoljeRef;

    void Start()
    {
        minskoPoljeRef = GetComponentInParent<MinskoPolje>();

        if (minskoPoljeRef == null)
        {
            minskoPoljeRef = FindFirstObjectByType<MinskoPolje>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Giovanni") && minskoPoljeRef != null)
        {
            // Provjera cooldowna SAMO ZA OVU BOMBU
            if (Time.time < zadnjiDodir + lokalniCooldown) return;

            zadnjiDodir = Time.time;

            // Šalje signal u Minsko Polje (odmah, bez čekanja drugih bombi!)
            minskoPoljeRef.RegistrirajDodir(transform.position, gameObject);
        }
    }
}