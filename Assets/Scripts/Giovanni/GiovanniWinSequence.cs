using System.Collections;
using UnityEngine;

public class GiovanniWinSequence : MonoBehaviour
{
    [Header("Divovska Lignja")]
    public GameObject divovskaLignja;
    public Transform lignjaPocetnaTocka;     // TVOJA POČETNA TOČKA U SCENI
    public Transform lignjaKrajnjaTocka;     // TVOJA KRAJNJA TOČKA U SCENI
    public float trajanjeSpustanja = 4.0f;   // Vrijeme kretanja između točaka

    [Header("Bistrenje Vode i Magla")]
    public float cistaMaglaGustoca = 0.008f; // Smanjena magla da se lignja jasno vidi
    public Color osvijetljenaVoda = new Color(0.05f, 0.2f, 0.4f); // Plavičasto svjetlo vode

    [Header("Audio (Krik Lignje)")]
    public AudioSource squidAudioSource;
    public AudioClip squidRoarClip;

    [Header("Reference")]
    public GiovanniController giovanniController;

    private bool sequenceStarted = false;

    void Start()
    {
        if (divovskaLignja != null) divovskaLignja.SetActive(false);
        if (giovanniController == null) giovanniController = FindFirstObjectByType<GiovanniController>();
    }

    public void PokreniScenuLignje(bool jePravaPobjeda)
    {
        if (sequenceStarted) return;
        StartCoroutine(DirectSquidRoutine(jePravaPobjeda));
    }

    IEnumerator DirectSquidRoutine(bool jePravaPobjeda)
    {
        sequenceStarted = true;

        // 1. ZAMRZAVAMO GIOVANNIJA
        if (giovanniController != null)
        {
            giovanniController.SetLock(true);
            giovanniController.isControlled = false;
        }

        // 2. BISTRIMO VODU (Da magla ne proguta tvoje točke)
        StartCoroutine(BistriVoduRoutine(1.5f));

        // 3. GIOVANNI GLATKO DIŽE POGLED I PALI BATERIJU
        if (giovanniController != null)
        {
            yield return StartCoroutine(giovanniController.LookUpAndAimLightRoutine(1.0f));
        }

        // 4. POSTAVLJAMO LIGNJU TOČNO NA TVOJU POČETNU TOČKU
        if (divovskaLignja != null && lignjaPocetnaTocka != null)
        {
            divovskaLignja.transform.position = lignjaPocetnaTocka.position;
            divovskaLignja.transform.rotation = lignjaPocetnaTocka.rotation;
            divovskaLignja.SetActive(true);
        }

        // KRIK LIGNJE
        if (squidAudioSource != null && squidRoarClip != null)
        {
            squidAudioSource.PlayOneShot(squidRoarClip);
        }

        // 5. GLATKO KRETANJE TOČNO OD POČETNE DO KRAJNJE TOČKE
        if (lignjaPocetnaTocka != null && lignjaKrajnjaTocka != null)
        {
            Vector3 startPos = lignjaPocetnaTocka.position;
            Vector3 endPos = lignjaKrajnjaTocka.position;

            float elapsed = 0f;
            while (elapsed < trajanjeSpustanja)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / trajanjeSpustanja;

                if (divovskaLignja != null)
                {
                    // Kreće se striktno po tvojim koordinatama
                    divovskaLignja.transform.position = Vector3.Lerp(startPos, endPos, t);
                }

                yield return null;
            }
        }

        // 6. GASIMO ZVUK I REZ NA POBJEDU / FAIL
        if (squidAudioSource != null) squidAudioSource.Stop();

        if (GameManager.Instance != null)
        {
            if (jePravaPobjeda)
            {
                GameManager.Instance.WinCurrentCharacter();
            }
            else
            {
                GameManager.Instance.TriggerGiovanniFail();
            }
        }
    }

    IEnumerator BistriVoduRoutine(float duration)
    {
        float elapsed = 0f;
        float startDensity = RenderSettings.fogDensity;
        Color startAmbient = RenderSettings.ambientLight;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            RenderSettings.fogDensity = Mathf.Lerp(startDensity, cistaMaglaGustoca, t);
            RenderSettings.ambientLight = Color.Lerp(startAmbient, osvijetljenaVoda, t);

            yield return null;
        }

        RenderSettings.fogDensity = cistaMaglaGustoca;
        RenderSettings.ambientLight = osvijetljenaVoda;
    }
}