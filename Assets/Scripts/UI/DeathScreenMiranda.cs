using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenMiranda : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;
    public Image tvStaticBackground; // NOVO: Slika TV statice

    [Header("UI Slika Uzroka")]
    public Image deathCauseUI;

    [Header("Završni Ekran (Gumbi)")]
    public GameObject buttonsContainer;

    [Header("TV Static Animator (Opcionalno)")]
    public Animator tvStaticAnimator;

    [Header("Postavke Fade-a")]
    public float fadeDuration = 2f;

    [Range(0f, 1f)]
    public float maxAlpha = 0.8f;

    private void Start()
    {
        if (tvStaticBackground != null) tvStaticBackground.color = Color.white;

        if (deathCauseUI != null) deathCauseUI.color = new Color(1, 1, 1, 0);

        if (buttonsContainer != null) buttonsContainer.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        if (deathCanvas != null) deathCanvas.SetActive(true);

        if (tvStaticBackground != null)
        {
            tvStaticBackground.gameObject.SetActive(true);

            Color c = tvStaticBackground.color;
            c.a = 1f;
            tvStaticBackground.color = c;
        }

        if (tvStaticAnimator != null)
        {
            tvStaticAnimator.enabled = true;
        }

        if (deathCauseUI != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        deathCauseUI.gameObject.SetActive(true);

        float elapsed = 0f;
        Color startColor = deathCauseUI.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Clamp01(elapsed / fadeDuration) * maxAlpha;
            deathCauseUI.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);
            yield return null;
        }

        deathCauseUI.color = new Color(startColor.r, startColor.g, startColor.b, maxAlpha);

        yield return new WaitForSeconds(0.5f);

        CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
        if (cursorManager != null)
        {
            cursorManager.ActivateDeathCursor();
        }

        // 5. Palimo gumbe za Restart i Exit
        if (buttonsContainer != null)
        {
            buttonsContainer.SetActive(true);
        }
    }
}