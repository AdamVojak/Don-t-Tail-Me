using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class GiovanniWinSequence : MonoBehaviour
{
    [Header("Kamere (Cinemachine)")]
    public CinemachineCamera kameraCijevi;   // Gleda cijevi dok pucaju
    public CinemachineCamera kameraLignja;   // Gleda prema stropu/lignji

    [Header("Cijevi koje se ruše")]
    public Transform cijevLijeva;
    public Transform cijevLijevaCiljNaPodu;  // Prazan objekt na podu kamo lijeva cijev padne
    public Transform cijevDesna;
    public Transform cijevDesnaCiljNaPodu;  // Prazan objekt na podu kamo desna cijev padne
    public float brzinaPadaCijevi = 2.5f;

    [Header("Divovska Lignja (Gun)")]
    public GameObject divovskaLignja;        // Model ogromne vampirske lignje
    public Transform lignjaPocetnaTocka;     // Visoko gore u magli/stropu
    public Transform lignjaCiljnaTocka;      // Iznad Giovannijeve glave
    public float brzinaSpustanjaLignje = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip zvukLomaCijevi;         // Metalno škripanje / pucanje
    public AudioClip zvukLignje;             // Duboki vodeni huk / glasanje nemani

    [Header("Reference")]
    public GiovanniController giovanniController;

    private bool sequenceStarted = false;

    // 1. Prima informaciju od cijevi
    public void PokreniScenuLignje(bool jePravaPobjeda)
    {
        if (sequenceStarted) return;
        StartCoroutine(SquidArrivalRoutine(jePravaPobjeda));
    }

    // 2. OVDJE JE BIO POPRAVAK: Dodan je (bool jePravaPobjeda) u zagradu!
    IEnumerator SquidArrivalRoutine(bool jePravaPobjeda)
    {
        sequenceStarted = true;

        // 1. ODUZIMAMO KONTROLE GIOVANNIJU
        if (giovanniController != null)
        {
            giovanniController.SetLock(true);
            giovanniController.isControlled = false;
        }

        // 2. KAMERA GLEDA CIJEVI DOK PUCAJU
        if (kameraCijevi != null) kameraCijevi.Priority = 30;

        if (audioSource != null && zvukLomaCijevi != null)
        {
            audioSource.PlayOneShot(zvukLomaCijevi);
        }

        // 3. RUŠENJE CIJEVI NA POD
        float elapsedTubes = 0f;
        Vector3 startPosL = cijevLijeva != null ? cijevLijeva.position : Vector3.zero;
        Quaternion startRotL = cijevLijeva != null ? cijevLijeva.rotation : Quaternion.identity;
        Vector3 startPosD = cijevDesna != null ? cijevDesna.position : Vector3.zero;
        Quaternion startRotD = cijevDesna != null ? cijevDesna.rotation : Quaternion.identity;

        while (elapsedTubes < 1f)
        {
            elapsedTubes += Time.deltaTime * brzinaPadaCijevi;

            if (cijevLijeva != null && cijevLijevaCiljNaPodu != null)
            {
                cijevLijeva.position = Vector3.Lerp(startPosL, cijevLijevaCiljNaPodu.position, elapsedTubes);
                cijevLijeva.rotation = Quaternion.Slerp(startRotL, cijevLijevaCiljNaPodu.rotation, elapsedTubes);
            }

            if (cijevDesna != null && cijevDesnaCiljNaPodu != null)
            {
                cijevDesna.position = Vector3.Lerp(startPosD, cijevDesnaCiljNaPodu.position, elapsedTubes);
                cijevDesna.rotation = Quaternion.Slerp(startRotD, cijevDesnaCiljNaPodu.rotation, elapsedTubes);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.4f); // Kratka tišina nakon pada cijevi

        // 4. KAMERA SE DIŽE PREMA STROPU / MAGLI
        if (kameraCijevi != null) kameraCijevi.Priority = 0;
        if (kameraLignja != null) kameraLignja.Priority = 35;

        // Palimo model ogromne lignje
        if (divovskaLignja != null && lignjaPocetnaTocka != null)
        {
            divovskaLignja.transform.position = lignjaPocetnaTocka.position;
            divovskaLignja.SetActive(true);
        }

        if (audioSource != null && zvukLignje != null)
        {
            audioSource.PlayOneShot(zvukLignje);
        }

        // 5. GIGANTSKA LIGNJA TONE PREMA GIOVANNIJU
        float elapsedSquid = 0f;
        while (elapsedSquid < 2.5f)
        {
            elapsedSquid += Time.deltaTime;

            if (divovskaLignja != null && lignjaCiljnaTocka != null)
            {
                divovskaLignja.transform.position = Vector3.MoveTowards(
                    divovskaLignja.transform.position,
                    lignjaCiljnaTocka.position,
                    brzinaSpustanjaLignje * Time.deltaTime
                );
            }

            yield return null;
        }

        // 6. GASIMO ZVUKOVE
        if (audioSource != null) audioSource.Stop();

        // 7. JAVLJAMO REZULTAT GAME MANAGERU
        if (GameManager.Instance != null)
        {
            if (jePravaPobjeda)
            {
                // PRAVI WIN: Kvačica na slideru!
                GameManager.Instance.WinCurrentCharacter();
            }
            else
            {
                // PRERANA SMRT: Crveni iksić i status Dead!
                GameManager.Instance.TriggerGiovanniFail();
            }
        }
    }
}