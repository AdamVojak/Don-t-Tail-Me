using System.Collections;
using UnityEngine;

public class GiovanniElevatorEscape : MonoBehaviour
{
    [Header("Postavke Lifta")]
    public float visinaDizanja = 15f;
    public float trajanjeVoznje = 5f;
    public float pauzaPrijePaljenja = 0.5f;

    [Header("Pozicioniranje Igrača")]
    [Tooltip("Prazan objekt u centru lifta gdje će se Giovanni teleportirati")]
    public Transform centarLifta; // NOVO: Uvuci točku iz centra lifta!

    [Header("Audio Izvori (2D ili 3D)")]
    public AudioSource sfxSource;
    public AudioSource loopSource;

    [Header("Audio Klipovi")]
    public AudioClip elevatorStartSound;
    public AudioClip elevatorMovingSound;

    private bool isEscaping = false;
    private GiovanniController giovanni;

    public void PokreniBijegLiftom()
    {
        if (isEscaping) return;
        StartCoroutine(ElevatorRoutine());
    }

    IEnumerator ElevatorRoutine()
    {
        isEscaping = true;
        Debug.Log("<color=cyan>LIFT: Giovanni bježi liftom!</color>");

        // 1. PRONALAZIMO GIOVANNIJA I ZAMRZAVAMO GA
        giovanni = FindFirstObjectByType<GiovanniController>();
        if (giovanni != null)
        {
            // Oduzimamo mu brzinu kretanja
            giovanni.moveSpeed = 0f;
            giovanni.sprintMultiplier = 0f;

            // =========================================================
            // NOVO: SNAPAMO GIOVANNIJA TOČNO U CENTAR LIFTA!
            // =========================================================
            if (centarLifta != null)
            {
                // Isključujemo CharacterController na milisekundu da dopusti teleportaciju
                CharacterController cc = giovanni.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // Postavljamo ga na X i Z od centra lifta, a Y ostavljamo njegov (da ne propadne u pod)
                Vector3 novaPozicija = new Vector3(centarLifta.position.x, giovanni.transform.position.y, centarLifta.position.z);
                giovanni.transform.position = novaPozicija;

                if (cc != null) cc.enabled = true;
            }

            // Stavljamo Giovannija kao dijete lifta da se diže s njim
            giovanni.transform.SetParent(this.transform);
        }

        // 2. KRATKA PAUZA NAKON KLIKA GUMBA
        yield return new WaitForSeconds(pauzaPrijePaljenja);

        // 3. PUŠTAMO ZVUK PALJENJA MOTORA I ČEKAMO DA ZAVRŠI
        float startSoundDuration = 0f;
        if (sfxSource != null && elevatorStartSound != null)
        {
            sfxSource.PlayOneShot(elevatorStartSound);
            startSoundDuration = elevatorStartSound.length;
        }

        // OSIGURAČ: Ako zvuka nema, čekamo minimalno 0.5s da lift ne krene prebrzo
        if (startSoundDuration <= 0f) startSoundDuration = 0.5f;

        yield return new WaitForSeconds(startSoundDuration);


        // 4. PALIMO LOOP ZVUK VOŽNJE I KREĆEMO GORE
        if (loopSource != null && elevatorMovingSound != null)
        {
            loopSource.clip = elevatorMovingSound;
            loopSource.loop = true;
            loopSource.Play();
        }

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, visinaDizanja, 0);

        while (elapsed < trajanjeVoznje)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / trajanjeVoznje;

            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        transform.position = endPos;

        // 6. POKREĆEMO POBJEDU U GAME MANAGERU
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinCurrentCharacter();
        }

        // 7. GLATKI FADE-OUT ZVUKA VOŽNJE
        if (loopSource != null)
        {
            yield return StartCoroutine(FadeOutAudioRoutine(loopSource, 1.5f));
        }
    }

    private IEnumerator FadeOutAudioRoutine(AudioSource audioSrc, float fadeDuration)
    {
        float startVolume = audioSrc.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSrc.volume = 0f;
        audioSrc.Stop();
        audioSrc.volume = startVolume;
    }
}