using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenGiovanni : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Header("TV Static Pozadina")]
    public Image tvStaticBackground;
    public Animator tvStaticAnimator; // NOVO: Dodan animator

    [Header("UI Elementi Uzroka")]
    public Image bombUI;
    public Image viperFishUI;

    [Header("Završni Ekran (Gumbi)")]
    public GameObject buttonsContainer;

    [Header("Postavke Fade-a")]
    public float causeFadeDuration = 2f;

    [Range(0f, 1f)]
    public float maxAlphaCause = 0.8f;

    [SerializeField] private DeathScreenAudio deathAudio;

    public void ShowDeathScreen(int cause)
    {
        // 1. ZVUČNI UDAR SMRTI (Eksplozija ili Viper Krik + Statika):
        if (deathAudio != null)
        {
            deathAudio.PlayDeathCauseSound(cause); // <-- DODAJ OVU LINIJU!
            deathAudio.StartStatic(1, false);
        }

        // 2. STATIC ODMAH PALIMO (Image i Animator = true, Alpha = 1)
        if (tvStaticBackground != null)
        {
            SetAlpha(tvStaticBackground, 1f);
            tvStaticBackground.enabled = true;
        }
        if (tvStaticAnimator != null) tvStaticAnimator.enabled = true;

        if (deathAudio != null) deathAudio.StartStatic(1, false);

        // 2. UZROKE GASIMO (Image = false)
        if (bombUI != null) bombUI.enabled = false;
        if (viperFishUI != null) viperFishUI.enabled = false;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);

        // 3. Palimo Canvas (Static se odmah vidi)
        if (deathCanvas != null) deathCanvas.SetActive(true);

        StartCoroutine(DeathSequence(cause));
    }

    private IEnumerator DeathSequence(int cause)
    {
        Image targetImage = (cause == 0) ? bombUI : viperFishUI;

        if (targetImage != null)
        {
            targetImage.gameObject.SetActive(true);
            SetAlpha(targetImage, 0f);
            targetImage.enabled = true;

            yield return StartCoroutine(FadeIn(targetImage, causeFadeDuration, maxAlphaCause));
        }

        // ZVUČNA SEKVENCA ZA KURSOR:
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