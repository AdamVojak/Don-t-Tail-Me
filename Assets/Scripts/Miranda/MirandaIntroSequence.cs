using System.Collections;
using UnityEngine;

public class MirandaIntroSequence : MonoBehaviour
{
    [Header("Kretanje")]
    public Transform pointB;
    public float rollSpeed = 6f;
    public float rollRotationMultiplier = 60f;

    [Header("Audio")]
    public float kutZvuka = 180f;
    private MirandaAudio mirandaAudio;
    private float accumulatedRotation = 0f;

    [Header("Povezana Ulazna Vrata")]
    public UlaznaVrata ulaznaVrata;

    private MirandaController mirandaController;
    private CharacterController characterController;

    [HideInInspector] public bool hasPlayedIntro = false;

    void Awake()
    {
        mirandaController = GetComponent<MirandaController>();
        characterController = GetComponent<CharacterController>();
        mirandaAudio = GetComponent<MirandaAudio>();
    }

    public void PokreniIntro()
    {
        if (hasPlayedIntro) return;
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        hasPlayedIntro = true;

        // 1. Privremeno gasimo kontroler
        if (mirandaController != null)
        {
            mirandaController.isControlled = false;
            mirandaController.enabled = false;
        }

        yield return new WaitForSeconds(0.2f);

        // 2. Automatsko rolanje s glatkim kočenjem
        if (pointB != null && characterController != null)
        {
            float pocetniSmjerZ = Mathf.Sign(pointB.position.z - transform.position.z);
            float zonaUsporavanja = 2.0f; // Počinje lagano kočiti 2m prije cilja

            while (true)
            {
                float preostalaUdaljenost = Mathf.Abs(pointB.position.z - transform.position.z);
                float trenutniSmjerZ = Mathf.Sign(pointB.position.z - transform.position.z);

                // Ako je došla blizu (0.3m) ili prešla točku -> STANI
                if (preostalaUdaljenost <= 0.3f || trenutniSmjerZ != pocetniSmjerZ)
                {
                    break;
                }

                // Glatko usporavanje
                float trenutnaBrzina = rollSpeed;
                if (preostalaUdaljenost < zonaUsporavanja)
                {
                    trenutnaBrzina = Mathf.Lerp(1.5f, rollSpeed, preostalaUdaljenost / zonaUsporavanja);
                }

                float moveStepZ = pocetniSmjerZ * trenutnaBrzina;
                Vector3 move = new Vector3(0, -9.81f, moveStepZ);
                characterController.Move(move * Time.deltaTime);

                // ROTACIJA (Direktno ažuriramo MirandaController varijablu!)
                if (mirandaController != null && mirandaController.mirandaTijelo != null)
                {
                    float rotDelta = moveStepZ * rollRotationMultiplier * Time.deltaTime;
                    mirandaController.currentRotation -= rotDelta; // <--- KLJUČNO: Miranda pamti ovaj kut!

                    mirandaController.mirandaTijelo.localRotation = Quaternion.Euler(0, -90, mirandaController.currentRotation);

                    // Audio Whoosh
                    accumulatedRotation += Mathf.Abs(rotDelta);
                    if (accumulatedRotation >= kutZvuka)
                    {
                        accumulatedRotation = 0f;
                        if (mirandaAudio != null) mirandaAudio.PlayWheelWhoosh();
                    }
                }

                yield return null;
            }
        }

        // 3. Zatvaranje ulaznih vrata iza nje
        if (ulaznaVrata != null)
        {
            ulaznaVrata.ZatvoriVrata();

            while (ulaznaVrata.DaLiSeMicu())
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Vraćamo kontroler (Kut tijela ostaje savršeno očuvan!)
        if (mirandaController != null)
        {
            mirandaController.enabled = true;
            mirandaController.isControlled = true;
        }
    }
}