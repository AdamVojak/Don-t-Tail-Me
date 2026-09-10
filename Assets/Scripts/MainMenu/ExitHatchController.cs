using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExitHatchController : MonoBehaviour
{
    [Header("Dijaloski Prozor")]
    [SerializeField] private GameObject exitPopup;
    [SerializeField] private Canvas canvas; // Tvoj glavni UI Canvas

    [Header("LIJEVA OPCIJA (0 = Odustani / Iksic)")]
    [SerializeField] private RectTransform leftOptionRect;      // Pravokutnik lijeve opcije (za detekciju misa)
    [SerializeField] private Image leftOptionBackground;        // Sivi okvir / pozadina lijevog gumba

    [Header("DESNA OPCIJA (1 = Izlaz / Kvacica)")]
    [SerializeField] private RectTransform rightOptionRect;     // Pravokutnik desne opcije (za detekciju misa)
    [SerializeField] private Image rightOptionBackground;       // Sivi okvir / pozadina desnog gumba

    [Header("Struktura Otvora (3 Djeteta)")]
    [SerializeField] private RectTransform stok;          // Fiksni stok
    [SerializeField] private RectTransform vrata;         // Pomicna vrata
    [SerializeField] private RectTransform tockaSpajanja; // Tocka oko koje vrata rotiraju

    [Header("Animacija Rotacije Vrata")]
    [SerializeField] private float openAngle = -110f;
    [SerializeField] private float hatchSpeed = 0.4f;
    [SerializeField] private AnimationCurve hatchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Slajd u Crnilo")]
    [SerializeField] private RectTransform decksContainer;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float quitDelay = 1.0f;

    [Header("Zvucni Efekti (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip switchOptionSFX; // Zvuk klika pri promjeni
    [SerializeField] private AudioClip exitSuccessSFX;   // Zvuk uspjeha pri izlazu
    [SerializeField] private AudioClip cancelErrorSFX;   // Error zvuk pri odustajanju
    [SerializeField] private AudioClip hatchOpenSFX;
    [SerializeField] private AudioClip hatchCloseSFX;

    // JEDINI IZVOR ISTINE: 0 = Lijevo (Odustani), 1 = Desno (Izlaz)
    private int selectedIndex = 1;

    private bool isDialogOpen = false;
    private Coroutine hatchCoroutine;
    private Coroutine exitSlideCoroutine;

    private Vector3 initialDoorLocalPos;
    private Quaternion initialDoorLocalRot;
    private Camera uiCamera;

    private Vector2 lastMousePos;

    public bool IsDialogOpen => isDialogOpen;

    private void Awake()
    {
        if (vrata != null)
        {
            initialDoorLocalPos = vrata.localPosition;
            initialDoorLocalRot = vrata.localRotation;
        }

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        // Ako je Canvas Screen Space - Overlay, uiCamera je null (ispravno za Unity)
        uiCamera = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas?.worldCamera;
    }

    private void Start()
    {
        if (exitPopup != null) exitPopup.SetActive(false);
        ApplyDoorRotation(0f);
    }

    private void Update()
    {
        if (!isDialogOpen) return;

        Vector2 currentMousePos = Input.mousePosition;
        bool hasMouseMoved = (currentMousePos - lastMousePos).sqrMagnitude > 2.0f;

        // 1. TIPKOVNICA
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SetSelection(0);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SetSelection(1);
        }

        // 2. MIŠ (AKO SE MIČE)
        if (hasMouseMoved)
        {
            if (leftOptionRect != null && RectTransformUtility.RectangleContainsScreenPoint(leftOptionRect, currentMousePos, uiCamera))
            {
                SetSelection(0);
            }
            else if (rightOptionRect != null && RectTransformUtility.RectangleContainsScreenPoint(rightOptionRect, currentMousePos, uiCamera))
            {
                SetSelection(1);
            }

            lastMousePos = currentMousePos;
        }

        // 3. POTVRDA (ENTER / SPACE / LIJEVI KLIK)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ExecuteCurrentSelection();
            return;
        }

        // (OVDJE VIŠE NEMA ESCAPE PROVJERE - SVE VODI SUBMARINE NAVIGATOR)
    }


    #region Upravljanje Selekcijom

    public void TriggerExitSequence()
    {
        if (isDialogOpen) return;

        isDialogOpen = true;
        if (exitPopup != null) exitPopup.SetActive(true);

        // Resetiramo poziciju miša pri otvaranju
        lastMousePos = Input.mousePosition;

        // DEFAULT: Početna opcija
        selectedIndex = 1;
        UpdateVisuals(playAudio: false);

        PlaySound(hatchOpenSFX);
        AnimateHatch(openAngle);
    }

    private void SetSelection(int newIndex)
    {
        if (selectedIndex == newIndex) return;

        selectedIndex = newIndex;
        UpdateVisuals(playAudio: true);
    }

    private void UpdateVisuals(bool playAudio)
    {
        // 0 = Lijevo upaljeno, Desno ugaseno
        // 1 = Desno upaljeno, Lijevo ugaseno
        if (leftOptionBackground != null) leftOptionBackground.enabled = (selectedIndex == 0);
        if (rightOptionBackground != null) rightOptionBackground.enabled = (selectedIndex == 1);

        if (playAudio)
        {
            PlaySound(switchOptionSFX);
        }
    }
private void ExecuteCurrentSelection()
    {
        // LOGIKE SU ZAMIJENJENE:
        if (selectedIndex == 1)
        {
            CancelAndReturn(); // Vraća na meni / odustaje
        }
        else
        {
            ConfirmQuit();     // Pokreće izlaz iz igre u crnilo
        }
}

    #endregion

    #region Ishodi (Izlaz ili Povratak)

    public void ConfirmQuit()
    {
        isDialogOpen = false;
        PlaySound(exitSuccessSFX);

        if (exitPopup != null) exitPopup.SetActive(false);

        if (exitSlideCoroutine != null) StopCoroutine(exitSlideCoroutine);
        exitSlideCoroutine = StartCoroutine(ExitToBlackRoutine());
    }

    private void CancelAndReturn()
    {
        isDialogOpen = false;
        PlaySound(cancelErrorSFX);

        if (exitPopup != null) exitPopup.SetActive(false);

        PlaySound(hatchCloseSFX);
        AnimateHatch(0f);
    }

    #endregion

    #region Fizicka Rotacija Vrata

    private void AnimateHatch(float targetAngle)
    {
        if (vrata == null || tockaSpajanja == null) return;
        if (hatchCoroutine != null) StopCoroutine(hatchCoroutine);
        hatchCoroutine = StartCoroutine(HatchRotateRoutine(targetAngle));
    }

    private IEnumerator HatchRotateRoutine(float targetAngle)
    {
        float time = 0f;
        float startAngle = (targetAngle == 0f) ? openAngle : 0f;

        while (time < hatchSpeed)
        {
            time += Time.deltaTime;
            float t = hatchCurve.Evaluate(time / hatchSpeed);
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);

            ApplyDoorRotation(currentAngle);
            yield return null;
        }

        ApplyDoorRotation(targetAngle);
    }

    private void ApplyDoorRotation(float angle)
    {
        if (vrata == null || tockaSpajanja == null) return;

        vrata.localPosition = initialDoorLocalPos;
        vrata.localRotation = initialDoorLocalRot;
        vrata.RotateAround(tockaSpajanja.position, Vector3.forward, angle);
    }

    private IEnumerator ExitToBlackRoutine()
    {
        float screenHeight = canvasRect != null ? canvasRect.rect.height : 1080f;
        Vector2 startPos = decksContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(0, -screenHeight);
        float time = 0f;
        float duration = 0.75f;

        while (time < duration)
        {
            time += Time.deltaTime;
            decksContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / duration);
            yield return null;
        }

        decksContainer.anchoredPosition = targetPos;
        yield return new WaitForSeconds(quitDelay);

        Debug.Log("Gasim igru...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    #endregion
}