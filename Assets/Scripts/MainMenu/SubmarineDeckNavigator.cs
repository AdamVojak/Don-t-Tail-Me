using System.Collections;
using UnityEngine;

public class SubmarineDeckNavigator : MonoBehaviour
{
    [Header("Kontejner i Canvas")]
    [SerializeField] private RectTransform decksContainer;
    [SerializeField] private RectTransform canvasRect;

    [Header("Postavke Paluba")]
    [SerializeField] private int totalDecks = 3;
    [SerializeField] private int startingDeck = 0;

    [Header("Animacija Kretanja")]
    [SerializeField] private float slideDuration = 0.65f;
    [SerializeField] private float emergencyEscapeDuration = 1.0f; // Vrijeme leta na palubu 0 na Escape
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Zasebni Zvukovi Podmornice")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip climbUpSFX;
    [SerializeField] private AudioClip climbDownSFX;

    [Header("Exit Hatch Referenca")]
    [SerializeField] private ExitHatchController exitHatchController;

    private int currentDeckIndex = 0;
    private bool isSliding = false;
    private Coroutine slideCoroutine;

    public int CurrentDeckIndex => currentDeckIndex;
    public bool IsSliding => isSliding;

    private void Awake()
    {
        if (canvasRect == null) canvasRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        currentDeckIndex = startingDeck;
        float screenHeight = canvasRect.rect.height;
        decksContainer.anchoredPosition = new Vector2(0, currentDeckIndex * screenHeight);
    }

    private void Update()
    {
        // --- GLOBALNA ESCAPE LOGIKA ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleGlobalEscape();
            return;
        }

        // Ako je otvoren Exit dijalog, blokiraj ostale tipke
        if (exitHatchController != null && exitHatchController.IsDialogOpen)
            return;

        if (isSliding) return;

        // W / Gore
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ClimbUp();
        }
        // S / Dolje
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ClimbDown();
        }
    }

    #region Escape Flow (Brzi Izlaz)

    private void HandleGlobalEscape()
    {
        if (exitHatchController == null) return;

        // DRUGI ESCAPE: Ako je prozor već otvoren -> Odmah gasi igru!
        if (exitHatchController.IsDialogOpen)
        {
            exitHatchController.ConfirmQuit();
            return;
        }

        // PRVI ESCAPE: Leti na Palubu 0 i otvori prozor
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        StartCoroutine(EscapeToBridgeRoutine());
    }

    private IEnumerator EscapeToBridgeRoutine()
    {
        isSliding = true;

        // Ako nismo bili na palubi 0, sviraj zvuk penjanja i odvezi kontejner gore
        if (currentDeckIndex != 0)
        {
            PlaySound(climbUpSFX);

            Vector2 startPos = decksContainer.anchoredPosition;
            Vector2 targetPos = Vector2.zero; // Paluba 0 je uvijek na Y = 0
            float time = 0f;

            while (time < emergencyEscapeDuration)
            {
                time += Time.deltaTime;
                float t = slideCurve.Evaluate(time / emergencyEscapeDuration);
                decksContainer.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
                yield return null;
            }

            decksContainer.anchoredPosition = targetPos;
            currentDeckIndex = 0;
        }

        isSliding = false;

        // Čim stigne na palubu 0 -> odmah otvara šaht i popup
        exitHatchController.TriggerExitSequence();
    }

    #endregion

    #region Standardna Navigacija

    public void ClimbUp()
    {
        if (currentDeckIndex == 0)
        {
            if (exitHatchController != null)
                exitHatchController.TriggerExitSequence();
        }
        else if (currentDeckIndex > 0)
        {
            GoToDeck(currentDeckIndex - 1);
        }
    }

    public void ClimbDown()
    {
        if (currentDeckIndex < totalDecks - 1)
        {
            GoToDeck(currentDeckIndex + 1);
        }
    }

    public void GoToDeck(int targetDeckIndex)
    {
        if (isSliding) return;
        targetDeckIndex = Mathf.Clamp(targetDeckIndex, 0, totalDecks - 1);

        if (targetDeckIndex != currentDeckIndex)
        {
            bool isGoingUp = targetDeckIndex < currentDeckIndex;
            PlaySound(isGoingUp ? climbUpSFX : climbDownSFX);

            currentDeckIndex = targetDeckIndex;
            float screenHeight = canvasRect.rect.height;
            float targetY = currentDeckIndex * screenHeight;

            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlideRoutine(targetY, slideDuration));
        }
    }

    private IEnumerator SlideRoutine(float targetY, float duration)
    {
        isSliding = true;
        Vector2 startPos = decksContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(0, targetY);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = slideCurve.Evaluate(time / duration);
            decksContainer.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
            yield return null;
        }

        decksContainer.anchoredPosition = targetPos;
        isSliding = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    #endregion
}