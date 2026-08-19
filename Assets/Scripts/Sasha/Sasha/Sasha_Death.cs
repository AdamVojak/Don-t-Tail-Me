using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Sasha_Death : MonoBehaviour
{
    public Transform glava;
    public Transform kaciga;

    public float brzinaAnimacije = 2f;
    public float udaljenostGlave = 0.5f;
    public float udaljenostKacige = 0.8f;

    private Vector3 pocetnaPozicija;
    private Vector3 ciljGlave;
    private Vector3 ciljKacige;
    private float timer = 0f;

    // NOVO: Varijabla koja prima informaciju što ga je ubilo
    [HideInInspector] public int uzrokSmrti;

    void Start()
    {
        Vector3 smjer = -transform.up;

        ciljGlave = glava.position + (smjer * udaljenostGlave);
        ciljKacige = kaciga.position + (smjer * udaljenostKacige);

        pocetnaPozicija = transform.position;

        // NOVO: Automatski pronalazi DeathScreen u sceni (čak i ako je ugašen) i pokreće ga
        DeathScreenSasha deathScreen = FindFirstObjectByType<DeathScreenSasha>(FindObjectsInactive.Include);
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen(uzrokSmrti);
        }
        else
        {
            Debug.LogWarning("Nije pronađen DeathScreenSasha u sceni!");
        }
    }

    void Update()
    {
        if (timer < 1f)
        {
            timer += Time.deltaTime * brzinaAnimacije;

            glava.position = Vector3.Lerp(glava.position, ciljGlave, timer);
            kaciga.position = Vector3.Lerp(kaciga.position, ciljKacige, timer);
        }
    }
}