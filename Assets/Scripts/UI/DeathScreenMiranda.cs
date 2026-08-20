using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenMiranda : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Header("TV Static Pozadina")]
    public Image tvStaticBackground;
    public Animator tvStaticAnimator; // Podrška za animator ako ga imaš

    [Header("UI Slika Uzroka")]
    public Image deathCauseUI;

    [Header("Završni Ekran (Gumbi i Kursor)")]
    public GameObject buttonsContainer;

    [Header("Postavke Fade-a")]
    public float causeFadeDuration = 2f;

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f;

    public void ShowDeathScreen()
    {
        // 1. STATIC ODMAH PALIMO (Alpha = 1, Image i Animator = true)
        if (tvStaticBackground != null)
        {
            SetAlpha(tvStaticBackground, 1f);
            tvStaticBackground.enabled = true;
        }
        if (tvStaticAnimator != null) tvStaticAnimator.enabled = true;

        // 2. UZROK I GUMBE GASIMO NA POČETKU
        if (deathCauseUI != null) deathCauseUI.enabled = false;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);

        // 3. Palimo Canvas (Static se odmah vidi)
        if (deathCanvas != null) deathCanvas.SetActive(true);

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 1. KORAK: Palimo sliku uzroka smrti, stavljamo prozirnost na 0 i radimo fade-in
        if (deathCauseUI != null)
        {
            deathCauseUI.gameObject.SetActive(true);
            SetAlpha(deathCauseUI, 0f);
            deathCauseUI.enabled = true;

            yield return StartCoroutine(FadeIn(deathCauseUI, causeFadeDuration, maxAlphaCause));
        }

        // 2. KORAK: Čekamo pola sekunde, palimo kursor i gumbe
        yield return new WaitForSeconds(0.5f);

        CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
        if (cursorManager != null)
        {
            cursorManager.ActivateDeathCursor();
        }

        if (buttonsContainer != null)
        {
            buttonsContainer.SetActive(true);
        }
    }

    private IEnumerator FadeIn(Image target, float duration, float targetAlpha)
    {
        float elapsed = 0f;
        Color c = target.color;

        // Osiguranje da slika nije ostala crna u memoriji
        if (c.r == 0 && c.g == 0 && c.b == 0) c = Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration) * targetAlpha;
            target.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        target.color = new Color(c.r, c.g, c.b, targetAlpha);
    }

    private void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}