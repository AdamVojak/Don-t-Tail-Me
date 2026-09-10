using UnityEngine;

public class WormsSobaZamka : MonoBehaviour
{
    [Header("Elementi Zamke u Sobi")]
    public GameObject svijetlo;
    public LeverSasha[] lever;
    public GumbSasha[] gumbi;
    public Door vrata;
    private bool trapActivated = false;

    // Pamtimo prijašnja stanja gumbiju i levera kako bismo znali kad su stisnuti
    private bool[] proslaStanjaGumba;
    private bool[] proslaStanjaLevera;

    [Header("Audio (2D Kratki Spoj / Power Outage)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerOutageClip;
    [Range(0f, 1f)][SerializeField] private float outageVolume = 1f;

    [Header("Predmeti koji pokreću zamku")]
    public GameObject predmetZaPokupiti;     // Glavni predmet (Default & S/G)
    public GameObject predmetZaPokupitiSM;   // Alternativni predmet (S/M)

    private GameObject stvarniPredmetUSobi;
    private bool sobaImaPredmet = false;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Start()
    {
        if (predmetZaPokupiti != null)
        {
            stvarniPredmetUSobi = predmetZaPokupiti;
            sobaImaPredmet = true;
        }
        else if (predmetZaPokupitiSM != null)
        {
            stvarniPredmetUSobi = predmetZaPokupitiSM;
            sobaImaPredmet = true;
        }

        if (gumbi != null)
        {
            proslaStanjaGumba = new bool[gumbi.Length];
            for (int i = 0; i < gumbi.Length; i++)
            {
                if (gumbi[i] != null) proslaStanjaGumba[i] = gumbi[i].aktiviran;
            }
        }

        // Inicijaliziramo praćenje stanja levera
        if (lever != null)
        {
            proslaStanjaLevera = new bool[lever.Length];
            for (int i = 0; i < lever.Length; i++)
            {
                if (lever[i] != null) proslaStanjaLevera[i] = lever[i].aktiviran;
            }
        }
    }

    void Update()
    {
        if (!trapActivated && sobaImaPredmet && stvarniPredmetUSobi == null)
        {
            AktivirajZamku();
        }

        // 2. NADZOR GUMBIJU (ostaje isto)
        ProvjeriGumbe();

        // 3. NADZOR LEVERA (ostaje isto)
        ProvjeriLevere();
    }

    void ProvjeriGumbe()
    {
        if (gumbi == null) return;

        for (int i = 0; i < gumbi.Length; i++)
        {
            if (gumbi[i] != null)
            {
                // Ako je gumb promijenio stanje (stisnut je):
                if (gumbi[i].aktiviran != proslaStanjaGumba[i])
                {
                    proslaStanjaGumba[i] = gumbi[i].aktiviran;

                    // Šaljemo naredbu vratima!
                    if (vrata != null)
                    {
                        vrata.OdrediSmjerIPokreni();
                    }
                }
            }
        }
    }

    void ProvjeriLevere()
    {
        if (lever == null) return;

        for (int i = 0; i < lever.Length; i++)
        {
            if (lever[i] != null)
            {
                if (lever[i].aktiviran != proslaStanjaLevera[i])
                {
                    proslaStanjaLevera[i] = lever[i].aktiviran;

                    if (vrata != null)
                    {
                        vrata.OdrediSmjerIPokreni();
                    }
                }
            }
        }
    }

    void AktivirajZamku()
    {
        trapActivated = true;

        // 1. Zvuk kratkog spoja
        if (audioSource != null && powerOutageClip != null)
        {
            audioSource.PlayOneShot(powerOutageClip, outageVolume);
        }

        // 2. Uništi svjetlo
        if (svijetlo != null)
        {
            Destroy(svijetlo.gameObject);
        }

        // 3. Resetiraj gumbe i levere i uskladi njihova stanja da ne okidaju vrata ponovo
        if (gumbi != null)
        {
            for (int i = 0; i < gumbi.Length; i++)
            {
                if (gumbi[i] != null)
                {
                    gumbi[i].aktiviran = false;
                    proslaStanjaGumba[i] = false;
                }
            }
        }

        if (lever != null)
        {
            for (int i = 0; i < lever.Length; i++)
            {
                if (lever[i] != null)
                {
                    lever[i].aktiviran = false;
                    proslaStanjaLevera[i] = false;
                }
            }
        }

        // 4. POKRENI VRATA (Zatvaraju se bez duplog poziva!)
        if (vrata != null)
        {
            vrata.OdrediSmjerIPokreni(); // SAMO JEDAN ČISTI POZIV!
        }

        // 5. Prebacivanje struje na Mirandu
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.struja = 0f;
        }

        if (SashaSvjetlaKontroler.Instance != null)
        {
            SashaSvjetlaKontroler.Instance.AktivirajUpravljanje();
        }

        Debug.Log("Worms Soba: Zamka aktivirana! Vrata se zatvaraju, struja prebačena na Mirandu.");
    }
}