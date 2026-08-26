using UnityEngine;

public class MirandaRuka : MonoBehaviour
{
    [Header("Točka Hvatanja (Pinceta / Prsti)")]
    public Transform tockaHvatanja;

    [Header("Spriteovi Šake (Otvorena / Stisnuta)")]
    public SpriteRenderer shakaSpriteRenderer; // SpriteRenderer na objektu Shaka
    public Sprite spriteOtvorenaShaka;          // Prirodna / Ispružena ruka
    public Sprite spriteStisnutaShaka;          // Stisnuta šaka (Fist / Grab)

    [HideInInspector]
    public bool isStisnuta = false; // Javna varijabla koju ćemo kasnije koristiti za levere

    [Header("Reference - Drugi dio ruke")]
    public Transform shakaObjekt;
    public Transform spojShaka;

    [Header("Reference - Granice izvlačenja")]
    public Transform pocetakShaka;
    public Transform krajShaka;

    [Header("Postavke")]
    public float glatkocaIzvlacenja = 0.15f;

    private Vector3 trenutnaBrzinaShake; // Služi Unityju za izračun inercije

    public float pragUvlacenja = 1.5f; // NOVO: Tampon zona oko lika

    public float offsetKuta = -90f;

    private Camera glavnaKamera;

    [Header("Collider Šake")]
    public Collider shakaCollider;

    private MirandaAudio mirandaAudio;

    public Transform GetTockaHvatanja()
    {
        return (tockaHvatanja != null) ? tockaHvatanja : shakaObjekt;
    }

    void Start()
    {
        glavnaKamera = Camera.main;
        mirandaAudio = GetComponentInParent<MirandaAudio>();
    }

    void Update()
    {
        if (shakaObjekt == null || spojShaka == null || pocetakShaka == null || krajShaka == null) return;

        // 1. PRONALAZAK MIŠA NA Y-Z RAVNINI
        Plane yzRavnina = new Plane(Vector3.right, transform.position);
        Ray zrakaIzKamere = glavnaKamera.ScreenPointToRay(Input.mousePosition);

        float udaljenostDoRavnine;
        Vector3 pozicijaMisaUSvijetu = Vector3.zero;

        if (yzRavnina.Raycast(zrakaIzKamere, out udaljenostDoRavnine))
        {
            pozicijaMisaUSvijetu = zrakaIzKamere.GetPoint(udaljenostDoRavnine);
        }

        // 2. STABILNA ROTACIJA (Sada u pravom smjeru!)
        Vector3 smjer = pozicijaMisaUSvijetu - transform.position;

        float kut = Mathf.Atan2(smjer.y, smjer.z) * Mathf.Rad2Deg;
        kut += offsetKuta;

        // OVDJE JE POPRAVAK: Dodali smo MINUS ispred 'kut' (-kut) da obrnemo smjer rotacije!
        Quaternion fiksniY = Quaternion.Euler(0f, -90f, 0f);
        Quaternion rotacijaOkoX = Quaternion.AngleAxis(-kut, Vector3.right);

        transform.rotation = rotacijaOkoX * fiksniY;

        // 3. TELESKOPSKA LOGIKA SA OFFSETOM I TAMPON ZONOM
        float udaljenostMisa = Vector3.Distance(transform.position, pozicijaMisaUSvijetu);

        // Dodajemo pragUvlacenja na min i max granice
        float minUdaljenost = Vector3.Distance(transform.position, pocetakShaka.position) + pragUvlacenja;
        float maxUdaljenost = Vector3.Distance(transform.position, krajShaka.position) + pragUvlacenja;

        // Sada će InverseLerp vratiti 0 (skroz uvučeno) čim miš dođe unutar tog kruga oko Mirande!
        float postotakIzvlacenja = Mathf.InverseLerp(minUdaljenost, maxUdaljenost, udaljenostMisa);

        Vector3 ciljnaPozicijaSpoja = Vector3.Lerp(pocetakShaka.localPosition, krajShaka.localPosition, postotakIzvlacenja);
        Vector3 ciljnaPozicijaShake = ciljnaPozicijaSpoja - spojShaka.localPosition;

        // Primjena kretanja (SmoothDamp)
        shakaObjekt.localPosition = Vector3.SmoothDamp(
            shakaObjekt.localPosition,
            ciljnaPozicijaShake,
            ref trenutnaBrzinaShake,
            glatkocaIzvlacenja
        );

        // 4. LOGIKA ZA PROMJENU SPRITE-A ŠAKE
        if (shakaSpriteRenderer != null)
        {
            bool previousStisnuta = isStisnuta;
            isStisnuta = Input.GetMouseButton(0);

            // ZVUKOVI ŠAKE:
            if (isStisnuta && !previousStisnuta)
            {
                // Upravo je stisnuo šaku (Klik)
                if (mirandaAudio != null) mirandaAudio.PlayFistClose();
            }
            else if (!isStisnuta && previousStisnuta)
            {
                // Upravo je pustio šaku (Otpust)
                if (mirandaAudio != null) mirandaAudio.PlayFistOpen();
            }

            // Promjena spritea...
            if (isStisnuta && spriteStisnutaShaka != null)
            {
                shakaSpriteRenderer.sprite = spriteStisnutaShaka;
            }
            else if (!isStisnuta && spriteOtvorenaShaka != null)
            {
                shakaSpriteRenderer.sprite = spriteOtvorenaShaka;
            }

            if (shakaCollider != null)
            {
                shakaCollider.enabled = isStisnuta;
            }
        }
    }
}