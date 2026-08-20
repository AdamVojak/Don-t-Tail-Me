using UnityEngine;
using System.Collections.Generic;

public class SashaSvjetlaKontroler : MonoBehaviour
{
    public static SashaSvjetlaKontroler Instance { get; private set; }

    [Header("Postavke")]
    public string imeSashaLeveli = "Sasha_Leveli";
    public bool upravljanjeAktivno = false;

    private GameObject sashaLeveliObj;
    private Light[] svaSvjetla;
    private SklopkaSpawner[] sveSklopke; // Ako koristiš sklopke u sobama

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AktivirajUpravljanje()
    {
        upravljanjeAktivno = true;
        OsveziKomponente();
        Debug.Log("Sustav struje preuzeo kontrolu nad svjetlima u Sasha_Leveli!");
    }

    // Pronalazi sve objekte, čak i ako su ugašeni!
    public void OsveziKomponente()
    {
        sashaLeveliObj = GameObject.Find(imeSashaLeveli);

        if (sashaLeveliObj != null)
        {
            // Druga varijabla (true) znači: "Traži čak i unutar ugašenih (Inactive) objekata!"
            svaSvjetla = sashaLeveliObj.GetComponentsInChildren<Light>(true);
            sveSklopke = sashaLeveliObj.GetComponentsInChildren<SklopkaSpawner>(true);
        }
    }

    private void Update()
    {
        if (!upravljanjeAktivno) return;

        // Ako se leveli učitaju/odspoje u runtime-u, ponovno osveži listu
        if (sashaLeveliObj == null || svaSvjetla == null || svaSvjetla.Length == 0)
        {
            OsveziKomponente();
        }

        if (svaSvjetla == null) return;

        // Provjeravamo ima li struje u centralnom sustavu
        bool imaStruje = EnergyManager.Instance != null && EnergyManager.Instance.struja > 0;

        // 1. UPRAVLJANJE SVJETLIMA
        foreach (Light l in svaSvjetla)
        {
            if (l != null)
            {
                // Svjetlo radi SAMO ako ima struje.
                // Ako je roditeljski objekt svjetla ugašen polugom/gumbom, 
                // Unity ga automatski neće prikazati u igri, što je savršeno!
                l.enabled = imaStruje;
            }
        }

        // 2. OPIONALNO: Ako želiš da se sklopke/gumbi uopće ne mogu stiskati dok nema struje:
        if (sveSklopke != null)
        {
            foreach (SklopkaSpawner sk in sveSklopke)
            {
                if (sk != null)
                {
                    // Onemogućujemo rad sklopki ako nema struje
                    sk.enabled = imaStruje;
                }
            }
        }
    }
}