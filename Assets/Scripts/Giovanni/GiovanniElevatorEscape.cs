using System.Collections;
using UnityEngine;

public class GiovanniElevatorEscape : MonoBehaviour
{
    [Header("Postavke Lifta")]
    public float visinaDizanja = 15f;    // Koliko visoko lift ide po Y osi
    public float trajanjeVoznje = 5f;    // Koliko dugo traje vožnja (u sekundama)
    public float pauzaPrijeKretanja = 0.5f; // Pauza nakon pritiska gumba

    [Header("Audio (Opcionalno)")]
    public AudioSource elevatorAudio;
    public AudioClip elevatorStartSound;
    public AudioClip elevatorMovingSound;

    private bool isEscaping = false;
    private GiovanniController giovanni;

    // Ovu metodu poziva KeyPanelController kada se stisne gumb!
    public void PokreniBijegLiftom()
    {
        if (isEscaping) return;
        StartCoroutine(ElevatorRoutine());
    }

    IEnumerator ElevatorRoutine()
    {
        isEscaping = true;
        Debug.Log("<color=cyan>LIFT: Giovanni bježi liftom!</color>");

        // 1. PRONALAZIMO GIOVANNIJA I ZAMRZAVAMO MU KRETANJE (Ali ostavljamo rotaciju i svjetlo!)
        giovanni = FindFirstObjectByType<GiovanniController>();
        if (giovanni != null)
        {
            // Oduzimamo mu brzinu kretanja, ali ostavljamo isControlled = true
            // tako da i dalje može pomicati miša i paliti bateriju!
            giovanni.moveSpeed = 0f;
            giovanni.sprintMultiplier = 0f;
        }

        // 2. KRATKA PAUZA PRIJE KRETANJA
        yield return new WaitForSeconds(pauzaPrijeKretanja);

        if (elevatorAudio != null && elevatorStartSound != null)
        {
            elevatorAudio.PlayOneShot(elevatorStartSound);
            if (elevatorMovingSound != null)
            {
                elevatorAudio.clip = elevatorMovingSound;
                elevatorAudio.loop = true;
                elevatorAudio.PlayDelayed(elevatorStartSound.length);
            }
        }

        // 3. LIFT SE DIŽE (I GIOVANNI S NJIM)
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, visinaDizanja, 0);

        // Ako je Giovanni u liftu, moramo ga učiniti djetetom lifta kako bi se dizao s njim
        if (giovanni != null)
        {
            giovanni.transform.SetParent(this.transform);
        }

        while (elapsed < trajanjeVoznje)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / trajanjeVoznje;

            // Glatko dizanje lifta
            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        transform.position = endPos;

        if (elevatorAudio != null) elevatorAudio.Stop();

        // 4. POBJEDA! (Zovemo GameManager)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinCurrentCharacter();
        }
    }
}