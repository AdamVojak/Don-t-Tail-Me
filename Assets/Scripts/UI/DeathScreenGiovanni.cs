using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenGiovanni : MonoBehaviour
{
    [Header("Glavni Canvas")]
    public GameObject deathCanvas;

    [Header("UI Elementi")]
    public Image bombUI;      // 0
    public Image viperFishUI; // 1

    public Animator tvStaticAnimator;

    [Header("Završni Ekran (Gumbi i Kursor)")]
    public GameObject buttonsContainer;

    [Header("Postavke Fade-a")]
    public float fadeDuration = 2f;

    [Range(0f, 1f)]
    [Tooltip("0 = potpuno prozirno, 1 = potpuno neprozirno. 0.8 znači 20% prozirno.")]
    public float maxAlpha = 0.8f;

    private void Start()
    {
        if (bombUI != null) bombUI.color = new Color(1, 1, 1, 0);
        if (viperFishUI != null) viperFishUI.color = new Color(1, 1, 1, 0);

        if (buttonsContainer != null) buttonsContainer.SetActive(false);
    }

    public void ShowDeathScreen(int cause)
    {

        if (cause == 0 && bombUI != null)
        {
            StartCoroutine(FadeIn(bombUI));
        }
        else if (cause == 1 && viperFishUI != null)
        {
            StartCoroutine(FadeIn(viperFishUI));
        }
    }

    private IEnumerator FadeIn(Image uiElement)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Clamp01(elapsed / fadeDuration) * maxAlpha;
            uiElement.color = new Color(1, 1, 1, currentAlpha);
            yield return null;
        }

        uiElement.color = new Color(1, 1, 1, maxAlpha);

        if (tvStaticAnimator != null)
        {
            tvStaticAnimator.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        CursorManager cursorManager = FindFirstObjectByType<CursorManager>();
        if (cursorManager != null)
        {
            cursorManager.ActivateDeathCursor();
        }

        if (buttonsContainer != null) buttonsContainer.SetActive(true);
    }
}