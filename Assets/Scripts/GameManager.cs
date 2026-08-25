using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum ActiveCharacter { Sasha, Miranda, Giovanni, Odabir }
    public ActiveCharacter currChar;

    [Header("Game States")]
    public bool isLoading = false;

    [Header("Environment")]
    public GameObject sunLight;

    [Header("Giovanni Fog Settings")]
    public Color underwaterColor = new Color(0f, 0.05f, 0.15f); // Tamno modra
    public float fogDensity = 0.04f; // Gustoća magle
    public FogMode fogMode = FogMode.ExponentialSquared; // Najprirodniji mod za vodu

    [Header("Characters")]
    public SashaController sashaScript;
    public MirandaController mirandaScript;
    public GiovanniController giovanniScript;

    public static GameManager Instance;

    [Header("Odabir Likova u Igri")]
    public bool sashaOdabran = true;
    public bool mirandaOdabrana = true;
    public bool giovanniOdabran = true;

    [Header("Managers")]
    public LoadingManager loadingManager;
    [SerializeField] private LoadingScreenAudio loadingAudio;

    [Header("Level Objects (Roditelji terena)")]
    public GameObject sashaLevel;
    public GameObject mirandaLevel;
    public GameObject giovanniLevel;

    [Header("Cameras")]
    public CinemachineCamera sashaCam;
    public CinemachineCamera mirandaCam;
    public CinemachineCamera giovanniCam;

    [Header("UI")]
    public GameObject UI_Sasha;
    public GameObject UI_Miranda;
    public GameObject UI_Giovanni;

    public GameObject kursorSasha;

    [Header("Transition Settings")]
    [SerializeField] private GameObject UI_Tranzicija;
    [SerializeField] private Slider tranzicijskiSlider;
    [SerializeField] private float transitionDuration = 2f; // Vrijeme putovanja slidera
    [SerializeField] private float loadingDuration = 1.0f;    // NOVO: Koliko dugo traje crni ekran s kotačićem

    private bool isTransitioning = false;
    private bool goingForward = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (sunLight != null) sunLight.SetActive(false);

        if (loadingAudio == null && loadingManager != null)
        {
            loadingAudio = loadingManager.GetComponent<LoadingScreenAudio>();
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.reflectionIntensity = 0;

        // 1. AUTOMATSKA DETEKCIJA: Tko je ugašen u Hierarchyju prije pokretanja, NE IGRA!
        if (sashaScript != null && !sashaScript.gameObject.activeSelf) sashaOdabran = false;
        if (mirandaScript != null && !mirandaScript.gameObject.activeSelf) mirandaOdabrana = false;
        if (giovanniScript != null && !giovanniScript.gameObject.activeSelf) giovanniOdabran = false;

        // 2. BRISANJE: Oni koji nisu odabrani se potpuno brišu iz memorije
        if (!sashaOdabran)
        {
            if (sashaLevel != null) Destroy(sashaLevel);
            if (sashaScript != null) Destroy(sashaScript.gameObject);
        }

        if (!mirandaOdabrana)
        {
            if (mirandaLevel != null) Destroy(mirandaLevel);
            if (mirandaScript != null) Destroy(mirandaScript.gameObject);
        }

        if (!giovanniOdabran)
        {
            if (giovanniLevel != null) Destroy(giovanniLevel);
            if (giovanniScript != null) Destroy(giovanniScript.gameObject);
        }
    }

    void Start()
    {
        if (sunLight == null)
        {
            Debug.LogWarning("Sun Light referenca nedostaje u GameManageru!");
        }

        if (sunLight == null) Debug.LogWarning("Sun Light referenca nedostaje u GameManageru!");

        //LockCursor(true);

        isLoading = false;

        if (UI_Tranzicija != null) UI_Tranzicija.SetActive(false);
        if (loadingManager != null) loadingManager.HideAll();

        StartCoroutine(InitialTransitionRoutine());
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isTransitioning)
        {
            if (GetAvailableCharacterCount() > 1)
            {
                SwitchToNextAvailableCharacter();
            }
            else
            {
                Debug.Log("Samo je jedan lik dostupan! Tranzicija otkazana.");
            }
        }

        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }*/
    }


    bool IsCharacterAvailable(ActiveCharacter character)
    {
        switch (character)
        {
            case ActiveCharacter.Sasha:
                return sashaOdabran && sashaScript != null && sashaCam != null && sashaScript.currentState != SashaController.SashaState.Dead;

            case ActiveCharacter.Miranda:
                return mirandaOdabrana && mirandaScript != null && mirandaCam != null && mirandaScript.currentState != MirandaController.MirandaState.Dead;

            case ActiveCharacter.Giovanni:
                return giovanniOdabran && giovanniScript != null && giovanniCam != null && giovanniScript.currentState != GiovanniController.GiovanniState.Dead;

            default:
                return false;
        }
    }

    void SelectFirstAvailableCharacter()
    {
        if (IsCharacterAvailable(ActiveCharacter.Sasha)) SwitchCharacter(ActiveCharacter.Sasha);
        else if (IsCharacterAvailable(ActiveCharacter.Miranda)) SwitchCharacter(ActiveCharacter.Miranda);
        else if (IsCharacterAvailable(ActiveCharacter.Giovanni)) SwitchCharacter(ActiveCharacter.Giovanni);
        else Debug.LogError("Nijedan lik nije dostupan u sceni! Provjerite reference u GameManageru.");
    }


    void SwitchToNextAvailableCharacter()
    {
        ActiveCharacter nextChar = currChar;

        bool tempDirection = goingForward;

        for (int i = 0; i < 3; i++)
        {
            if (nextChar == ActiveCharacter.Sasha) tempDirection = true;
            else if (nextChar == ActiveCharacter.Giovanni) tempDirection = false;

            if (nextChar == ActiveCharacter.Sasha)
            {
                nextChar = ActiveCharacter.Miranda;
            }
            else if (nextChar == ActiveCharacter.Giovanni)
            {
                nextChar = ActiveCharacter.Miranda;
            }
            else if (nextChar == ActiveCharacter.Miranda)
            {
                nextChar = tempDirection ? ActiveCharacter.Giovanni : ActiveCharacter.Sasha;
            }

            if (nextChar != currChar && IsCharacterAvailable(nextChar))
            {
                goingForward = tempDirection;
                StartCoroutine(TransitionToCharacter(nextChar));
                return;
            }
        }
    }

    int GetAvailableCharacterCount()
    {
        int count = 0;
        if (IsCharacterAvailable(ActiveCharacter.Sasha)) count++;
        if (IsCharacterAvailable(ActiveCharacter.Miranda)) count++;
        if (IsCharacterAvailable(ActiveCharacter.Giovanni)) count++;
        return count;
    }


    void SwitchCharacter(ActiveCharacter newCharacter)
    {
        currChar = newCharacter;

        if (sashaScript != null) sashaScript.isControlled = false;
        if (mirandaScript != null) mirandaScript.isControlled = false;
        if (giovanniScript != null) giovanniScript.isControlled = false;

        if (sashaCam != null) sashaCam.Priority = 0;
        if (mirandaCam != null) mirandaCam.Priority = 0;
        if (giovanniCam != null) giovanniCam.Priority = 0;

        if (kursorSasha != null) kursorSasha.SetActive(false);

        if (UI_Sasha != null) UI_Sasha.SetActive(false);
        if (UI_Miranda != null) UI_Miranda.SetActive(false);
        if (UI_Giovanni != null) UI_Giovanni.SetActive(false);


        if (newCharacter == ActiveCharacter.Giovanni)
        {
            ApplyGiovanniEnvironment(true);
        }
        else
        {
            ApplyGiovanniEnvironment(false);
        }


        if (currChar == ActiveCharacter.Sasha && IsCharacterAvailable(ActiveCharacter.Sasha))
        {
            sashaScript.isControlled = true;
            if (kursorSasha != null) kursorSasha.SetActive(true);
            sashaCam.Priority = 10;
            if (UI_Sasha != null) UI_Sasha.SetActive(true);
        }
        else if (currChar == ActiveCharacter.Miranda && IsCharacterAvailable(ActiveCharacter.Miranda))
        {
            mirandaScript.isControlled = true;
            mirandaCam.Priority = 10;
            if (UI_Miranda != null) UI_Miranda.SetActive(true);
        }
        else if (currChar == ActiveCharacter.Giovanni && IsCharacterAvailable(ActiveCharacter.Giovanni))
        {
            giovanniScript.isControlled = true;
            giovanniCam.Priority = 10;
            if (UI_Giovanni != null) UI_Giovanni.SetActive(true);
        }
    }


    IEnumerator TransitionToCharacter(ActiveCharacter targetCharacter)
    {
        isLoading = true;
        isTransitioning = true;

        float startValue = GetCharacterSliderValue(currChar);
        float endValue = GetCharacterSliderValue(targetCharacter);

        currChar = ActiveCharacter.Odabir;

        // --- 1. ZID PADA ---
        if (loadingManager != null)
        {
            yield return StartCoroutine(loadingManager.DropWallRoutine(loadingManager.vrijemeZid));
        }

        // --- 2. ODUZIMAMO KONTROLE I UI ---
        DisableAllControls();
        HideAllCharacterUIs();


        // --- 3. JITTERY FADE-IN (Drhtavo paljenje) ---
        if (UI_Tranzicija != null)
        {
            // DODAJ OVE DVIJE LINIJE (Zvuk paljenja kompjutera + Flicker):
            if (loadingAudio != null) loadingAudio.PlayComputerStartup();
            if (loadingAudio != null) loadingAudio.PlayFlicker();

            UI_Tranzicija.SetActive(true);

            CanvasGroup cg = UI_Tranzicija.GetComponent<CanvasGroup>();
            if (cg == null) cg = UI_Tranzicija.AddComponent<CanvasGroup>();

            float fadeDuration = 0.4f;
            float elapsedFade = 0f;

            while (elapsedFade < fadeDuration)
            {
                elapsedFade += Time.deltaTime;
                float baseAlpha = elapsedFade / fadeDuration;
                float jitter = Random.Range(-0.25f, 0.25f);
                cg.alpha = Mathf.Clamp01(baseAlpha + jitter);
                yield return null;
            }
            cg.alpha = 1f;
        }


        // --- 4. SLIDER ANIMACIJA ---
        float elapsed = 0f;
        int lastClickTick = Mathf.RoundToInt(startValue); // DODAJ OVO (Pamti zadnji klik)

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            if (tranzicijskiSlider != null)
            {
                tranzicijskiSlider.value = Mathf.Lerp(startValue, endValue, t);

                // DODAJ OVO (Brzi klik zvuk dok slider prelazi preko cijelih brojeva):
                int currentTick = Mathf.RoundToInt(tranzicijskiSlider.value);
                if (currentTick != lastClickTick)
                {
                    lastClickTick = currentTick;
                    if (loadingAudio != null) loadingAudio.PlayPlayerSwitchClick();
                }
            }

            yield return null;
        }

        if (tranzicijskiSlider != null) tranzicijskiSlider.value = endValue;

        // DODAJ OVU LINIJU (Goofy akord kada slider stigne na metu!):
        if (loadingAudio != null) loadingAudio.PlayPlayerSelectedChord();

        yield return new WaitForSeconds(0.3f);


        // --- 5. JITTERY FADE-OUT (Drhtavo gašenje) ---
        if (UI_Tranzicija != null)
        {
            // DODAJ OVE DVIJE LINIJE (Zvuk gašenja kompjutera koji prekida paljenje + Flicker):
            if (loadingAudio != null) loadingAudio.PlayComputerShutdown();
            if (loadingAudio != null) loadingAudio.PlayFlicker();

            CanvasGroup cg = UI_Tranzicija.GetComponent<CanvasGroup>();
            float fadeDuration = 0.3f;
            float elapsedFade = 0f;

            while (elapsedFade < fadeDuration)
            {
                elapsedFade += Time.deltaTime;
                float baseAlpha = 1f - (elapsedFade / fadeDuration);
                float jitter = Random.Range(-0.25f, 0.25f);
                cg.alpha = Mathf.Clamp01(baseAlpha + jitter);
                yield return null;
            }

            cg.alpha = 0f;
            UI_Tranzicija.SetActive(false);
            cg.alpha = 1f;
        }


        // --- 6. UČITAVANJE SVEGA IZA ZIDA ---
        ToggleLevels(targetCharacter);
        SetCameraPriorities(targetCharacter);
        FinalizeCharacterSwitch(targetCharacter);

        yield return null;
        yield return new WaitForSeconds(loadingDuration); // Tvoj buffer


        // --- 7. DIŽEMO ZID ---
        if (loadingManager != null)
        {
            yield return StartCoroutine(loadingManager.RaiseWallRoutine(loadingManager.vrijemeZid));
        }

        isTransitioning = false;
        isLoading = false;
    }

    IEnumerator InitialTransitionRoutine()
    {
        isTransitioning = true;

        // 1. Tražimo tko je prvi dostupan lik
        ActiveCharacter targetCharacter = ActiveCharacter.Odabir;
        if (IsCharacterAvailable(ActiveCharacter.Sasha)) targetCharacter = ActiveCharacter.Sasha;
        else if (IsCharacterAvailable(ActiveCharacter.Miranda)) targetCharacter = ActiveCharacter.Miranda;
        else if (IsCharacterAvailable(ActiveCharacter.Giovanni)) targetCharacter = ActiveCharacter.Giovanni;

        if (targetCharacter == ActiveCharacter.Odabir)
        {
            Debug.LogError("Nema dostupnih likova za početak igre!");
            yield break;
        }

        currChar = ActiveCharacter.Odabir;
        DeactivateAllCharacters();

        // 2. INSTANTNO SPUŠTAMO ZID (Crni ekran, bez mjehurića)
        if (loadingManager != null) loadingManager.SnapWallDown();

        isLoading = true;

        // 3. PRAZNINA (0.5 sekundi crnog ekrana prije nego se išta dogodi)
        yield return new WaitForSeconds(1f);


        // --- OVO JE KLJUČNA PROMJENA ---
        // 4. PRIPREMA SLIDERA (Postavljamo vrijednost PRIJE paljenja ekrana!)
        float targetValue = GetCharacterSliderValue(targetCharacter);
        if (tranzicijskiSlider != null)
        {
            tranzicijskiSlider.value = targetValue; // Odmah ga stavlja na metu u mraku
        }

        if (UI_Tranzicija != null)
        {
            if (loadingAudio != null) loadingAudio.PlayComputerStartup();
            if (loadingAudio != null) loadingAudio.PlayFlicker();

            UI_Tranzicija.SetActive(true);
            CanvasGroup cg = UI_Tranzicija.GetComponent<CanvasGroup>();
            if (cg == null) cg = UI_Tranzicija.AddComponent<CanvasGroup>();

            float fadeDuration = 0.75f;
            float elapsedFade = 0f;
            while (elapsedFade < fadeDuration)
            {
                elapsedFade += Time.deltaTime;
                float baseAlpha = elapsedFade / fadeDuration;
                float jitter = Random.Range(-0.25f, 0.25f);
                cg.alpha = Mathf.Clamp01(baseAlpha + jitter);
                yield return null;
            }
            cg.alpha = 1f;

            if (loadingAudio != null) loadingAudio.PlayPlayerSelectedChord();
        }

        // 6. ZADRŽAVANJE NA EKRANU (Čeka da igrač vidi tko je odabran)
        yield return new WaitForSeconds(1.5f);


        // 7. JITTERY FADE-OUT (Drhtavo gašenje Slidera)
        if (UI_Tranzicija != null)
        {
            // DODAJ OVE DVIJE LINIJE (Gašenje kompjutera + Flicker):
            if (loadingAudio != null) loadingAudio.PlayComputerShutdown();
            if (loadingAudio != null) loadingAudio.PlayFlicker();

            CanvasGroup cg = UI_Tranzicija.GetComponent<CanvasGroup>();
            float fadeDuration = 0.3f;
            float elapsedFade = 0f;
            while (elapsedFade < fadeDuration)
            {
                elapsedFade += Time.deltaTime;
                float baseAlpha = 1f - (elapsedFade / fadeDuration);
                float jitter = Random.Range(-0.25f, 0.25f);
                cg.alpha = Mathf.Clamp01(baseAlpha + jitter);
                yield return null;
            }
            cg.alpha = 0f;
            UI_Tranzicija.SetActive(false);
            cg.alpha = 1f;
        }

        // 8. UČITAVANJE LEVELA IZA ZIDA
        ToggleLevels(targetCharacter);
        SetCameraPriorities(targetCharacter);
        FinalizeCharacterSwitch(targetCharacter);

        yield return null;
        yield return new WaitForSeconds(0.5f); // Buffer da lik padne na pod
        isLoading = false;

        // 9. DIŽEMO ZID (Ovdje će se normalno upaliti mjehurići pri dizanju!)
        if (loadingManager != null)
        {
            yield return StartCoroutine(loadingManager.RaiseWallRoutine(loadingManager.vrijemeZid));
        }

        isTransitioning = false;
    }

    void DeactivateAllCharacters()
    {
        if (sashaScript != null) sashaScript.isControlled = false;
        if (mirandaScript != null) mirandaScript.isControlled = false;
        if (giovanniScript != null) giovanniScript.isControlled = false;

        if (sashaScript != null) sashaScript.gameObject.SetActive(false);
        if (mirandaScript != null) mirandaScript.gameObject.SetActive(false);
        if (giovanniScript != null) giovanniScript.gameObject.SetActive(false);

        if (kursorSasha != null) kursorSasha.SetActive(false);

        if (UI_Sasha != null) UI_Sasha.SetActive(false);
        if (UI_Miranda != null) UI_Miranda.SetActive(false);
        if (UI_Giovanni != null) UI_Giovanni.SetActive(false);
    }

    float GetCharacterSliderValue(ActiveCharacter character)
    {
        switch (character)
        {
            case ActiveCharacter.Sasha: return 0f;
            case ActiveCharacter.Miranda: return 1f;
            case ActiveCharacter.Giovanni: return 6f;
            default: return 0f;
        }
    }


    void SetCameraPriorities(ActiveCharacter targetCharacter)
    {
        if (sashaCam != null) sashaCam.Priority = (targetCharacter == ActiveCharacter.Sasha) ? 10 : 0;
        if (mirandaCam != null) mirandaCam.Priority = (targetCharacter == ActiveCharacter.Miranda) ? 10 : 0;
        if (giovanniCam != null) giovanniCam.Priority = (targetCharacter == ActiveCharacter.Giovanni) ? 10 : 0;
    }

    void ToggleLevels(ActiveCharacter targetCharacter)
    {
        if (sashaLevel != null && sashaOdabran) sashaLevel.SetActive(true);
        if (mirandaLevel != null && mirandaOdabrana) mirandaLevel.SetActive(true);
        if (giovanniLevel != null && giovanniOdabran) giovanniLevel.SetActive(true);

        if (sashaScript != null && sashaOdabran)
            sashaScript.gameObject.SetActive(targetCharacter == ActiveCharacter.Sasha);

        if (mirandaScript != null && mirandaOdabrana)
            mirandaScript.gameObject.SetActive(targetCharacter == ActiveCharacter.Miranda);

        if (giovanniScript != null && giovanniOdabran)
            giovanniScript.gameObject.SetActive(targetCharacter == ActiveCharacter.Giovanni);
    }

    void FinalizeCharacterSwitch(ActiveCharacter newCharacter)
    {
        currChar = newCharacter;

        if (newCharacter == ActiveCharacter.Giovanni)
        {
            ApplyGiovanniEnvironment(true);
        }
        else
        {
            ApplyGiovanniEnvironment(false);
        }

        if (newCharacter == ActiveCharacter.Sasha && IsCharacterAvailable(ActiveCharacter.Sasha))
        {
            sashaScript.isControlled = true;
            if (kursorSasha != null) kursorSasha.SetActive(true);
            if (UI_Sasha != null) UI_Sasha.SetActive(true);
        }
        else if (newCharacter == ActiveCharacter.Miranda && IsCharacterAvailable(ActiveCharacter.Miranda))
        {
            mirandaScript.isControlled = true;
            if (UI_Miranda != null) UI_Miranda.SetActive(true);
        }
        else if (newCharacter == ActiveCharacter.Giovanni && IsCharacterAvailable(ActiveCharacter.Giovanni))
        {
            giovanniScript.isControlled = true;
            if (UI_Giovanni != null) UI_Giovanni.SetActive(true);
        }
    }

    void ApplyGiovanniEnvironment(bool isUnderwater)
    {
        if (isUnderwater)
        {
            // 1. Magla
            RenderSettings.fog = true;
            RenderSettings.fogColor = underwaterColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogMode = fogMode;

            // 2. Kamera (Mora biti identična boja kao magla!)
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = underwaterColor;
            }

            // 3. Svjetlo okoline (da lik ne bude skroz crn)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = underwaterColor * 0.5f;

            if (sunLight != null) sunLight.SetActive(false);
        }
        else
        {
            // Vrati na crni mrak za Sashu/Mirandu
            RenderSettings.fog = false;
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
            }
            RenderSettings.ambientLight = Color.black;

            // Ovdje odluči pališ li sunce ili ne (ovisno o levelu)
            // if (sunLight != null) sunLight.SetActive(true); 
        }
    }


    /*private void LockCursor(bool lockState)
    {
        Cursor.visible = !lockState;
    }*/

    void DisableAllControls()
    {
        if (sashaScript != null)
        {
            sashaScript.PrisilnoPrekiniInterakciju();
            sashaScript.isControlled = false;
        }

        if (mirandaScript != null)
        {
            if (mirandaScript.GetComponent<MirandaController>() != null)
                mirandaScript.GetComponent<MirandaController>().SetLock(false);

            Ventilacija_In_Miranda mirandaVent = FindFirstObjectByType<Ventilacija_In_Miranda>();
            if (mirandaVent != null) mirandaVent.SendMessage("CloseUI", SendMessageOptions.DontRequireReceiver);

            mirandaScript.isControlled = false;
        }

        if (giovanniScript != null)
        {
            if (giovanniScript.GetComponent<GiovanniController>() != null)
                giovanniScript.GetComponent<GiovanniController>().SetLock(false);

            Computer komp = FindFirstObjectByType<Computer>();
            if (komp != null) komp.ToggleComputerState(false);

            giovanniScript.isControlled = false;
        }

        if (kursorSasha != null) kursorSasha.SetActive(false);
    }

    void HideAllCharacterUIs()
    {
        // Gasi samo UI od likova
        if (UI_Sasha != null) UI_Sasha.SetActive(false);
        if (UI_Miranda != null) UI_Miranda.SetActive(false);
        if (UI_Giovanni != null) UI_Giovanni.SetActive(false);
    }

    public void QuitGame()
    {
        //LockCursor(false);
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}