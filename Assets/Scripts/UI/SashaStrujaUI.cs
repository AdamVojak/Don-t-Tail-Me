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
        // Osigurač
        if (EnergyManager.Instance == null || uiSlika == null || slicice.Length == 0) return;

        // 1. FAZA: ZAMKA JOŠ NIJE RIJEŠENA
        // Ako sustav struje još nije preuzeo kontrolu, skrivamo UI sliku i prekidamo kod.
        if (SashaSvjetlaKontroler.Instance != null && !SashaSvjetlaKontroler.Instance.upravljanjeAktivno)
        {
            uiSlika.enabled = false;
            return;
        }

        // 2. FAZA: ZAMKA JE RIJEŠENA (Kratki spoj se dogodio)
        // Od ovog trenutka nadalje, UI slika je UVIJEK upaljena!
        if (!uiSlika.enabled)
        {
            uiSlika.enabled = true;
        }

        // 3. PRIKAZ RAZINE STRUJE (Čak i kada je 0)
        float trenutnaStruja = EnergyManager.Instance.struja;
        float maxStruja = EnergyManager.Instance.maxStruja;

        float max = (maxStruja > 0) ? maxStruja : 100f;
        float postotak = trenutnaStruja / max;

        // Računanje indeksa sličice
        int normalniIndex = Mathf.RoundToInt(postotak * (slicice.Length - 1));
        int obrnutiIndex = (slicice.Length - 1) - normalniIndex;
        int finalniIndex = Mathf.Clamp(obrnutiIndex, 0, slicice.Length - 1);

        // Postavljanje sličice (ako je struja 0, stavit će zadnju sličicu - praznu bateriju)
        uiSlika.sprite = slicice[finalniIndex];
    }
}