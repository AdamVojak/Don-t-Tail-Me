using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MirandaClimaxManager : MonoBehaviour
{
    [Header("Kamere")]
    public CinemachineCamera mirandaCam;   // Glavna Mirandina kamera
    public CinemachineCamera doorCam;      // NOVO: Kamera uperena u ulazna vrata
    public CinemachineCamera wormCam;      // Kamera uperena u crva/rupu
    public float adrenalineLensSize = 8f;  // Povećanje leće na 8
    public float lensZoomDuration = 2f;

    [Header("Brzine Crva (moveInterval)")]
    [Tooltip("Veći broj = sporiji crv")]
    public float sporiCrvInterval = 0.45f; // Kad Miranda povuče polugu (lakše)
    public float brziCrvInterval = 0.22f;  // Kad istekne vrijeme (panika!)

    [Header("Reference")]
    public VentWormAI crvAI;
    public UlaznaVrata ulaznaVrata;
    public MirandaController mirandaController; // Da je možemo pauzirati dok gleda animaciju
    public Transform doorThresholdZ;
    public TimerUI timerUI; // Da možemo ugasiti kucanje sata kad krene bježanija!

    private bool climaxStarted = false;
    private bool wormPassedDoor = false;

    // A) Poziva Poluga (Lever) kada Miranda riješi level:
    public void PokreniKlimaksPrekoPoluge()
    {
        if (climaxStarted) return;
        StartCoroutine(ClimaxRoutine(true));
    }

    // B) Poziva Tajmer ako vrijeme istekne:
    public void PokreniKlimaksPrekoTajmera()
    {
        if (climaxStarted) return;
        StartCoroutine(ClimaxRoutine(false));
    }

    IEnumerator ClimaxRoutine(bool prekoPoluge)
    {
        climaxStarted = true;

        if (mirandaController != null) mirandaController.SetLock(true);
        if (timerUI != null) timerUI.ZaustaviIUgasiTimer();

        // 2. Postavljamo brzinu kretanja crva
        if (crvAI != null)
        {
            crvAI.SetMoveInterval(prekoPoluge ? sporiCrvInterval : brziCrvInterval);
        }

        // =========================================================================
        // KLJUČNA PROMJENA: CRV ODMAH KREĆE U MRAKU HODNIKA!
        // Dok se vrata budu podizala, on će već napraviti par koraka i stvoriti
        // sve dijelove tijela i rep, tako da pred kameru izlazi POTPUNO FORMIRAN!
        // =========================================================================
        if (crvAI != null)
        {
            crvAI.gameObject.SetActive(true);
            crvAI.enabled = true;
        }


        // =========================================================================
        // FAZA 1: DIZANJE VRATA
        // =========================================================================
        if (prekoPoluge)
        {
            // Kamera gleda vrata dok se podižu
            if (doorCam != null) doorCam.Priority = 30;

            if (ulaznaVrata != null)
            {
                ulaznaVrata.otvorenaPrekoPoluge = true;
                ulaznaVrata.OtvoriVrata();

                // Čekamo na kameri vrata dok se ona potpuno ne podignu
                while (ulaznaVrata.DaLiSeMicu())
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // Ako je preko tajmera
            if (ulaznaVrata != null) ulaznaVrata.OtvoriVrata();
        }


        // =========================================================================
        // FAZA 2: KAMERA PRELAZI NA CRVA (KOJI JE SADA VEĆ VELIK I IMA CIJELO TIJELO!)
        // =========================================================================
        if (doorCam != null) doorCam.Priority = 0;
        if (wormCam != null) wormCam.Priority = 35;

        // Gledamo gotovog, spojenog crva točno 1.2 sekunde dok izlazi ispod otvorenih vrata
        yield return new WaitForSeconds(1.2f);


        // =========================================================================
        // FAZA 3: POVRATAK NA MIRANDU + ADRENALINSKI LENS (5 -> 8)
        // =========================================================================
        if (wormCam != null) wormCam.Priority = 0; // Kamera se vraća na Mirandu

        // Vraćamo kontrole igraču (Bježi!)
        if (mirandaController != null) mirandaController.SetLock(false);

        // Glatko širimo kadar (leću) sa 5 na 8
        if (mirandaCam != null)
        {
            float startLens = mirandaCam.Lens.OrthographicSize;
            float elapsed = 0f;

            while (elapsed < lensZoomDuration)
            {
                elapsed += Time.deltaTime;
                mirandaCam.Lens.OrthographicSize = Mathf.Lerp(startLens, adrenalineLensSize, elapsed / lensZoomDuration);
                yield return null;
            }
            mirandaCam.Lens.OrthographicSize = adrenalineLensSize;
        }
    }

    void Update()
    {
        // Provjera kad rep crva prođe kroz vrata
        if (climaxStarted && !wormPassedDoor && crvAI != null && crvAI.IsTailSpawned())
        {
            Transform repCrva = crvAI.GetTailTransform();

            if (repCrva != null && doorThresholdZ != null)
            {
                if (repCrva.position.z > doorThresholdZ.position.z)
                {
                    wormPassedDoor = true;
                    if (ulaznaVrata != null) ulaznaVrata.ZatvoriNakonCrva();
                }
            }
        }
    }
}