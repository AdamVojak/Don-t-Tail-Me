using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenSasha : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Tooltip("Slika pozadine (TV Static) koja se prva pojavljuje")]
    public Image tvStaticBackground;

    [Header("UI Elementi Uzroka")]
    public Image wormUI;
    public Image fistUI;


    [Header("Završni Ekran (Gumbi i Kursor)")]
    public GameObject buttonsContainer; 

    [Header("Postavke Vremena i Fade-a")]
    public float delayBeforeScreen = 1f;       // Pauza prije nego se išta dogodi
    public float backgroundFadeDuration = 1f;  // Koliko dugo se pojavljuje TV static
    public float causeFadeDuration = 2f;       // Koliko dugo se pojavljuje slika uzroka smrti

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f; // Prozirnost za sliku uzroka (0.8 = 20% prozirno)

    private void Start()
    {
        if (tvStaticBackground != null) tvStaticBackground.color = new Color(1, 1, 1, 0);
        if (wormUI != null) wormUI.color = new Color(1, 1, 1, 0);
        if (fistUI != null) fistUI.color = new Color(1, 1, 1, 0);

        if (buttonsContainer != null) buttonsContainer.SetActive(false);
    }

    public void ShowDeathScreen(int cause)
    {
        // Obavezno upalimo Canvas odmah kako bi Coroutine mogao raditi, 
        // ali se ništa neće vidjeti jer su slike prozirne.
        if (deathCanvas != null) deathCanvas.SetActive(true);

        // Pokrećemo sekvencu
        StartCoroutine(DeathSequence(cause));
    }

    private IEnumerator DeathSequence(int cause)
    {
        yield return new WaitForSeconds(delayBeforeScreen);

        if (tvStaticBackground != null)
        {
            yield return StartCoroutine(FadeInElement(tvStaticBackground, backgroundFadeDuration, 1f));
        }

        if (cause == 0 && wormUI != null)
        {
            yield return StartCoroutine(FadeInElement(wormUI, causeFadeDuration, maxAlphaCause));
        }
        else if (cause == 1 && fistUI != null)
        {
            yield return StartCoroutine(FadeInElement(fistUI, causeFadeDuration, maxAlphaCause));
        }

        yield return new WaitForSeconds(0.5f);

        CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
        if (cursorManager != null)
        {
            cursorManager.ActivateDeathCursor();
        }

        if (buttonsContainer != null) buttonsContainer.SetActive(true);
    }

    // Univerzalna metoda za Fade-in
    private IEnumerator FadeInElement(Image uiElement, float duration, float targetAlpha)
    {
        // OSIGURAČ 1: Obavezno upali objekt ako je slučajno ugašen u Inspectoru
        uiElement.gameObject.SetActive(true);

        // OSIGURAČ 2: Gurni ovu sliku na sam vrh (da ju TV static ne može prekriti)
        uiElement.transform.SetAsLastSibling();

        float elapsed = 0f;

        // Čuvamo originalnu boju slike (u slučaju da nije čisto bijela)
        Color startColor = uiElement.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Clamp01(elapsed / duration) * targetAlpha;

            // Mijenjamo samo prozirnost (Alpha), ostavljamo originalne RGB boje
            uiElement.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);
            yield return null;
        }

        // Osiguravamo točnu prozirnost na kraju
        uiElement.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}