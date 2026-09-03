using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MirandaClimaxManager : MonoBehaviour
{
    [Header("Kamere")]
    public CinemachineCamera mirandaCam;   // Glavna Mirandina kamera
    public CinemachineCamera wormCam;      // Kamera koja gleda rupu/crva
    public float adrenalineLensSize = 7f;  // Povećanje leće (sa 5 na 7)
    public float lensZoomDuration = 1.5f;  // Koliko glatko se širi leća

    [Header("Reference")]
    public VentWormAI crvAI;               // Skripta crva
    public UlaznaVrata ulaznaVrata;        // Ulazna vrata kroz koja crv prolazi
    public Transform doorThresholdZ;       // Pozicija na Z osi nakon koje se smatra da je rep prošao vrata

    private bool climaxStarted = false;
    private bool wormPassedDoor = false;

    // Ovu metodu poziva tvoj tajmer (TimerUI) kada istekne vrijeme!
    public void StartClimaxSequence()
    {
        if (climaxStarted) return;
        StartCoroutine(ClimaxRoutine());
    }

    IEnumerator ClimaxRoutine()
    {
        climaxStarted = true;

        // 1. OTVARAMO ULAZNA VRATA DA CRV MOŽE IZAĆI
        if (ulaznaVrata != null)
        {
            ulaznaVrata.OtvoriVrata();
        }

        // 2. KAMERA PRELAZI NA CRVA
        if (wormCam != null) wormCam.Priority = 20; // Veći prioritet preuzima kadar

        // Palimo crva da krene puzati
        if (crvAI != null) crvAI.enabled = true;

        // Gledamo crva kako izlazi 1.5 sekundu
        yield return new WaitForSeconds(1.5f);

        // 3. KAMERA SE VRAĆA NA MIRANDU
        if (wormCam != null) wormCam.Priority = 0;

        // 4. ADRENALINSKI EFEKT: Lagano širimo leću (Orthographic Size sa 5 na 7)
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
        // Pratimo je li crv cijeli (s repom) prošao kroz vrata
        if (climaxStarted && !wormPassedDoor && crvAI != null && crvAI.IsTailSpawned())
        {
            Transform repCrva = crvAI.GetTailTransform();

            if (repCrva != null && doorThresholdZ != null)
            {
                // Provjeravamo je li rep prešao liniju vrata po Z osi
                // (Prilagodi '>' ili '<' ovisno o smjeru hodnika!)
                if (repCrva.position.z > doorThresholdZ.position.z)
                {
                    wormPassedDoor = true;

                    // Javljamo vratima da se zatvore (ako nisu otvorena polugom)
                    if (ulaznaVrata != null)
                    {
                        ulaznaVrata.ZatvoriNakonCrva();
                    }
                }
            }
        }
    }
}