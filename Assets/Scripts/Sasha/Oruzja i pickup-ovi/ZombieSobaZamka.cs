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

    [Header("Audio")]
    [SerializeField] private TrapRoomAudio trapAudio;

    private void Awake()
    {
        if (trapAudio == null) trapAudio = GetComponent<TrapRoomAudio>();

        glavnoSvijetlo.SetActive(true);
        crvenaSvijetla.SetActive(false);
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
            bool imaUkljucenaSklopka = false;

            foreach (SklopkaSpawner sk in sklopke)
            {
                if (sk.isOn)
                {
                    imaUkljucenaSklopka = true;
                    break;
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

        if (trapAudio != null) trapAudio.StartSiren();

        glavnoSvijetlo.SetActive(false);
        crvenaSvijetla.SetActive(true);

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

    void DeaktivirajCrvenaSvjetla()
    {
        allTurnedOff = true;
        crvenaSvijetla.SetActive(false);

        if (trapAudio != null) trapAudio.StopSirenAndPlayOutage();


        foreach (Spawner s in spawneri)
        {
            s.SetActiveState(false);
            s.enabled = false;
        }

        Debug.Log("Sve sklopke su isključene! Spawneri ugašeni.");

        // ODMAH PREBACUJEMO IGRU NA MIRANDIN SUSTAV STRUJE
        if (SashaSvjetlaKontroler.Instance != null)
        {
            SashaSvjetlaKontroler.Instance.AktivirajUpravljanje();
        }
    }
}