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
    public Image wormUI; // 0 = Ubio ga je Worm
    public Image fistUI; // 1 = Ubio ga je Fist

    [Header("Postavke Vremena i Fade-a")]
    public float delayBeforeScreen = 5f;       // Pauza prije nego se išta dogodi
    public float backgroundFadeDuration = 1f;  // Koliko dugo se pojavljuje TV static
    public float causeFadeDuration = 2f;       // Koliko dugo se pojavljuje slika uzroka smrti

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f; // Prozirnost za sliku uzroka (0.8 = 20% prozirno)

    private void Start()
    {
        // Na početku sve slike stavljamo na potpuno prozirno (nevidljivo)
        if (tvStaticBackground != null) tvStaticBackground.color = new Color(1, 1, 1, 0);
        if (wormUI != null) wormUI.color = new Color(1, 1, 1, 0);
        if (fistUI != null) fistUI.color = new Color(1, 1, 1, 0);
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
        // 1. KORAK: Čekamo 5 sekundi u mraku
        yield return new WaitForSeconds(delayBeforeScreen);

        // 2. KORAK: Fade-in TV Static pozadine (pojavljuje se do 100% vidljivosti, tj. 1f)
        if (tvStaticBackground != null)
        {
            // yield return znači "čekaj da ovaj fade-in završi prije nego kreneš dalje"
            yield return StartCoroutine(FadeInElement(tvStaticBackground, backgroundFadeDuration, 1f));
        }

        // 3. KORAK: Fade-in slike uzroka smrti (pojavljuje se do maxAlphaCause)
        if (cause == 0 && wormUI != null)
        {
            yield return StartCoroutine(FadeInElement(wormUI, causeFadeDuration, maxAlphaCause));
        }
        else if (cause == 1 && fistUI != null)
        {
            yield return StartCoroutine(FadeInElement(fistUI, causeFadeDuration, maxAlphaCause));
        }
    }

    // Univerzalna metoda za Fade-in bilo koje slike
    private IEnumerator FadeInElement(Image uiElement, float duration, float targetAlpha)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Clamp01(elapsed / duration) * targetAlpha;
            uiElement.color = new Color(1, 1, 1, currentAlpha);
            yield return null;
        }

        // Osiguravamo točnu prozirnost na kraju
        uiElement.color = new Color(1, 1, 1, targetAlpha);
    }
}