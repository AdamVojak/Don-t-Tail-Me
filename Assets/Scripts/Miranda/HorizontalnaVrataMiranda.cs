using UnityEngine;

public class HorizontalnaVrataMiranda : MonoBehaviour
{
    [SerializeField] private Kljucanice kljucanica; // Samo jedan ključ
    [SerializeField] private float brzinaOtvaranja = 2f;
    [SerializeField] private float udaljenostPomaka = 3f; // Koliko se vrata pomiču po Z osi

    public bool suOtkljucana = false;
    private bool vrataSeOtvaraju = false;

    private Vector3 pocetnaPozicija;
    private Vector3 ciljnaPozicija;

    [Header("UI")]
    public GameObject kljucUI;
    public GameObject rukaUI;

    [Header("Audio")]
    [SerializeField] private DoorAudio doorAudio;

    void Start()
    {
        if (doorAudio == null) doorAudio = GetComponent<DoorAudio>();
        pocetnaPozicija = this.transform.position;

        // Ciljna pozicija je pomaknuta po Z osi (Forward/Back)
        ciljnaPozicija = pocetnaPozicija + (Vector3.forward * udaljenostPomaka);
    }

    void Update()
    {
        if (vrataSeOtvaraju)
        {
            // Pomičemo se po Z osi (Vector3.forward)
            this.transform.position = Vector3.MoveTowards(this.transform.position, ciljnaPozicija, brzinaOtvaranja * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, ciljnaPozicija) < 0.001f)
            {
                vrataSeOtvaraju = false;
                if (doorAudio != null) doorAudio.StopMoving();
                Debug.Log("Horizontalna vrata su otvorena.");
            }
        }
    }

    public void ProvjeriKljucanicu()
    {
        if (kljucanica != null && kljucanica.otkljucana)
        {
            suOtkljucana = true;
            if (kljucUI != null) Destroy(kljucUI.gameObject);
            Debug.Log("Treća ključanica otključana. Vrata spremna.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (suOtkljucana && !vrataSeOtvaraju && other.CompareTag("Miranda"))
        {
            vrataSeOtvaraju = true;
            if (doorAudio != null) doorAudio.StartMoving();
            if (rukaUI != null) rukaUI.SetActive(true);
            Debug.Log("Pokreće se horizontalno otvaranje.");
        }
    }
}