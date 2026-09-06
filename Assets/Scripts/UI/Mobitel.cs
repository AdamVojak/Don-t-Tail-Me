using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static GiovanniController;

public class MobitelTracker : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GiovanniInventory inventory;
    [SerializeField] private GiovanniController giovanniControllerRef;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Image arrowImage;
    [SerializeField] private CursorManager cursorManager;
    [SerializeField] private GiovanniStats giovanniStatsRef;
    [SerializeField] private int threatCost = 25;
    [SerializeField] private Computer computer;
    [SerializeField] private Ventilacija_Out_Giovanni ventGiovanni;

    [Header("Spriteovi Strelice")]
    [SerializeField] private Sprite strelicaGore;
    [SerializeField] private Sprite strelicaLijevo;
    [SerializeField] private Sprite strelicaDesno;
    [SerializeField] private Sprite krizic;

    [Header("Angler Fish i Out of Bounds")]
    [SerializeField] private AnglerFishController anglerFish;
    [SerializeField] private Sprite strelicaPolukruzno;

    [Header("Audio")]
    [SerializeField] private MobitelAudio mobitelAudio;

    [Header("Ciljevi na Mapi (Redoslijedno)")]
    [SerializeField] private Transform[] ciljevi;
    private int trenutniItemIndex = 0;

    private bool needsToVisitComputer = false;
    private bool allItemsFinished = false;
    private bool isTracking = false;

    [Header("Animacija mobitela")]
    [SerializeField] private int brojBlicanja = 8;
    [SerializeField] private float vrijemeBlicanja = 0.2f;

    void Start()
    {
        if (mobitelAudio == null) mobitelAudio = GetComponent<MobitelAudio>();
        if (giovanniControllerRef == null) giovanniControllerRef = FindFirstObjectByType<GiovanniController>();
        if (cursorManager == null) cursorManager = FindFirstObjectByType<CursorManager>();
        if (arrowImage != null) arrowImage.enabled = false;
        if (playerTransform == null) playerTransform = this.transform;
        if (giovanniStatsRef == null) giovanniStatsRef = FindFirstObjectByType<GiovanniStats>();
        if (computer == null) computer = FindFirstObjectByType<Computer>();
        if (ventGiovanni == null) ventGiovanni = FindFirstObjectByType<Ventilacija_Out_Giovanni>();

        needsToVisitComputer = false;
    }

    void Update()
    {
        bool aktivan = giovanniControllerRef != null && giovanniControllerRef.isControlled;

        if (giovanniControllerRef == null || giovanniControllerRef.currentState == GiovanniState.Dead || !aktivan)
        {
            return;
        }

        ProvjeriPokupljeniItem();

        bool isOutOfBounds = (anglerFish != null && anglerFish.IsOutOfBounds);

        if (isOutOfBounds && !isTracking && inventory != null && inventory.HasItem(GiovanniInventory.ID_MOBITEL))
        {
            StartCoroutine(BlinkArrowRoutine(false));
        }

        else if (!isOutOfBounds && Input.GetKeyDown(KeyCode.Mouse0) && !isTracking)
        {
            bool computerBusy = computer != null && computer.IsInteracting();
            bool gledaUPredmet = cursorManager != null && cursorManager.IsTargetingItem;

            if (!computerBusy && !gledaUPredmet && inventory != null && inventory.HasItem(GiovanniInventory.ID_MOBITEL))
            {
                StartCoroutine(BlinkArrowRoutine(true));
            }
        }
    }

    public void OnComputerUsed()
    {
        if (allItemsFinished) return;

        needsToVisitComputer = false;

        if (trenutniItemIndex >= ciljevi.Length)
        {
            allItemsFinished = true;
            Debug.Log("Završeni svi zadaci s računalom i itemima!");
        }
    }

    private void ProvjeriPokupljeniItem()
    {
        if (!needsToVisitComputer && trenutniItemIndex < ciljevi.Length)
        {
            if (ciljevi[trenutniItemIndex] == null)
            {
                trenutniItemIndex++;
                needsToVisitComputer = true;
                Debug.Log("Item pokupljen! Mobitel sada vodi do Računala.");
            }
        }
    }

    private Transform GetActiveTarget()
    {
        if (anglerFish != null && anglerFish.IsOutOfBounds)
        {
            return anglerFish.MapCenterTransform;
        }

        if (ventGiovanni != null && ventGiovanni.ImaAktivnogItema)
        {
            return ventGiovanni.transform;
        }

        if (allItemsFinished)
        {
            return null;
        }

        if (needsToVisitComputer)
        {
            return (computer != null) ? computer.transform : null;
        }

        if (trenutniItemIndex < ciljevi.Length && ciljevi[trenutniItemIndex] != null)
        {
            return ciljevi[trenutniItemIndex];
        }

        return null;
    }


    private IEnumerator BlinkArrowRoutine(bool consumeThreat)
    {
        isTracking = true;

        if (consumeThreat && giovanniStatsRef != null)
        {
            giovanniStatsRef.ReduceThreat(threatCost);
        }

        for (int i = 0; i < brojBlicanja; i++)
        {
            PostaviIspravanSprite(GetActiveTarget());

            arrowImage.enabled = true;
            yield return new WaitForSeconds(vrijemeBlicanja);

            arrowImage.enabled = false;
            yield return new WaitForSeconds(vrijemeBlicanja);
        }

        isTracking = false;
    }

    private void PostaviIspravanSprite(Transform trenutniCilj)
    {
        // 1. KRIŽIĆ (Nema više itema) -> Error zvuk i križić sprite
        if (trenutniCilj == null)
        {
            if (mobitelAudio != null) mobitelAudio.PlayError();
            arrowImage.sprite = krizic;
            return;
        }

        Vector3 smjerPremaCilju = trenutniCilj.position - playerTransform.position;
        smjerPremaCilju.y = 0;

        Vector3 igracNaprijed = playerTransform.forward;
        igracNaprijed.y = 0;

        float kut = Vector3.SignedAngle(igracNaprijed, smjerPremaCilju, Vector3.up);

        // 2. NOVO: U-TURN (Igrač gleda u pogrešnom smjeru izvan mape) -> ERROR ZVUK i polukružna strelica!
        if (anglerFish != null && anglerFish.IsOutOfBounds && (kut > 120f || kut < -120f) && strelicaPolukruzno != null)
        {
            if (mobitelAudio != null) mobitelAudio.PlayError();
            arrowImage.sprite = strelicaPolukruzno;
            return;
        }

        // 3. NORMALNE STRELICE PREMA CILJU
        if (kut > -25f && kut < 25f)
        {
            if (SFX.zvucniEfekti != null && SFX.zvucniEfekti.ZvukMobitelaFwd != null) SFX.zvucniEfekti.ZvukMobitelaFwd.Play();
            arrowImage.sprite = strelicaGore;
        }
        else if (kut <= -25f)
        {
            if (SFX.zvucniEfekti != null && SFX.zvucniEfekti.ZvukMobitelaL != null) SFX.zvucniEfekti.ZvukMobitelaL.Play();
            arrowImage.sprite = strelicaLijevo;
        }
        else if (kut >= 25f)
        {
            if (SFX.zvucniEfekti != null && SFX.zvucniEfekti.ZvukMobitelaR != null) SFX.zvucniEfekti.ZvukMobitelaR.Play();
            arrowImage.sprite = strelicaDesno;
        }
    }

    public bool AreAllItemsFinished()
    {
        return allItemsFinished;
    }
}