using UnityEngine;

public class ZombieSobaZamka : MonoBehaviour
{
    [Header("Predmet koji pokreće zamku")]
    public GameObject predmetZaPokupiti;

    [Header("Elementi Zamke u Sobi")]
    public Spawner[] spawneri;
    public SklopkaSpawner[] sklopke;

    public GameObject glavnoSvijetlo;
    public GameObject strujaUI;
    public GameObject crvenaSvijetla;

    private bool trapActivated = false;
    private bool allTurnedOff = false;

    private void Awake()
    {
        glavnoSvijetlo.SetActive(true);
        crvenaSvijetla.SetActive(false);
        strujaUI.SetActive(false);
        allTurnedOff = false;
    }

    void Start()
    {
        foreach (Spawner s in spawneri)
        {
            s.SetActiveState(false);
            s.enabled = false;
        }

        foreach (SklopkaSpawner sk in sklopke)
        {
            sk.enabled = false;
            sk.SetState(false);
        }
    }

    void Update()
    {
        // 1. KORAK: Provjera treba li se zamka aktivirati
        if (!trapActivated && predmetZaPokupiti == null)
        {
            AktivirajZamku();
        }

        // 2. KORAK: Ako je zamka aktivna, a još nismo ugasili obje sklopke...
        if (trapActivated && !allTurnedOff)
        {
            // Provjeravamo je li ijedna sklopka još uvijek upaljena
            bool imaUkljucenaSklopka = false;

            foreach (SklopkaSpawner sk in sklopke)
            {
                if (sk.isOn)
                {
                    imaUkljucenaSklopka = true;
                    break; // Dovoljno je da je jedna upaljena, nema potrebe dalje provjeravati u ovom frameu
                }
            }

            // Ako niti jedna sklopka NIJE uključena (obje su ugašene)
            if (!imaUkljucenaSklopka)
            {
                DeaktivirajCrvenaSvjetla();
            }
        }
    }

    void AktivirajZamku()
    {
        trapActivated = true;

        glavnoSvijetlo.SetActive(false);
        crvenaSvijetla.SetActive(true);
        strujaUI.SetActive(true);

        foreach (Spawner s in spawneri)
        {
            s.enabled = true;
            s.SetActiveState(true);
        }

        foreach (SklopkaSpawner sk in sklopke)
        {
            sk.enabled = true;
            sk.SetState(true);
        }
    }

    // Ova funkcija se poziva točno JEDNOM čim se obje sklopke ugase
    void DeaktivirajCrvenaSvjetla()
    {
        allTurnedOff = true; // Osigurava da se ovo izvrši samo jednom i nikad više
        crvenaSvijetla.SetActive(false);
        strujaUI.SetActive(false);

        // Opcionalno: Ako želiš vratiti normalno svjetlo kad ugase sklopke:
        // glavnoSvijetlo.SetActive(true);

        Debug.Log("Sve sklopke su isključene! Crvena svjetla trajno ugašena.");
    }
}