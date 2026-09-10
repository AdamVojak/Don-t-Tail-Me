using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Reference")]
    public Transform knob;
    public Transform tockaIskljucen;
    public Transform tockaUkljucen;

    [Header("Povezana Vrata (Logika Bijega)")]
    public UlaznaVrata ulaznaVrata;

    [Header("Postavke 3D Dubine")]
    public float xOffsetDrzanja = 5.5f;

    [Header("Postavke Hvatanja i Otpora")]
    public float radiusHvatanja = 1.0f;
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
    public MirandaClimaxManager climaxManager;

    [Header("Audio")]
    [SerializeField] private LeverAudio leverAudio;

    [Header("Blokada (Samo za S/M i G/M modove)")]
    public Sprite hintKljucGore;   // Strelica GORE + Ključ (kad igra sa Sashom)
    public Sprite hintKljucDolje;  // Strelica DOLJE + Ključ (kad igra sa Giovannijem)
    public float dometHinta = 3.5f; // Na kojoj udaljenosti se prikazuje hint

    private MirandaController mirandaController;
    private MirandaInventory mirandaInventar;
    private bool bioZakljucan = false;

    [Header("Stari Hint i Trigger (Za gašenje u igri sa 3 lika)")]
    [Tooltip("Uvuci Trigger collider s levera koji pali hint")]
    public Collider triggerColliderHinta;
    [Tooltip("Uvuci komponentu ClickHintMiranda s levera")]
    public MonoBehaviour clickHintMirandaSkripta;

    void Start()
    {
        if (clickHintMirandaSkripta != null)
        {
            clickHintMirandaSkripta.enabled = false;
        }

        if (leverAudio == null) leverAudio = GetComponent<LeverAudio>();
        if (radiusHvatanja <= 0.1f) radiusHvatanja = 1.0f;
        if (radiusPraga <= 0.1f) radiusPraga = 0.4f;

        // Automatski pronalazimo trigger i ClickHint skriptu ako nisu uvučeni u Inspectoru
        if (triggerColliderHinta == null)
        {
            foreach (Collider c in GetComponents<Collider>())
            {
                if (c.isTrigger) { triggerColliderHinta = c; break; }
            }
        }
        if (clickHintMirandaSkripta == null)
        {
            clickHintMirandaSkripta = GetComponent("ClickHintMiranda") as MonoBehaviour;
        }

        // =========================================================================
        // NOVO: AKO IGRAJU SVA 3 LIKA -> PRISILNO GASIMO TRIGGER I HINT ODMAH NA STARTU!
        // =========================================================================
        if (IsThreePlayerGame())
        {
            if (triggerColliderHinta != null) triggerColliderHinta.enabled = false;
            if (clickHintMirandaSkripta != null) clickHintMirandaSkripta.enabled = false;
            Debug.Log("[Lever] Sva 3 lika u igri -> Trigger i ClickHint trajno ugašeni na startu.");
        }

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
        if (ruka == null) ruka = Object.FindFirstObjectByType<MirandaRuka>();
        if (mirandaController == null) mirandaController = Object.FindFirstObjectByType<MirandaController>();
        if (mirandaInventar == null && mirandaController != null) mirandaInventar = mirandaController.GetComponent<MirandaInventory>();
    }

    void Update()
    {
        if (ruka == null) PronadjiRuku();
        if (knob == null || tockaIskljucen == null || tockaUkljucen == null || ruka == null) return;

        // =========================================================================
        // 1. PROVJERA BLOKADE (100% NEPROBOJNO ZAKLJUČAVANJE)
        // =========================================================================
        bool trebaBitiZakljucan = ProvjeriTrebaLiZakljucati(out bool isWithSasha);

        if (trebaBitiZakljucan)
        {
            bioZakljucan = true;
            isDragging = false; // Prisilno prekidamo bilo kakvo držanje!

            // KNOB JE ZAVAREN ZA GORNJU TOČKU (Ne mrda ni mikron):
            knob.position = new Vector3(fiksniX, tockaIskljucen.position.y, tockaIskljucen.position.z);

            // Provjera prikazivanja hinta ovisno o blizini Mirande
            if (mirandaController != null)
            {
                float distancaDoMirande = Vector3.Distance(transform.position, mirandaController.transform.position);
                if (distancaDoMirande <= dometHinta)
                {
                    Sprite hintZaPrikaz = isWithSasha ? hintKljucGore : hintKljucDolje;
                    mirandaController.PrikaziHint(this, hintZaPrikaz);
                }
                else
                {
                    mirandaController.SakrijHint(this);
                }
            }

            return; // PREKIDAMO CIJELI UPDATE! Ništa ne može pomaknuti knob dok je zaključan!
        }
        else
        {
            // Čim se riješi ključa, gasimo hint jednom zauvijek
            if (bioZakljucan)
            {
                bioZakljucan = false;
                if (mirandaController != null)
                {
                    mirandaController.SakrijHint(this);
                    mirandaController.PrisilnoUgasiSveHintove();
                }
                Debug.Log("<color=green>LEVER: Poluga je trajno odblokirana!</color>");
            }
        }

        // =========================================================================
        // 2. NORMALNA LOGIKA POVLAČENJA POLUGE (Kada nije zaključana)
        // =========================================================================
        Transform tockaPrstiju = ruka.GetTockaHvatanja();
        float udaljenostDoKnoba = UdaljenostYZ(tockaPrstiju.position, knob.position);

        if (!isDragging && udaljenostDoKnoba <= radiusHvatanja && ruka.isStisnuta)
        {
            isDragging = true;
        }

        if (isDragging && !ruka.isStisnuta)
        {
            ZavrsiPovlacenje();
        }

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
            float privremeniX = fiksniX + xOffsetDrzanja;
            knob.position = new Vector3(privremeniX, novaPozicija.y, novaPozicija.z);
        }
        else
        {
            Vector3 ciljnaTocka = isUkljucen ? tockaUkljucen.position : tockaIskljucen.position;
            Vector3 ciljSaOriginalnimX = new Vector3(fiksniX, ciljnaTocka.y, ciljnaTocka.z);
            knob.position = Vector3.Lerp(knob.position, ciljSaOriginalnimX, Time.deltaTime * brzinaOpruge);
        }
    }

    // =========================================================================
    // PRAVILO BLOKADE: ZAKLJUČANO SAMO U S/M ILI G/M KAD IMA KLJUČ (ID 6)
    // =========================================================================
    bool ProvjeriTrebaLiZakljucati(out bool isWithSasha)
    {
        isWithSasha = false;

        // 1. PRAVILO ZA SVA 3 LIKA: NIKADA NEMA BLOKADE!
        if (GameModeConfigurator.Instance != null && GameModeConfigurator.Instance.activeMode == GameModeConfigurator.GameMode.AllThree)
        {
            return false;
        }

        // Ako fali Configurator, provjeri GameManager za sva 3 lika:
        if (GameManager.Instance != null && GameManager.Instance.sashaOdabran && GameManager.Instance.mirandaOdabrana && GameManager.Instance.giovanniOdabran)
        {
            return false;
        }

        // 2. PROVJERA 2-PLAYER MODOVA
        bool isSashaMiranda = false;
        bool isMirandaGiovanni = false;

        if (GameModeConfigurator.Instance != null)
        {
            isSashaMiranda = (GameModeConfigurator.Instance.activeMode == GameModeConfigurator.GameMode.SashaAndMiranda);
            isMirandaGiovanni = (GameModeConfigurator.Instance.activeMode == GameModeConfigurator.GameMode.MirandaAndGiovanni);
        }
        else if (GameManager.Instance != null)
        {
            isSashaMiranda = (GameManager.Instance.sashaOdabran && GameManager.Instance.mirandaOdabrana && !GameManager.Instance.giovanniOdabran);
            isMirandaGiovanni = (!GameManager.Instance.sashaOdabran && GameManager.Instance.mirandaOdabrana && GameManager.Instance.giovanniOdabran);
        }

        // Ako nismo u ta dva moda, nema blokade
        if (!isSashaMiranda && !isMirandaGiovanni) return false;

        isWithSasha = isSashaMiranda;

        // 3. PROVJERA IMA LI OBICAN KLJUČ (ID 6)
        if (mirandaInventar == null && mirandaController != null)
            mirandaInventar = mirandaController.GetComponent<MirandaInventory>();

        if (mirandaInventar != null && mirandaInventar.HasItem(6))
        {
            return true; // ZAKLJUČAJ!
        }

        return false; // Nema ključ, slobodno!
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

                if (leverAudio != null) leverAudio.PlaySwitch(true);

                if (climaxManager != null)
                {
                    climaxManager.PokreniKlimaksPrekoPoluge();
                }

                onUkljuci.Invoke();
                Debug.Log("Lever: UKLJUČEN! Pokreće se klimaks.");
            }
        }
        else if (udaljenostDoIskljuceno <= radiusPraga)
        {
            if (isUkljucen)
            {
                isUkljucen = false;

                if (leverAudio != null) leverAudio.PlaySwitch(false);

                if (ulaznaVrata != null)
                {
                    ulaznaVrata.otvorenaPrekoPoluge = false;
                    ulaznaVrata.ZatvoriVrata();
                }

                onIskljuci.Invoke();
            }
        }
    }

    float UdaljenostYZ(Vector3 tockaA, Vector3 tockaB)
    {
        return Vector2.Distance(new Vector2(tockaA.z, tockaA.y), new Vector2(tockaB.z, tockaB.y));
    }

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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(knob.position, radiusHvatanja);
        }
    }

    bool IsThreePlayerGame()
    {
        if (GameModeConfigurator.Instance != null)
            return GameModeConfigurator.Instance.activeMode == GameModeConfigurator.GameMode.AllThree;

        if (GameManager.Instance != null)
            return GameManager.Instance.sashaOdabran && GameManager.Instance.mirandaOdabrana && GameManager.Instance.giovanniOdabran;

        return false;
    }
}