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

    private int sklopkaUkljucena;

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

        if (!trapActivated && predmetZaPokupiti == null)
        {
            AktivirajZamku();
            allTurnedOff = false;

            if (trapActivated)
            {
                foreach (SklopkaSpawner sk in sklopke)
                {
                    if (sk.isOn)
                    {
                        sklopkaUkljucena++;
                    }
                    if (!sk.isOn)
                    {
                        sklopkaUkljucena--;
                    }
                }
                if (sklopkaUkljucena == 0)
                {
                    allTurnedOff = true;
                }
            }
        }
        if (allTurnedOff)
        {
            crvenaSvijetla.SetActive(false);
        }
    }
    void AktivirajZamku()
    {
        trapActivated = true;

        glavnoSvijetlo.SetActive(false);
        crvenaSvijetla.SetActive(true);
        strujaUI.SetActive(true);
        sklopkaUkljucena = 2;

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
}