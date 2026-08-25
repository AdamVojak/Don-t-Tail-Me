using System.Collections;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject LoadingUI;

    public GameObject Bubbles;
    public GameObject blackPanel;
    public GameObject loadingSpinner;
    public float spinnerSpeed = -200f;

    [Header("Mjehurići Reference")]
    public BubbleEmitter bubbleEmitter; // Naš UI Emitter na novom Canvasu
    public float razlikaVisine = 24f;

    [Header("Postavke Animacije Zida")]
    public float vrijemeZid = 0.8f;
    public float wallOffscreenY = 1080f;
    public float wallOnscreenY = 0f;

    private RectTransform wallRect;
    private RectTransform emitterRect; 
    [SerializeField] private LoadingScreenAudio loadingAudio;

    private void Awake()
    {
        if (loadingAudio == null) loadingAudio = GetComponent<LoadingScreenAudio>();

        if (blackPanel != null) wallRect = blackPanel.GetComponent<RectTransform>();
        if (bubbleEmitter != null) emitterRect = bubbleEmitter.GetComponent<RectTransform>();
        LoadingUI.SetActive(true);
        Bubbles.SetActive(true);
    }

    private void Start()
    {
        HideAll();
    }

    private void Update()
    {
        if (loadingSpinner != null && loadingSpinner.activeSelf)
        {
            loadingSpinner.transform.Rotate(0f, 0f, spinnerSpeed * Time.deltaTime);
        }
    }

    public void SnapWallDown()
    {
        if (blackPanel != null) blackPanel.SetActive(true);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        if (bubbleEmitter != null) bubbleEmitter.Stop();

        if (wallRect == null && blackPanel != null)
            wallRect = blackPanel.GetComponent<RectTransform>();

        if (wallRect != null)
            wallRect.anchoredPosition = new Vector2(0, wallOnscreenY);
    }

    public IEnumerator DropWallRoutine(float duration)
    {
        if (blackPanel != null) blackPanel.SetActive(true);
        if (bubbleEmitter != null) bubbleEmitter.Play();
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        if (loadingAudio != null) loadingAudio.PlayTransitionBubbles();

        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, wallOffscreenY);
        Vector2 endPos = new Vector2(0, wallOnscreenY);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, elapsed / duration);

            // Pomičemo OBA objekta istovremeno!
            if (wallRect != null) wallRect.anchoredPosition = currentPos;
            if (emitterRect != null) emitterRect.anchoredPosition = currentPos - new Vector2(0, razlikaVisine);

            yield return null;
        }

        if (wallRect != null) wallRect.anchoredPosition = endPos;
        if (emitterRect != null) emitterRect.anchoredPosition = endPos - new Vector2(0, razlikaVisine);

        if (bubbleEmitter != null) bubbleEmitter.Stop();
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
    }

    public IEnumerator RaiseWallRoutine(float duration)
    {
        if (bubbleEmitter != null) bubbleEmitter.Play();
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        if (loadingAudio != null) loadingAudio.PlayTransitionBubbles();

        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, wallOnscreenY);
        Vector2 endPos = new Vector2(0, wallOffscreenY);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, elapsed / duration);

            if (wallRect != null) wallRect.anchoredPosition = currentPos;
            if (emitterRect != null) emitterRect.anchoredPosition = currentPos - new Vector2(0, razlikaVisine);

            yield return null;
        }

        if (wallRect != null) wallRect.anchoredPosition = endPos;
        if (emitterRect != null) emitterRect.anchoredPosition = endPos - new Vector2(0, razlikaVisine);

        // 2. KADA ZID IZAĐE S EKRANA, GASIMO SVE (HideAll automatski gasi i mjehuriće)
        HideAll();
    }

    public void HideAll()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        if (bubbleEmitter != null) bubbleEmitter.Stop();

        Vector2 offscreenPos = new Vector2(0, wallOffscreenY);

        if (blackPanel != null)
        {
            if (wallRect == null) wallRect = blackPanel.GetComponent<RectTransform>();
            if (wallRect != null) wallRect.anchoredPosition = offscreenPos;
            blackPanel.SetActive(false);
        }

        if (bubbleEmitter != null)
        {
            if (emitterRect == null) emitterRect = bubbleEmitter.GetComponent<RectTransform>();
            if (emitterRect != null) emitterRect.anchoredPosition = offscreenPos;
        }
    }
}