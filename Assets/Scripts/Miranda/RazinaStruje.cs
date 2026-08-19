using UnityEngine;

public class TreadmillEkran : MonoBehaviour
{
    [Header("Poveznice")]
    public SpriteRenderer ekranRenderer;
    public Light treadmillLight; // Standardno Point Light svjetlo

    [Header("Sličice")]
    public Sprite[] slicice;

    [Header("Postavke Svjetla")]
    public Color bojaCrvena = new Color(1f, 0.2f, 0.2f); // Blago crvena
    public Color bojaZuta = new Color(1f, 0.8f, 0.2f);   // Blago žuta
    public Color bojaZelena = new Color(0.2f, 1f, 0.2f); // Blago zelena
    public float maxIntenzitet = 6f; // Maksimalni multiplier

    void Update()
    {
        // Provjeravamo je li EnergyManager prisutan u sceni
        if (EnergyManager.Instance == null || ekranRenderer == null || slicice.Length == 0) return;

        // Dohvaćamo trenutne podatke iz centralnog EnergyManagera
        float trenutnaStruja = EnergyManager.Instance.struja;
        float maxStruja = EnergyManager.Instance.maxStruja;

        // 1. LOGIKA ZA SPRITEOVE
        float max = (maxStruja > 0) ? maxStruja : 100f;
        float postotak = trenutnaStruja / max;

        int normalniIndex = Mathf.RoundToInt(postotak * (slicice.Length - 1));
        int obrnutiIndex = (slicice.Length - 1) - normalniIndex;
        int finalniIndex = Mathf.Clamp(obrnutiIndex, 0, slicice.Length - 1);

        ekranRenderer.sprite = slicice[finalniIndex];

        // 2. LOGIKA ZA SVJETLO
        if (treadmillLight != null)
        {
            if (trenutnaStruja <= 0)
            {
                // Ako je struja 0, ugasi svjetlo
                treadmillLight.enabled = false;
                treadmillLight.intensity = 0;
            }
            else
            {
                // Upali svjetlo ako je struja iznad 0
                treadmillLight.enabled = true;

                // Postavljanje intenziteta (raste od 0 do maxIntenzitet ovisno o postotku)
                treadmillLight.intensity = postotak * maxIntenzitet;

                // Određivanje boje prema pragovima
                if (trenutnaStruja <= 10f)
                {
                    treadmillLight.color = bojaCrvena;
                }
                else if (trenutnaStruja <= 90f)
                {
                    treadmillLight.color = bojaZuta;
                }
                else // Iznad 90
                {
                    treadmillLight.color = bojaZelena;
                }
            }
        }
    }
}