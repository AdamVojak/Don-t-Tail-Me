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
    public GuideArrowSasha guideArrow;

    private bool trapActivated = false;
    private bool isAlertActive = false; // Prati svira li trenutno uzbuna

    [Header("Audio")]
    [SerializeField] private TrapRoomAudio trapAudio;

    private void Awake()
    {
        if (trapAudio == null) trapAudio = GetComponent<TrapRoomAudio>();

        glavnoSvijetlo.SetActive(true);
        crvenaSvijetla.SetActive(false);
        isAlertActive = false;
        trapActivated = false;
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
        // 1. KORAK: Provjera pokretanja zamke
        if (!trapActivated && predmetZaPokupiti == null)
        {
            AktivirajZamku();
        }

        // 2. KORAK: Dinamička provjera sklopki dok je zamka aktivna
        if (trapActivated)
        {
            bool imaUkljucenaSklopka = false;

            foreach (SklopkaSpawner sk in sklopke)
            {
                if (sk != null && sk.isOn)
                {
                    imaUkljucenaSklopka = true;
                    break;
                }
            }

            // A) Barem jedna sklopka radi, a uzbuna još nije upaljena -> PALI SVE
            if (imaUkljucenaSklopka && !isAlertActive)
            {
                isAlertActive = true;
                glavnoSvijetlo.SetActive(false);
                crvenaSvijetla.SetActive(true);

                if (trapAudio != null) trapAudio.StartSiren();
            }
            // B) Sve sklopke su ugašene, a uzbuna je bila upaljena -> UGASI SVE I VRATI SVJETLO
            else if (!imaUkljucenaSklopka && isAlertActive)
            {
                isAlertActive = false;
                crvenaSvijetla.SetActive(false);
                glavnoSvijetlo.SetActive(true); // Normalno svjetlo se vraća!
                guideArrow.PostaviCilj(new Vector3(-3.320000171661377f, 18.440000534057618f, 0.0f), 1f);

                if (trapAudio != null) trapAudio.StopSiren();
            }
        }
    }

    void AktivirajZamku()
    {
        trapActivated = true;
        isAlertActive = true;

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
}