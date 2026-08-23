using UnityEngine;
using System.Collections.Generic;

public class SashaSvjetlaKontroler : MonoBehaviour
{
    public static SashaSvjetlaKontroler Instance { get; private set; }

    [Header("Postavke")]
    public string imeSashaLeveli = "Sasha_Leveli";
    public bool upravljanjeAktivno = false;

    // Pomoćna klasa u kojoj pamtimo svjetlo i njegov originalni intenzitet
    [System.Serializable]
    public class PodaciSvjetla
    {
        public Light svjetlo;
        public float originalniIntenzitet;
    }

    // Lista u kojoj držimo sva svjetla iz svih 5 soba
    private List<PodaciSvjetla> svaSvjetla = new List<PodaciSvjetla>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Poziva se iz ZombieSobaZamka skripte
    public void AktivirajUpravljanje()
    {
        upravljanjeAktivno = true;
        SkenirajSvaSvjetla();
        Debug.Log("Sustav struje preuzeo kontrolu nad svjetlima. Započinjem praćenje 10% pravila!");
    }

    // Skenira cijeli Sasha_Leveli objekt i sprema originalne vrijednosti
    private void SkenirajSvaSvjetla()
    {
        svaSvjetla.Clear();
        GameObject sashaLeveliObj = GameObject.Find(imeSashaLeveli);

        if (sashaLeveliObj != null)
        {
            // (true) znači da pronalazi svjetla čak i u sobama koje su trenutno SetActive(false)
            Light[] pronadjenaSvjetla = sashaLeveliObj.GetComponentsInChildren<Light>(true);

            foreach (Light l in pronadjenaSvjetla)
            {
                // Spremamo referencu na svjetlo i njegov početni intenzitet
                PodaciSvjetla noviPodatak = new PodaciSvjetla();
                noviPodatak.svjetlo = l;
                noviPodatak.originalniIntenzitet = l.intensity;

                svaSvjetla.Add(noviPodatak);
            }
        }
    }

    private void Update()
    {
        if (!upravljanjeAktivno || EnergyManager.Instance == null) return;

        // 1. Računamo postotak struje (0 do 100)
        float trenutnaStruja = EnergyManager.Instance.struja;
        float maxStruja = EnergyManager.Instance.maxStruja;
        float postotakStruje = (trenutnaStruja / maxStruja) * 100f;

        // 2. Određujemo množitelj intenziteta
        float mnoziteljIntenziteta = 1f; // Po defaultu je 1 (svjetla rade 100% normalno)

        if (postotakStruje <= 10f)
        {
            // Ako je struja na 10%, množitelj je 1. Ako je na 5%, množitelj je 0.5. Ako je 0, množitelj je 0.
            mnoziteljIntenziteta = postotakStruje / 10f;
        }

        // 3. Primjenjujemo matematiku na sva svjetla
        foreach (PodaciSvjetla podatak in svaSvjetla)
        {
            // Provjeravamo postoji li svjetlo i je li njegova soba trenutno upaljena (radi optimizacije)
            if (podatak.svjetlo != null && podatak.svjetlo.gameObject.activeInHierarchy)
            {
                // Mijenjamo intenzitet. 
                // Ako je struja > 10%, množi se s 1 (ostaje isto).
                // Ako je struja u padu ispod 10%, množi se s decimalnim brojem i radi fade-out.
                podatak.svjetlo.intensity = podatak.originalniIntenzitet * mnoziteljIntenziteta;
            }
        }
    }
}