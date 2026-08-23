using UnityEngine;
using UnityEngine.Events;

public class StropnaVrata : MonoBehaviour
{
    [Header("Reference")]
    public Transform spojVrata;
    public Transform krajPlatforme;
    public Transform knob;
    public LineRenderer uzeRenderer;

    [Header("Kutovi Rotacije")]
    public float kutZatvoreno = 0f;
    public float kutOtvoreno = -80f;

    [Header("Postavke")]
    public float duzinaUzeta = 2.5f;
    public float radiusHvatanja = 1.0f;
    public float glatkocaOtvaranja = 0.35f;
    public float xOffsetDrzanja = 5.5f;

    [Header("Postavke Težine i Otpora")]
    public float tezinaVrata = 0.4f;
    public float faktorOtpora = 1.5f;

    [Header("Događaj kada se otvore")]
    public UnityEvent onOtvoreno;

    private bool isDragging = false;
    private bool isTrajnoOtvoreno = false;
    private float fiksniX;
    private float fiksniZ_Knoba;
    private float trenutniKut = 0f;
    private float brzinaRotacijeGlancanje;
    private Quaternion pocetnaLokalnaRotacijaSpoja;
    private Quaternion pocetnaRotacijaKnoba;
    private MirandaRuka ruka;

    void Start()
    {
        if (spojVrata != null)
        {
            pocetnaLokalnaRotacijaSpoja = spojVrata.localRotation;
        }

        if (knob != null && krajPlatforme != null)
        {
            fiksniX = knob.position.x;
            fiksniZ_Knoba = krajPlatforme.position.z;
            pocetnaRotacijaKnoba = knob.rotation;

            knob.position = new Vector3(fiksniX, krajPlatforme.position.y - duzinaUzeta, fiksniZ_Knoba);
        }

        trenutniKut = kutZatvoreno;
        PrimijeniRotaciju(trenutniKut);

        // NOVO: Nacrtaj uže odmah pri pokretanju igre!
        CrtajUze();
    }

    void Update()
    {
        // 1. AKO SU VRATA ZAKLJUČANA OTVORENA
        if (isTrajnoOtvoreno)
        {
            trenutniKut = Mathf.SmoothDampAngle(trenutniKut, kutOtvoreno, ref brzinaRotacijeGlancanje, glatkocaOtvaranja);
            PrimijeniRotaciju(trenutniKut);

            if (knob != null && krajPlatforme != null)
            {
                Vector3 ciljKnob = new Vector3(fiksniX, krajPlatforme.position.y - duzinaUzeta, krajPlatforme.position.z);
                knob.position = Vector3.Lerp(knob.position, ciljKnob, Time.deltaTime * 5f);
                knob.rotation = pocetnaRotacijaKnoba;
            }

            CrtajUze();
            return;
        }

        // 2. EFEKT OPRUGE (Ovo se sada izvršava UVIJEK ako nitko ne vuče uže, čak i ako ruka ne postoji)
        if (!isDragging)
        {
            trenutniKut = Mathf.SmoothDampAngle(trenutniKut, kutZatvoreno, ref brzinaRotacijeGlancanje, 0.2f);
            PrimijeniRotaciju(trenutniKut);

            if (knob != null && krajPlatforme != null)
            {
                float ciljniY = krajPlatforme.position.y - duzinaUzeta;
                Vector3 ciljnaPozicijaKnoba = new Vector3(fiksniX, ciljniY, fiksniZ_Knoba);
                knob.position = Vector3.Lerp(knob.position, ciljnaPozicijaKnoba, Time.deltaTime * 10f);
                knob.rotation = pocetnaRotacijaKnoba;
            }
        }

        // 3. INTERAKCIJA S RUKOM (Samo ako je ruka upaljena i postoji)
        if (ruka == null) ruka = Object.FindAnyObjectByType<MirandaRuka>();

        if (ruka != null && knob != null && krajPlatforme != null && spojVrata != null)
        {
            Transform tockaPrstiju = ruka.GetTockaHvatanja();
            float udaljenostDoKnoba = UdaljenostYZ(tockaPrstiju.position, knob.position);

            // POČETAK POVLAČENJA
            if (!isDragging && udaljenostDoKnoba <= radiusHvatanja && ruka.isStisnuta)
            {
                isDragging = true;
                Debug.Log("Vrata: Uže zgrabljeno!");
            }

            // PUŠTANJE KLIKA
            if (isDragging && !ruka.isStisnuta)
            {
                isDragging = false;
                ZavrsiPovlacenje();
            }

            // POVLAČENJE DOK IGRAČ DRŽI MIŠ
            if (isDragging)
            {
                float razlikaY = tockaPrstiju.position.y - spojVrata.position.y;

                float potrebnaDubina = (-duzinaUzeta - 1.5f) * faktorOtpora;
                float postotak = Mathf.InverseLerp(0f, potrebnaDubina, razlikaY);
                postotak = Mathf.Clamp01(postotak);

                float ciljniKut = Mathf.Lerp(kutZatvoreno, kutOtvoreno, postotak);
                trenutniKut = Mathf.SmoothDampAngle(trenutniKut, ciljniKut, ref brzinaRotacijeGlancanje, tezinaVrata);
                PrimijeniRotaciju(trenutniKut);

                knob.position = new Vector3(fiksniX + xOffsetDrzanja, tockaPrstiju.position.y, tockaPrstiju.position.z);
                knob.rotation = pocetnaRotacijaKnoba;

                if (postotak >= 0.60f)
                {
                    AktivirajTrajnoOtvaranje();
                }
            }
        }

        // 4. UVIJEK CRTAJ UŽE NA KRAJU FRAME-A (Neovisno o ruci)
        CrtajUze();
    }

    void ZavrsiPovlacenje()
    {
        float prag = Mathf.Lerp(kutZatvoreno, kutOtvoreno, 0.60f);
        bool dovoljnoOtvoreno = (kutOtvoreno < kutZatvoreno) ? (trenutniKut <= prag) : (trenutniKut >= prag);

        if (dovoljnoOtvoreno)
        {
            AktivirajTrajnoOtvaranje();
        }
    }

    void AktivirajTrajnoOtvaranje()
    {
        if (isTrajnoOtvoreno) return;

        isTrajnoOtvoreno = true;
        isDragging = false;
        onOtvoreno.Invoke();
        Debug.Log("Vrata: PLATFORMA JE TRAJNO OTVORENA!");
    }

    void PrimijeniRotaciju(float deltaKut)
    {
        if (spojVrata != null)
        {
            spojVrata.localRotation = pocetnaLokalnaRotacijaSpoja * Quaternion.Euler(deltaKut, 0f, 0f);
        }
    }

    void CrtajUze()
    {
        if (uzeRenderer != null && krajPlatforme != null && knob != null)
        {
            uzeRenderer.positionCount = 2;
            uzeRenderer.SetPosition(0, krajPlatforme.position);
            uzeRenderer.SetPosition(1, knob.position);
        }
    }

    float UdaljenostYZ(Vector3 tockaA, Vector3 tockaB)
    {
        return Vector2.Distance(new Vector2(tockaA.z, tockaA.y), new Vector2(tockaB.z, tockaB.y));
    }
}