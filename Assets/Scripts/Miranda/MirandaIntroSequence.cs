using System.Collections;
using UnityEngine;

public class MirandaIntroSequence : MonoBehaviour
{
    [Header("Kretanje")]
    public Transform pointB;
    public float rollSpeed = 6f;
    public float rollRotationMultiplier = 60f;

    [Header("Svijetlo")]
    public Light mirandaLight;
    public float pocetniIntenzitet = 1f;
    private float ciljniIntenzitet;

    [Header("Povezana Ulazna Vrata")]
    public UlaznaVrata ulaznaVrata;

    private MirandaController mirandaController;
    private CharacterController characterController;
    private float currentRotation = 0f;

    [Header("Audio")]
    public float kutZvuka = 180f;
    private MirandaAudio mirandaAudio;
    private float accumulatedRotation = 0f;

    [HideInInspector] public bool hasPlayedIntro = false;

    void Awake()
    {
        mirandaController = GetComponent<MirandaController>();
        characterController = GetComponent<CharacterController>();
        mirandaAudio = GetComponent<MirandaAudio>();

        if (mirandaLight != null)
        {
            ciljniIntenzitet = mirandaLight.intensity;
        }
    }

    public void PokreniIntro()
    {
        if (hasPlayedIntro) return;
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        hasPlayedIntro = true;

        // 1. PRIVREMENO GASIMO MirandaController da se skripte ne bi tukle oko rotacije!
        if (mirandaController != null)
        {
            mirandaController.isControlled = false;
            mirandaController.enabled = false; // <--- OVO SPJEČAVA SUKOB!
        }

        if (mirandaLight != null) mirandaLight.intensity = pocetniIntenzitet;

        yield return new WaitForSeconds(0.2f);

        // 2. Automatsko rolanje do točke B s glatkim usporavanjem
        if (pointB != null && characterController != null)
        {
            float ukupnaUdaljenostZ = Mathf.Abs(pointB.position.z - transform.position.z);
            float pocetniSmjerZ = Mathf.Sign(pointB.position.z - transform.position.z);
            float zonaUsporavanja = 2.0f; // Na 2 metra prije točke B počinje lagano kočiti

            while (true)
            {
                float preostalaUdaljenost = Mathf.Abs(pointB.position.z - transform.position.z);
                float trenutniSmjerZ = Mathf.Sign(pointB.position.z - transform.position.z);

                // OSIGURAČ: Ako je došla dovoljno blizu (0.3m) ILI ako je slučajno prešla točku -> ODMAH STANI!
                if (preostalaUdaljenost <= 0.3f || trenutniSmjerZ != pocetniSmjerZ)
                {
                    break;
                }

                // GLATKO USPORAVANJE (Što je bliže točki B, to se sporije kotrlja):
                float trenutnaBrzina = rollSpeed;
                if (preostalaUdaljenost < zonaUsporavanja)
                {
                    // Usporava s pune brzine na lagano kotrljanje (1.5f)
                    trenutnaBrzina = Mathf.Lerp(1.5f, rollSpeed, preostalaUdaljenost / zonaUsporavanja);
                }

                float moveStepZ = pocetniSmjerZ * trenutnaBrzina;

                Vector3 move = new Vector3(0, -9.81f, moveStepZ);
                characterController.Move(move * Time.deltaTime);

                // Rotacija tijela prati trenutnu (smanjenu) brzinu
                if (mirandaController != null && mirandaController.mirandaTijelo != null)
                {
                    float rotDelta = moveStepZ * rollRotationMultiplier * Time.deltaTime;
                    currentRotation -= rotDelta;
                    mirandaController.mirandaTijelo.localRotation = Quaternion.Euler(0, -90, currentRotation);

                    // ZVUK KOTAČA (Whoosh zvuk svakih 180 stupnjeva):
                    accumulatedRotation += Mathf.Abs(rotDelta);

                    if (accumulatedRotation >= kutZvuka)
                    {
                        accumulatedRotation = 0f; // Resetiramo brojač
                        if (mirandaAudio != null) mirandaAudio.PlayWheelWhoosh(); // Pusti whoosh!
                    }
                }

                yield return null;
            }

            // Kada stane, osiguravamo da je svjetlo na 100%
            if (mirandaLight != null) mirandaLight.intensity = ciljniIntenzitet;
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

        // 4. VRAĆAMO MirandaController NATRAG U ŽIVOT I DAJEMO KONTROLE!
        if (mirandaController != null)
        {
            mirandaController.enabled = true; // <--- PONOVO PALIMO SKRIPTU!
            mirandaController.isControlled = true;
        }
    }
}