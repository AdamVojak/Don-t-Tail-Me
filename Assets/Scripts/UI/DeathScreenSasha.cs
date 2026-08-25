using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenSasha : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Header("TV Static Pozadina")]
    public Image tvStaticBackground;
    public Animator tvStaticAnimator; // NOVO: Dodan animator da ga možemo gasiti!

    [Header("UI Elementi Uzroka")]
    public Image wormUI;
    public Image fistUI;

    [Header("Završni Ekran (Gumbi)")]
    public GameObject buttonsContainer;

    [Header("Postavke Vremena i Fade-a")]
    public float delayBeforeScreen = 2f;
    public float backgroundFadeDuration = 1f;
    public float causeFadeDuration = 2f;

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f;

    [SerializeField] private DeathScreenAudio deathAudio;

    public void ShowDeathScreen(int cause)
    {
        // 1. TVOJA IDEJA: Gasimo Image i Animator komponente PRIJE paljenja Canvasa!
        if (tvStaticBackground != null) tvStaticBackground.enabled = false;
        if (tvStaticAnimator != null) tvStaticAnimator.enabled = false;
        if (wormUI != null) wormUI.enabled = false;
        if (fistUI != null) fistUI.enabled = false;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);

        // 2. Sada sigurno palimo Canvas (ništa se ne vidi jer su Image komponente ugašene)
        if (deathCanvas != null) deathCanvas.SetActive(true);

        StartCoroutine(DeathSequence(cause));
    }

    private IEnumerator DeathSequence(int cause)
    {
        // 1. Čekamo u mraku
        yield return new WaitForSeconds(delayBeforeScreen);

        // 2. Palimo TV Static i radimo fade-in zvuka i slike
        if (tvStaticBackground != null)
        {
            SetAlpha(tvStaticBackground, 0f);
            tvStaticBackground.enabled = true;
            if (tvStaticAnimator != null) tvStaticAnimator.enabled = true;

            // DODAJ OVO (Pali Fade-In statike u trajanju backgroundFadeDuration):
            if (deathAudio != null) deathAudio.StartStatic(0, true, backgroundFadeDuration);

            yield return StartCoroutine(FadeIn(tvStaticBackground, backgroundFadeDuration, 1f));
        }

        // 3. Palimo Image uzroka i radimo fade-in
        Image causeImage = (cause == 0) ? wormUI : fistUI;
        if (causeImage != null)
        {
            causeImage.gameObject.SetActive(true);
            SetAlpha(causeImage, 0f);
            causeImage.enabled = true;

            yield return StartCoroutine(FadeIn(causeImage, causeFadeDuration, maxAlphaCause));
        }

        // --- ZVUČNI EFEKT PILJENJA I LOMLJENJA KOSTI PRIJE POJAVE RUKE ---
        yield return new WaitForSeconds(0.5f);

        if (deathAudio != null)
        {
            deathAudio.PlayHandSaw();
            yield return new WaitForSeconds(1.0f);
        }

        CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
        if (cursorManager != null)
        {
            deathAudio.PlayBoneCrack();
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