using UnityEngine;

public class Vrata : MonoBehaviour
{
    [SerializeField]
    private Kljucanice kljucanica1;
    [SerializeField]
    private Kljucanice kljucanica2;

    [SerializeField]
    private float brzinaOtvaranja = 2f;

    public bool suOtkljucana = false;
    private bool vrataSeOtvaraju = false;

    private Vector3 pocetnaPozicija;
    private Vector3 ciljnaPozicija;

    public GameObject kljuceviUI;
    public GameObject rukaUI;

    [Header("Audio")]
    [SerializeField] private DoorAudio doorAudio;

    void Start()
    {
        if (doorAudio == null) doorAudio = GetComponent<DoorAudio>();

        pocetnaPozicija = this.transform.position;

        float visinaVrata = IzracunajVisinu();

        ciljnaPozicija = pocetnaPozicija + Vector3.up * visinaVrata;
    }

    void Update()
    {
        if (vrataSeOtvaraju)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, ciljnaPozicija, brzinaOtvaranja * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, ciljnaPozicija) < 0.001f)
            {
                this.transform.position = ciljnaPozicija;
                vrataSeOtvaraju = false;

                if (doorAudio != null) doorAudio.StopMoving();

                Debug.Log("Vrata su se potpuno otvorila.");
            }
        }
    }

    public void ProvjeriKljucanice()
    {
        if (kljucanica1 != null && kljucanica2 != null)
        {
            if (kljucanica1.otkljucana && kljucanica2.otkljucana)
            {
                suOtkljucana = true;
                Destroy(kljuceviUI.gameObject);
                Debug.Log("Obje ključanice su otključane. Vrata se mogu otvoriti.");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (suOtkljucana && !vrataSeOtvaraju && other.CompareTag("Miranda"))
        {
            vrataSeOtvaraju = true;

            if (doorAudio != null) doorAudio.StartMoving();
            rukaUI.SetActive(true);
            Debug.Log("Miranda je zakoračila u trigger. Pokreće se podizanje cijelog objekta.");
        }
    }

    private float IzracunajVisinu()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                return col.bounds.size.y;
            }
        }

        Collider childCollider = GetComponentInChildren<Collider>();
        if (childCollider != null)
        {
            return childCollider.bounds.size.y;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }

        return 3f;
    }
}