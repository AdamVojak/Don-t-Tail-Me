using UnityEngine;
using UnityEngine.UI;

public class SashaStrujaUI : MonoBehaviour
{
    [Header("Poveznice")]
    public Image uiSlika;

    [Header("Sličice")]
    public Sprite[] slicice;

    void Update()
    {
        // Ako nema EnergyManagera ili UI slike, prekidamo
        if (EnergyManager.Instance == null || uiSlika == null || slicice.Length == 0) return;

        // NOVO: Provjeravamo je li sustav struje uopće preuzeo kontrolu nad igrom (nakon zamke)
        // Ako zamka još nije riješena, skrivamo UI i prekidamo Update.
        if (SashaSvjetlaKontroler.Instance != null && !SashaSvjetlaKontroler.Instance.upravljanjeAktivno)
        {
            uiSlika.enabled = false;
            return;
        }

        // Ako smo prošli gornji uvjet, znači da je zamka riješena i igra ovisi o Mirandi!
        float trenutnaStruja = EnergyManager.Instance.struja;
        float maxStruja = EnergyManager.Instance.maxStruja;

        if (trenutnaStruja <= 0)
        {
            uiSlika.enabled = false; // Skrivamo sliku ako nema struje
        }
        else
        {
            if (!uiSlika.enabled)
            {
                uiSlika.enabled = true; // Palimo sliku ako ima struje
            }

            // LOGIKA ZA SPRITEOVE
            float max = (maxStruja > 0) ? maxStruja : 100f;
            float postotak = trenutnaStruja / max;

            int normalniIndex = Mathf.RoundToInt(postotak * (slicice.Length - 1));
            int obrnutiIndex = (slicice.Length - 1) - normalniIndex;
            int finalniIndex = Mathf.Clamp(obrnutiIndex, 0, slicice.Length - 1);

            uiSlika.sprite = slicice[finalniIndex];
        }
    }
}