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

    [Header("Win State Settings")]
    public GameObject canvasConfetti;
    public UIConfettiEmitter confettiEmitter;
    public GameObject UI_CongratsScreen;

    [Header("Brzine Spuštanja Zida")]
    [SerializeField] private float brzinaZidaTipkaC = 0.45f;      // Brzo spuštanje za tipku C (umjesto 0.8s)
    [SerializeField] private float pobjednickiZidBrzina = 0.35f;  // Munjevito (giljotina) za pobjedu!
    [SerializeField] private float pobjednickaPauza = 2.0f;       // 2 sekunde jezive tišine prije fešte

    [Header("Completed Icons (Slider)")]
    public GameObject sashaCompletedIcon;   // Kvačica/Pečat uz Sashu na slideru
    public GameObject mirandaCompletedIcon; // Kvačica/Pečat uz Mirandu
    public GameObject giovanniCompletedIcon;// Kvačica/Pečat uz Giovannija

    [Header("Nedostupni Likovi UI (Preko imena)")]
    public GameObject sashaUnavailableUI;   // UI element preko Sashinog imena (npr. prekriženo ili zaključano)
    public GameObject mirandaUnavailableUI; // UI element preko Mirandinog imena
    public GameObject giovanniUnavailableUI;// UI element preko Giovannijevog imena

    // Zastavice da znamo tko je pobijedio
    [HideInInspector] public bool sashaWon = false;
    [HideInInspector] public bool mirandaWon = false;
    [HideInInspector] public bool giovanniWon = false;

    [Header("Transition Settings")]
    [SerializeField] private GameObject UI_Tranzicija;
    [SerializeField] private Slider tranzicijskiSlider;
    [SerializeField] private GameObject UI_EnterTipka; // NOVO: Slika tipke Enter
    [SerializeField] private float transitionDuration = 2f; // Vrijeme putovanja slidera
    [SerializeField] private float loadingDuration = 1.0f;    // NOVO: Koliko dugo traje crni ekran s kotačićem

    [Header("Elevator Buttons UI")]
    public Image slikaPanelaGumbiju; // UI Image komponenta na kojoj se mijenjaju spriteovi
    public Sprite defaultGumbiSprite; // Slika kada NIJEDAN gumb nije stisnut
    public Sprite gumb1StisnutSprite; // Slika kada je stisnut gumb 1 (Sasha)
    public Sprite gumb2StisnutSprite; // Slika kada je stisnut gumb 2 (Miranda)
    public Sprite gumb3StisnutSprite; // Slika kada je stisnut gumb 3 (Giovanni)
    public float vrijemeStisnutogGumba = 0.2f; // Koliko dugo gumb ostaje "udubljen"

    private Coroutine buttonVisualCoroutine; // Da možemo prekinuti animaciju ako igrač brzo stišće

    private bool isMenuOpen = false;
    private ActiveCharacter selectedCharInMenu;
    private Coroutine sliderCoroutine;

    private bool isTransitioning = false;

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
        if (sunLight == null) Debug.LogWarning("Sun Light referenca nedostaje u GameManageru!");

        isLoading = false;

        if (canvasConfetti != null) canvasConfetti.SetActive(false);
        if (UI_Tranzicija != null) UI_Tranzicija.SetActive(false);
        if (loadingManager != null) loadingManager.HideAll();

        // 2. NOVO: Gašenje svih elemenata vezanih uz Pobjedu (Win State)
        if (UI_CongratsScreen != null) UI_CongratsScreen.SetActive(false);
        if (sashaCompletedIcon != null) sashaCompletedIcon.SetActive(false);
        if (mirandaCompletedIcon != null) mirandaCompletedIcon.SetActive(false);
        if (giovanniCompletedIcon != null) giovanniCompletedIcon.SetActive(false);
        if (confettiEmitter != null) confettiEmitter.Stop();

        if (sashaUnavailableUI != null) sashaUnavailableUI.SetActive(!sashaOdabran);
        if (mirandaUnavailableUI != null) mirandaUnavailableUI.SetActive(!mirandaOdabrana);
        if (giovanniUnavailableUI != null) giovanniUnavailableUI.SetActive(!giovanniOdabran);

        //LockCursor(true);

        // 3. Pokretanje uvodne tranzicije
        StartCoroutine(InitialTransitionRoutine());
    }


    void Update()
    {
        // 1. OTVARANJE IZBORNIKA (Tipka C)
        if (Input.GetKeyDown(KeyCode.C) && !isTransitioning && !isMenuOpen)
        {
            if (GetAvailableCharacterCount() > 1)
            {
                StartCoroutine(OpenTransitionMenu());
            }
            else
            {
                Debug.Log("Samo je jedan lik dostupan! Tranzicija otkazana.");
            }
        }

        // 2. LOGIKA UNUTAR IZBORNIKA
        if (isMenuOpen)
        {
            // Biranje likova tipkama 1, 2, 3
            if (Input.GetKeyDown(KeyCode.Alpha1) && IsCharacterAvailable(ActiveCharacter.Sasha) && selectedCharInMenu != ActiveCharacter.Sasha)
            {
                MoveSliderTo(ActiveCharacter.Sasha);
                PrikaziKlikGumba(gumb1StisnutSprite); // NOVO: Vizualni klik za 1
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && IsCharacterAvailable(ActiveCharacter.Miranda) && selectedCharInMenu != ActiveCharacter.Miranda)
            {
                MoveSliderTo(ActiveCharacter.Miranda);
                PrikaziKlikGumba(gumb2StisnutSprite); // NOVO: Vizualni klik za 2
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && IsCharacterAvailable(ActiveCharacter.Giovanni) && selectedCharInMenu != ActiveCharacter.Giovanni)
            {
                MoveSliderTo(ActiveCharacter.Giovanni);
                PrikaziKlikGumba(gumb3StisnutSprite); // NOVO: Vizualni klik za 3
            }

            // Potvrda odabira tipkom Enter
            if (Input.GetKeyDown(KeyCode.Return) && UI_EnterTipka != null && UI_EnterTipka.activeSelf)
            {
                StartCoroutine(CloseTransitionMenuAndLoad(selectedCharInMenu));
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
                return sashaOdabran && !sashaWon && sashaScript != null && sashaCam != null && sashaScript.currentState != SashaController.SashaState.Dead;

            case ActiveCharacter.Miranda:
                return mirandaOdabrana && !mirandaWon && mirandaScript != null && mirandaCam != null && mirandaScript.currentState != MirandaController.MirandaState.Dead;

            case ActiveCharacter.Giovanni:
                return giovanniOdabran && !giovanniWon && giovanniScript != null && giovanniCam != null && giovanniScript.currentState != GiovanniController.GiovanniState.Dead;

            default:
                return false;
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


    public void TriggerWinSequence(ActiveCharacter winningCharacter)
    {
        StartCoroutine(CharacterWonRoutine(winningCharacter));
    }

    IEnumerator CharacterWonRoutine(ActiveCharacter winningCharacter)
    {
        isLoading = true;
        isTransitioning = true;
        isMenuOpen = true; // Odmah blokiramo druge unose

        // 1. Zabilježi pobjedu i upali odgovarajuću ikonu na slideru
        if (winningCharacter == ActiveCharacter.Sasha) { sashaWon = true; if (sashaCompletedIcon != null) sashaCompletedIcon.SetActive(true); }
        else if (winningCharacter == ActiveCharacter.Miranda) { mirandaWon = true; if (mirandaCompletedIcon != null) mirandaCompletedIcon.SetActive(true); }
        else if (winningCharacter == ActiveCharacter.Giovanni) { giovanniWon = true; if (giovanniCompletedIcon != null) giovanniCompletedIcon.SetActive(true); }

        DisableAllControls();

        // --- 2. MUNJEVITO (SKORO INSTANT) SPUŠTANJE ZIDA KOD POBJEDE ---
        if (loadingManager != null)
            yield return StartCoroutine(loadingManager.DropWallRoutine(pobjednickiZidBrzina));

        HideAllCharacterUIs();

        yield return new WaitForSeconds(pobjednickaPauza);


        // --- 3. IZNENADNI KAZOO I KONFETI EKSPLOZIJA! ---
        if (canvasConfetti != null) canvasConfetti.SetActive(true);
        if (confettiEmitter != null)
        {
            confettiEmitter.gameObject.SetActive(true);
            confettiEmitter.Play();
        }

        // ... (ostatak koda ostaje isti) ...


        // --- 3. IZNENADNI KAZOO ZVUK I KONFETI! ---
        if (loadingAudio != null) loadingAudio.PlayConfetti();

        if (canvasConfetti != null) canvasConfetti.SetActive(true);
        if (confettiEmitter != null)
        {
            confettiEmitter.gameObject.SetActive(true);
            confettiEmitter.Play();
        }

        // ... (nastavak na 4. korak s Congrats ekranom) ...


        // --- 4. PRIKAZ "CONGRATS" EKRANA ---
        if (UI_CongratsScreen != null)
        {
            // A) Palimo glavni Canvas da bi se natpis mogao vidjeti
            Canvas parentCanvas = UI_CongratsScreen.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null) parentCanvas.gameObject.SetActive(true);

            // =========================================================================
            // KLJUČNA LINIJA: PRISILNO GASIMO SLIDER DOK TRAJE CONGRATS SLAVLJE!
            // =========================================================================
            if (UI_Tranzicija != null) UI_Tranzicija.SetActive(false);

            // B) Palimo praznog roditelja od Congratsa
            UI_CongratsScreen.SetActive(true);

            // C) Palimo sliku unutar roditelja
            foreach (Transform child in UI_CongratsScreen.transform)
            {
                child.gameObject.SetActive(true);
            }

            if (loadingAudio != null) loadingAudio.PlayFlicker();

            CanvasGroup cg = UI_CongratsScreen.GetComponent<CanvasGroup>();
            if (cg == null) cg = UI_CongratsScreen.AddComponent<CanvasGroup>();

            float fadeDuration = 0.5f;
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

        // --- 5. SLAVLJE I PRIRODNI IZLAZAK KONFETA ---
        yield return new WaitForSeconds(1f);
        if (confettiEmitter != null) confettiEmitter.Stop(); // Konfeti staju na pola
        yield return new WaitForSeconds(1.5f);

        // --- 6. NAGLI PREKID NATPISA ---
        if (loadingAudio != null) loadingAudio.PlayFlicker();
        if (UI_CongratsScreen != null) UI_CongratsScreen.SetActive(false); // Gasimo Congrats!

        yield return new WaitForSeconds(0.5f); // 0.5s čiste tišine u mraku

        // --- 7. TRAŽENJE SLJEDEĆEG LIKA I PALJENJE IZBORNIKA ---

        // A) Algoritam koji traži prvog idućeg slobodnog lika u krug (Sasha -> Miranda -> Giovanni)
        ActiveCharacter nextChar = ActiveCharacter.Odabir;
        ActiveCharacter check = winningCharacter;

        for (int i = 0; i < 3; i++)
        {
            if (check == ActiveCharacter.Sasha) check = ActiveCharacter.Miranda;
            else if (check == ActiveCharacter.Miranda) check = ActiveCharacter.Giovanni;
            else if (check == ActiveCharacter.Giovanni) check = ActiveCharacter.Sasha;

            // Provjeravamo je li taj lik u igri, živ i DA NIJE VEĆ POBIJEDIO:
            if (IsCharacterAvailable(check))
            {
                nextChar = check;
                break;
            }
        }

        // B) Provjera: Što ako su SVI likovi već pobijedili? (KRAJ CIJELE IGRE!)
        if (nextChar == ActiveCharacter.Odabir)
        {
            Debug.Log("ČESTITAMO! Svi likovi su završili svoje levele! KRAJ IGRE!");
            // Ovdje kasnije možemo dodati završnu špicu ili pobjednički meni!
            yield break;
        }


        // C) Postavljamo slider na pobjednika u mraku da se vidi njegov pečat čim se TV upali
        if (tranzicijskiSlider != null)
            tranzicijskiSlider.value = GetCharacterSliderValue(winningCharacter);
        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(false);


        // D) Drhtavo paljenje TV-a (Slider izbornika)
        if (UI_Tranzicija != null)
        {
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

        // E) Kratka pauza (0.4s) da igrač vidi pobjednički pečat na liku koji je završio
        yield return new WaitForSeconds(0.4f);


        // F) AUTOMATSKI POMICAMO SLIDER NA SLJEDEĆEG SLOBODNOG LIKA!
        // Ova metoda će sama pokrenuti zvukove klikanja i upaliti tipku ENTER čim stigne!
        MoveSliderTo(nextChar);
    }

    // =========================================================================
    // GIOVANNI FAIL / PRERANA SMRT (Lignja ga je pojela!)
    // =========================================================================
    public void TriggerGiovanniFail()
    {
        StartCoroutine(GiovanniFailRoutine());
    }

    IEnumerator GiovanniFailRoutine()
    {
        isLoading = true;
        isTransitioning = true;
        isMenuOpen = true;

        // 1. GIOVANNI JE SLUŽBENO MRTAV!
        if (giovanniScript != null)
        {
            giovanniScript.currentState = GiovanniController.GiovanniState.Dead;
        }
        giovanniWon = false; // NIJE pobijedio!

        DisableAllControls();

        // 2. Munjeviti zid (0.35s)
        if (loadingManager != null)
            yield return StartCoroutine(loadingManager.DropWallRoutine(pobjednickiZidBrzina));

        HideAllCharacterUIs();

        // 3. Tišina 2 sekunde
        yield return new WaitForSeconds(pobjednickaPauza);

        // 4. I dalje svira Kazoo i padaju konfeti (Crni humor i lažno slavlje!)
        if (canvasConfetti != null) canvasConfetti.SetActive(true);
        if (confettiEmitter != null)
        {
            confettiEmitter.gameObject.SetActive(true);
            confettiEmitter.Play();
        }

        // Prikaz "Congrats" (Igra mu se ruga što je požurio s pištoljem)
        if (UI_CongratsScreen != null)
        {
            Canvas parentCanvas = UI_CongratsScreen.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null) parentCanvas.gameObject.SetActive(true);
            if (UI_Tranzicija != null) UI_Tranzicija.SetActive(false);

            UI_CongratsScreen.SetActive(true);
            foreach (Transform child in UI_CongratsScreen.transform) child.gameObject.SetActive(true);

            if (loadingAudio != null) loadingAudio.PlayFlicker();
            yield return new WaitForSeconds(1.5f);
            if (confettiEmitter != null) confettiEmitter.Stop();
            yield return new WaitForSeconds(1.5f);

            if (loadingAudio != null) loadingAudio.PlayFlicker();
            UI_CongratsScreen.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        // 5. NA SLIDERU SE PALI IKSIĆ UMESTO KVAČICE!
        if (giovanniUnavailableUI != null) giovanniUnavailableUI.SetActive(true); // Crveni iksić!
        if (giovanniCompletedIcon != null) giovanniCompletedIcon.SetActive(false); // Nema kvačice!

        // 6. Tražimo idućeg živog lika koji nije Dead
        ActiveCharacter nextChar = ActiveCharacter.Odabir;
        ActiveCharacter check = ActiveCharacter.Giovanni;

        for (int i = 0; i < 3; i++)
        {
            if (check == ActiveCharacter.Sasha) check = ActiveCharacter.Miranda;
            else if (check == ActiveCharacter.Miranda) check = ActiveCharacter.Giovanni;
            else if (check == ActiveCharacter.Giovanni) check = ActiveCharacter.Sasha;

            if (IsCharacterAvailable(check))
            {
                nextChar = check;
                break;
            }
        }

        // Postavljamo slider na Giovannija da igrač vidi IKSIĆ
        if (tranzicijskiSlider != null) tranzicijskiSlider.value = GetCharacterSliderValue(ActiveCharacter.Giovanni);

        // Palimo TV
        if (UI_Tranzicija != null)
        {
            if (loadingAudio != null) loadingAudio.PlayComputerStartup();
            if (loadingAudio != null) loadingAudio.PlayFlicker();
            UI_Tranzicija.SetActive(true);
        }

        yield return new WaitForSeconds(0.4f);

        // Slider automatski bježi s Giovannija na sljedećeg slobodnog lika!
        if (nextChar != ActiveCharacter.Odabir)
        {
            MoveSliderTo(nextChar);
        }
        else
        {
            Debug.Log("Nema više živih likova! KRAJ IGRE.");
        }
    }

    // --- FAZA 1: OTVARANJE IZBORNIKA ---
    IEnumerator OpenTransitionMenu()
    {
        isLoading = true;
        isTransitioning = true;
        isMenuOpen = true;

        selectedCharInMenu = currChar; // Počinjemo od lika kojeg trenutno igramo
        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(false);

        // 1. BRZO SPUŠTANJE ZIDA NA TIPKU C
        if (loadingManager != null)
            yield return StartCoroutine(loadingManager.DropWallRoutine(brzinaZidaTipkaC));

        // 2. ODUZIMAMO KONTROLE I UI
        DisableAllControls();
        HideAllCharacterUIs();

        // 3. JITTERY FADE-IN (Paljenje TV-a)
        if (UI_Tranzicija != null)
        {
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

        // Postavljamo slider na trenutnog lika i palimo Enter tipku
        if (tranzicijskiSlider != null) tranzicijskiSlider.value = GetCharacterSliderValue(currChar);
        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(true);
    }


    // --- FAZA 2: POMICANJE SLIDERA (Poziva se tipkama 1, 2, 3) ---
    void MoveSliderTo(ActiveCharacter targetCharacter)
    {
        selectedCharInMenu = targetCharacter;

        // Ako se slider već miče, prekidamo staro micanje i krećemo prema novom liku
        if (sliderCoroutine != null) StopCoroutine(sliderCoroutine);
        sliderCoroutine = StartCoroutine(SliderMovementRoutine(targetCharacter));
    }

    IEnumerator SliderMovementRoutine(ActiveCharacter targetCharacter)
    {
        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(false); // Gasimo Enter dok putuje

        float startValue = tranzicijskiSlider.value;
        float endValue = GetCharacterSliderValue(targetCharacter);
        float elapsed = 0f;
        int lastClickTick = Mathf.RoundToInt(startValue);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            if (tranzicijskiSlider != null)
            {
                tranzicijskiSlider.value = Mathf.Lerp(startValue, endValue, t);

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

        // Stigli smo na metu! Palimo Enter tipku.
        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(true);
    }


    // --- FAZA 3: ZATVARANJE IZBORNIKA I UČITAVANJE (Tipka Enter) ---
    IEnumerator CloseTransitionMenuAndLoad(ActiveCharacter targetCharacter)
    {
        isMenuOpen = false;
        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(false);

        // Zvuk uspješnog odabira!
        if (loadingAudio != null) loadingAudio.PlayPlayerSelectedChord();
        yield return new WaitForSeconds(0.3f);

        // 1. JITTERY FADE-OUT (Gašenje TV-a)
        if (UI_Tranzicija != null)
        {
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

        if (loadingManager != null) loadingManager.ShowSpinner();

        // 2. UČITAVANJE SVEGA IZA ZIDA
        currChar = ActiveCharacter.Odabir;
        ToggleLevels(targetCharacter);
        SetCameraPriorities(targetCharacter);
        FinalizeCharacterSwitch(targetCharacter);

        yield return null;
        yield return new WaitForSeconds(loadingDuration);

        // 3. DIŽEMO ZID
        if (loadingManager != null)
            yield return StartCoroutine(loadingManager.RaiseWallRoutine(loadingManager.vrijemeZid));

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


        // 4. PRIPREMA SLIDERA (Postavljamo vrijednost PRIJE paljenja ekrana!)
        float targetValue = GetCharacterSliderValue(targetCharacter);
        if (tranzicijskiSlider != null)
        {
            tranzicijskiSlider.value = targetValue; // Odmah ga stavlja na metu u mraku
        }

        if (UI_EnterTipka != null) UI_EnterTipka.SetActive(false);

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

    // Metoda koja pokreće vizualni klik
    void PrikaziKlikGumba(Sprite stisnutiSprite)
    {
        if (slikaPanelaGumbiju == null || stisnutiSprite == null) return;

        // Ako se neki gumb već animira, prekidamo ga da možemo stisnuti novi
        if (buttonVisualCoroutine != null) StopCoroutine(buttonVisualCoroutine);

        buttonVisualCoroutine = StartCoroutine(ButtonVisualRoutine(stisnutiSprite));
    }

    IEnumerator ButtonVisualRoutine(Sprite stisnutiSprite)
    {
        if (loadingAudio != null) loadingAudio.PlayButtonClick();

        slikaPanelaGumbiju.sprite = stisnutiSprite;

        yield return new WaitForSeconds(vrijemeStisnutogGumba);

        if (loadingAudio != null) loadingAudio.PlayButtonRelease();

        slikaPanelaGumbiju.sprite = defaultGumbiSprite;
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
            // PROVJERAVAMO TREBA LI ODIGRATI INTRO:
            MirandaIntroSequence intro = mirandaScript.GetComponent<MirandaIntroSequence>();
            if (intro != null && !intro.hasPlayedIntro)
            {
                // Pokrećemo intro (skripta će sama dati kontrole kad se vrata zatvore!)
                intro.PokreniIntro();
            }
            else
            {
                // Ako je intro već odrađen prije, odmah dajemo kontrole
                mirandaScript.isControlled = true;
            }

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

    public void WinCurrentCharacter()
    {
        if (isTransitioning || isLoading || currChar == ActiveCharacter.Odabir)
            return;

        TriggerWinSequence(currChar);
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