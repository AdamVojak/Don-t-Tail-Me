using UnityEngine;

public class MirandaRuka : MonoBehaviour
{
    [Header("Reference - Drugi dio ruke")]
    public Transform shakaObjekt;
    public Transform spojShaka;

    [Header("Reference - Granice izvlačenja")]
    public Transform pocetakShaka;
    public Transform krajShaka;

    [Header("Postavke")]
    public float brzinaIzvlacenja = 15f;
    [Tooltip("Ako ruka ne pokazuje točno u miša, upiši 90, -90 ili 180 ovdje")]
    public float offsetKuta = 0f;

    private Camera glavnaKamera;

    void Start()
    {
        glavnaKamera = Camera.main;
    }

    void Update()
    {
        if (shakaObjekt == null || spojShaka == null || pocetakShaka == null || krajShaka == null) return;

        // 1. PRONALAZAK MIŠA NA Y-Z RAVNINI
        Plane yzRavnina = new Plane(Vector3.right, transform.position);
        Ray zrakaIzKamere = glavnaKamera.ScreenPointToRay(Input.mousePosition);

        float udaljenostDoRavnine;
        Vector3 pozicijaMisaUSvijetu = Vector3.zero;

        if (yzRavnina.Raycast(zrakaIzKamere, out udaljenostDoRavnine))
        {
            pozicijaMisaUSvijetu = zrakaIzKamere.GetPoint(udaljenostDoRavnine);
        }

        // 2. STABILNA ROTACIJA (Sada u pravom smjeru!)
        Vector3 smjer = pozicijaMisaUSvijetu - transform.position;

        float kut = Mathf.Atan2(smjer.y, smjer.z) * Mathf.Rad2Deg;
        kut += offsetKuta;

        // OVDJE JE POPRAVAK: Dodali smo MINUS ispred 'kut' (-kut) da obrnemo smjer rotacije!
        Quaternion fiksniY = Quaternion.Euler(0f, -90f, 0f);
        Quaternion rotacijaOkoX = Quaternion.AngleAxis(-kut, Vector3.right);

        transform.rotation = rotacijaOkoX * fiksniY;

        // 3. TELESKOPSKA LOGIKA SA OFFSETOM
        float udaljenostMisa = Vector3.Distance(transform.position, pozicijaMisaUSvijetu);
        float minUdaljenost = Vector3.Distance(transform.position, pocetakShaka.position);
        float maxUdaljenost = Vector3.Distance(transform.position, krajShaka.position);

        float postotakIzvlacenja = Mathf.InverseLerp(minUdaljenost, maxUdaljenost, udaljenostMisa);

        Vector3 ciljnaPozicijaSpoja = Vector3.Lerp(pocetakShaka.localPosition, krajShaka.localPosition, postotakIzvlacenja);
        Vector3 ciljnaPozicijaShake = ciljnaPozicijaSpoja - spojShaka.localPosition;

        shakaObjekt.localPosition = Vector3.Lerp(shakaObjekt.localPosition, ciljnaPozicijaShake, Time.deltaTime * brzinaIzvlacenja);
    }
}