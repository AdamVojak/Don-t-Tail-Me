using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenGiovanni : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Tooltip("Slika pozadine (TV Static) koja se odmah naglo pojavljuje")]
    public Image tvStaticBackground;

    [Header("UI Elementi Uzroka")]
    public Image bombUI;      // 0 = Bomba
    public Image viperFishUI; // 1 = Viper riba

    [Header("Završni Ekran (Gumbi i Kursor)")]
    public GameObject buttonsContainer;

    [Header("Postavke Fade-a")]
    public float causeFadeDuration = 2f; // Koliko dugo se pojavljuje slika uzroka smrti

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f;   // Prozirnost za sliku uzroka (0.8 = 20% prozirno)

    private void Start()
    {
        if (tvStaticBackground != null) tvStaticBackground.color = Color.white;

        if (bombUI != null) bombUI.color = new Color(1, 1, 1, 0);
        if (viperFishUI != null) viperFishUI.color = new Color(1, 1, 1, 0);

        if (buttonsContainer != null) buttonsContainer.SetActive(false);
    }

    public void ShowDeathScreen(int cause)
    {
        if (deathCanvas != null) deathCanvas.SetActive(true);

        // Pokrećemo sekvencu
        StartCoroutine(DeathSequence(cause));
    }

    private IEnumerator DeathSequence(int cause)
    {
        // 1. NAGLI (ABRUPT) STATIC: Odmah forsiramo punu bijelu boju i palimo ga bez čekanja
        if (tvStaticBackground != null)
        {
            tvStaticBackground.gameObject.SetActive(true);
            tvStaticBackground.color = Color.white; // 100% vidljivo odmah!
        }

        // 2. FADE-IN UZROKA: Lagano pojavljivanje Bombe ili Vipera (identično kao kod Sashe)
        if (cause == 0 && bombUI != null)
        {
            yield return StartCoroutine(FadeInElement(bombUI, causeFadeDuration, maxAlphaCause));
        }
        else if (cause == 1 && viperFishUI != null)
        {
            yield return StartCoroutine(FadeInElement(viperFishUI, causeFadeDuration, maxAlphaCause));
        }

        // 3. Čekamo pola sekunde
        yield return new WaitForSeconds(0.5f);

        // 4. Palimo Death Kursor preko CursorManagera
        CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
        if (cursorManager != null)
        {
            cursorManager.ActivateDeathCursor();
        }

        // 5. Palimo gumbe
        if (buttonsContainer != null) buttonsContainer.SetActive(true);
    }

    // Univerzalna metoda za Fade-in (1:1 ista kao kod Sashe)
    private IEnumerator FadeInElement(Image uiElement, float duration, float targetAlpha)
    {
        uiElement.gameObject.SetActive(true);

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