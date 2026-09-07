using System.Collections;
using UnityEngine;

public class SubmarineDeckNavigator : MonoBehaviour
{
    [Header("Kontejner i Canvas")]
    [SerializeField] private RectTransform decksContainer;
    [SerializeField] private RectTransform canvasRect;

    [Header("Postavke Paluba")]
    [Tooltip("Ukupan broj paluba (npr. 3: 0 = Most, 1 = Posada, 2 = Opcije)")]
    [SerializeField] private int totalDecks = 3;
    [SerializeField] private int startingDeck = 0;

    [Header("Animacija Kretanja")]
    [SerializeField] private float slideDuration = 0.65f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Zasebni Zvukovi Podmornice")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Zvuk koji se čuje kada se igrač penje na gornju palubu (W / Strelica Gore)")]
    [SerializeField] private AudioClip climbUpSFX;
    [Tooltip("Zvuk koji se čuje kada se igrač spušta na donju palubu (S / Strelica Dolje)")]
    [SerializeField] private AudioClip climbDownSFX;

    [SerializeField] private float climbVolume = 0.75f;

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
        // STROGA BLOKADA: Ako je otvoren Exit dijalog, NE DOPUSTI nikakvo kretanje paluba!
        if (exitHatchController != null && exitHatchController.IsDialogOpen)
            return;

        if (isSliding) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ClimbUp();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ClimbDown();
        }
    }

    #region Navigacija

    public void ClimbUp()
    {
        // Ako smo već na palubi 0 (Most) i stisnemo GORE -> Pokrećemo sekvencu izlaza kroz šaht!
        if (currentDeckIndex == 0)
        {
            if (exitHatchController != null)
            {
                exitHatchController.TriggerExitSequence();
            }
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
            // Provjera smjera: idemo li GORE ili DOLJE?
            bool isGoingUp = targetDeckIndex < currentDeckIndex;

            // Reproduciraj odgovarajući zvuk za taj smjer
            PlaySound(isGoingUp ? climbUpSFX : climbDownSFX, climbVolume);

            currentDeckIndex = targetDeckIndex;

            float screenHeight = canvasRect.rect.height;
            float targetY = currentDeckIndex * screenHeight;

            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlideRoutine(targetY));
        }
    }

    private IEnumerator SlideRoutine(float targetY)
    {
        isSliding = true;
        Vector2 startPos = decksContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(0, targetY);
        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = slideCurve.Evaluate(time / slideDuration);
            decksContainer.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
            yield return null;
        }

        decksContainer.anchoredPosition = targetPos;
        isSliding = false;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.volume = volume;
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion
}