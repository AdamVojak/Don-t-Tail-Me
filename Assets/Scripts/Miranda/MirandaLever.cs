using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Reference")]
    public Transform knob;           // Objekt Knob koji klizi
    public Transform tockaIskljucen; // Gornja točka (OFF)
    public Transform tockaUkljucen;  // Donja točka (ON)

    [Header("Postavke 3D Dubine")]
    [Tooltip("Koliko se knob pomakne po X osi dok ga držiš")]
    public float xOffsetDrzanja = 5.5f;

    [Header("Postavke Hvatanja i Otpora")]
    [Tooltip("Domet hvatanja (postavi na 0.8 do 1.2 za ugodno igranje)")]
    public float radiusHvatanja = 1.0f;
    [Tooltip("Radijus oko krajnjih točaka za prebacivanje stanja")]
    public float radiusPraga = 0.4f;
    public float brzinaOpruge = 18f;

    [Header("Stanje")]
    public bool isUkljucen = false;

    [Header("Događaji (Events)")]
    public UnityEvent onUkljuci;
    public UnityEvent onIskljuci;

    private bool isDragging = false;
    private float fiksniX;
    [SerializeField] private MirandaRuka ruka;

    void Start()
    {
        // Zaštita od Unity Inspector reseta na 0
        if (radiusHvatanja <= 0.1f) radiusHvatanja = 1.0f;
        if (radiusPraga <= 0.1f) radiusPraga = 0.4f;

        PronadjiRuku();

        if (knob != null && tockaIskljucen != null && tockaUkljucen != null)
        {
            fiksniX = knob.position.x;
            Vector3 pocetna = isUkljucen ? tockaUkljucen.position : tockaIskljucen.position;
            knob.position = new Vector3(fiksniX, pocetna.y, pocetna.z);
        }
    }

    void PronadjiRuku()
    {
        if (ruka == null)
        {
            ruka = Object.FindFirstObjectByType<MirandaRuka>();
            if (ruka == null) Debug.LogError("Lever: Ne mogu pronaći skriptu MirandaRuka u sceni!");
        }
    }

    void Update()
    {
        if (ruka == null) PronadjiRuku();
        if (knob == null || tockaIskljucen == null || tockaUkljucen == null || ruka == null) return;

        // OVDJE KORISTIMO VRH PINCETE/PRSTIJU UMJESTO CENTRA ŠAKE!
        Transform tockaPrstiju = ruka.GetTockaHvatanja();
        float udaljenostDoKnoba = UdaljenostYZ(tockaPrstiju.position, knob.position);

        // 1. POČETAK HVATANJA (Gleda se točno vrh pincete)
        if (!isDragging && udaljenostDoKnoba <= radiusHvatanja && ruka.isStisnuta)
        {
            isDragging = true;
            Debug.Log("Lever: Pinceta je zgrabila Knob!");
        }

        // 2. PUŠTANJE KLIKA
        if (isDragging && !ruka.isStisnuta)
        {
            ZavrsiPovlacenje();
        }

        // 3. POVLAČENJE (Knob prati točno vrh pincete duž vodilice)
        if (isDragging)
        {
            Vector2 a = new Vector2(tockaIskljucen.position.z, tockaIskljucen.position.y);
            Vector2 b = new Vector2(tockaUkljucen.position.z, tockaUkljucen.position.y);
            Vector2 prstiPos = new Vector2(tockaPrstiju.position.z, tockaPrstiju.position.y);

            Vector2 linija = b - a;
            Vector2 smjerRuke = prstiPos - a;

            float magnituda = linija.sqrMagnitude;
            float t = (magnituda > 0.0001f) ? Vector2.Dot(smjerRuke, linija) / magnituda : 0f;
            t = Mathf.Clamp01(t);

            Vector3 novaPozicija = Vector3.Lerp(tockaIskljucen.position, tockaUkljucen.position, t);

            // Pop-out po X osi dok držiš
            float privremeniX = fiksniX + xOffsetDrzanja;
            knob.position = new Vector3(privremeniX, novaPozicija.y, novaPozicija.z);
        }
        else
        {
            // 4. EFEKT OPRUGE
            Vector3 ciljnaTocka = isUkljucen ? tockaUkljucen.position : tockaIskljucen.position;
            Vector3 ciljSaOriginalnimX = new Vector3(fiksniX, ciljnaTocka.y, ciljnaTocka.z);

            knob.position = Vector3.Lerp(knob.position, ciljSaOriginalnimX, Time.deltaTime * brzinaOpruge);
        }
    }

    void ZavrsiPovlacenje()
    {
        isDragging = false;

        float udaljenostDoUkljuceno = UdaljenostYZ(knob.position, tockaUkljucen.position);
        float udaljenostDoIskljuceno = UdaljenostYZ(knob.position, tockaIskljucen.position);

        if (udaljenostDoUkljuceno <= radiusPraga)
        {
            if (!isUkljucen)
            {
                isUkljucen = true;
                onUkljuci.Invoke();
                Debug.Log("Lever: Uspješno UKLJUČEN (ON)!");
            }
        }
        else if (udaljenostDoIskljuceno <= radiusPraga)
        {
            if (isUkljucen)
            {
                isUkljucen = false;
                onIskljuci.Invoke();
                Debug.Log("Lever: Uspješno UGAŠEN (OFF)!");
            }
        }
        else
        {
            Debug.Log("Lever: Opruga vraća polugu nazad.");
        }
    }

    float UdaljenostYZ(Vector3 tockaA, Vector3 tockaB)
    {
        return Vector2.Distance(new Vector2(tockaA.z, tockaA.y), new Vector2(tockaB.z, tockaB.y));
    }

    // --- GIZMOS ---
    private void OnDrawGizmosSelected()
    {
        if (tockaIskljucen != null && tockaUkljucen != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(tockaIskljucen.position, tockaUkljucen.position);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(tockaUkljucen.position, radiusPraga);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(tockaIskljucen.position, radiusPraga);
        }

        if (knob != null)
        {
            // Žuti krug prikazuje zonu hvatanja
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(knob.position, radiusHvatanja);
        }
    }
}