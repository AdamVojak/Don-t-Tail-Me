using UnityEngine;

public class Bomba : MonoBehaviour
{
    [Header("Lokalni Cooldown")]
    [Tooltip("Koliko ova specifična bomba mora čekati prije nego opet registrira dodir")]
    [SerializeField] private float lokalniCooldown = 1.0f;
    private float zadnjiDodir = -1f;

    [Header("Audio")]
    [SerializeField] private BombaAudio bombaAudio; // Referenca na zvuk mine

    private MinskoPolje minskoPoljeRef;

    void Awake()
    {
        if (bombaAudio == null) bombaAudio = GetComponent<BombaAudio>();
    }

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
            if (Time.time < zadnjiDodir + lokalniCooldown) return;

            zadnjiDodir = Time.time;

            // 1. PUSTI 3D ZVUK ZVECKANJA LANCA OVE BOMBE:
            if (bombaAudio != null) bombaAudio.PlayChainHit();

            // 2. Pošalji signal u Minsko Polje
            minskoPoljeRef.RegistrirajDodir(transform.position, gameObject);
        }
    }
}