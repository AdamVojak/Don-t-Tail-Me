using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenSasha : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Tooltip("Slika pozadine (TV Static)")]
    public Image tvStaticBackground;

    [Header("UI Elementi Uzroka")]
    public Image wormUI;
    public Image fistUI;

    [Header("Završni Ekran (Gumbi i Kursor)")]
    public GameObject buttonsContainer;

    [Header("Postavke Vremena i Fade-a")]
    public float delayBeforeScreen = 2f;       // Pauza u potpunom mraku
    public float backgroundFadeDuration = 1f;  // Koliko dugo se pojavljuje TV static
    public float causeFadeDuration = 2f;       // Koliko dugo se pojavljuje slika uzroka smrti

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f;

    private void Start()
    {
        // Na početku igre sve gasimo
        if (tvStaticBackground != null) tvStaticBackground.gameObject.SetActive(false);
        if (wormUI != null) wormUI.gameObject.SetActive(false);
        if (fistUI != null) fistUI.gameObject.SetActive(false);
        if (buttonsContainer != null) buttonsContainer.SetActive(false);
    }

    public void ShowDeathScreen(int cause)
    {
        // 1. FIZIČKI GASIMO sve slike prije nego upalimo Canvas
        if (tvStaticBackground != null) tvStaticBackground.gameObject.SetActive(false);
        if (wormUI != null) wormUI.gameObject.SetActive(false);
        if (fistUI != null) fistUI.gameObject.SetActive(false);
        if (buttonsContainer != null) buttonsContainer.SetActive(false);

        // 2. Upalimo samo Canvas
        if (deathCanvas != null) deathCanvas.SetActive(true);

        StartCoroutine(DeathSequence(cause));
    }

    private IEnumerator DeathSequence(int cause)
    {
        // 1. KORAK: Čekamo 2 sekunde (Sve je UGAŠENO, nemoguće je da se išta vidi!)
        yield return new WaitForSeconds(delayBeforeScreen);

        // 2. KORAK: Palimo TV Static i pokrećemo njegov fade-in od nule
        if (tvStaticBackground != null)
        {
            tvStaticBackground.gameObject.SetActive(true);
            tvStaticBackground.color = new Color(1, 1, 1, 0); // Kreni od 0
            yield return StartCoroutine(FadeInElement(tvStaticBackground, backgroundFadeDuration, 1f));
        }

        // 3. KORAK: Palimo sliku uzroka smrti i pokrećemo fade-in
        if (cause == 0 && wormUI != null)
        {
            wormUI.gameObject.SetActive(true);
            wormUI.color = new Color(1, 1, 1, 0);
            yield return StartCoroutine(FadeInElement(wormUI, causeFadeDuration, maxAlphaCause));
        }
        else if (cause == 1 && fistUI != null)
        {
            fistUI.gameObject.SetActive(true);
            fistUI.color = new Color(1, 1, 1, 0);
            yield return StartCoroutine(FadeInElement(fistUI, causeFadeDuration, maxAlphaCause));
        }

        // 4. KORAK: Čekamo pola sekunde, palimo kursor i gumbe
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
        float elapsed = 0f;
        Color startColor = uiElement.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Clamp01(elapsed / duration) * targetAlpha;
            uiElement.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);
            yield return null;
        }

        uiElement.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}