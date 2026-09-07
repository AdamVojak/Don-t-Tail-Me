using UnityEngine;

public class HorizontalnaVrataMiranda : MonoBehaviour
{
    [SerializeField] private KljucanicaObican kljucanica;
    [SerializeField] private float brzinaOtvaranja = 2f;
    [SerializeField] public float udaljenostPomaka = 5f;

    public bool suOtkljucana = false;
    private bool vrataSeOtvaraju = false;

    private Vector3 pocetnaPozicija;
    private Vector3 ciljnaPozicija;

    [Header("Audio")]
    [SerializeField] private DoorAudio doorAudio;

    void Start()
    {
        pocetnaPozicija = this.transform.position;
        ciljnaPozicija = pocetnaPozicija + (Vector3.forward * udaljenostPomaka);
    }

    void Update()
    {
        if (vrataSeOtvaraju)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, ciljnaPozicija, brzinaOtvaranja * Time.deltaTime);

            if (Vector3.Distance(this.transform.position, ciljnaPozicija) < 0.001f)
            {
                vrataSeOtvaraju = false;
                if (doorAudio != null) doorAudio.StopMoving();
                Debug.Log("Horizontalna vrata su otvorena.");
            }
        }
    }

    // Poziva se čim se ključanica otključa
    public void ProvjeriKljucanicu()
    {
        if (kljucanica != null && kljucanica.Otkljucana)
        {
            suOtkljucana = true;
            vrataSeOtvaraju = true; // Automatski pokreni otvaranje
            if (doorAudio != null) doorAudio.StartMoving();
            Debug.Log("Vrata su otključana i otvaraju se!");
        }
    }
}