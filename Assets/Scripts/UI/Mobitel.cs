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

    [Header("Audio")]
    [SerializeField] private MobitelAudio mobitelAudio; // NOVO: Referenca na audio

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

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isTracking && aktivan)
        {
            bool computerBusy = computer != null && computer.IsInteracting();
            bool gledaUPredmet = cursorManager != null && cursorManager.IsTargetingItem;

            if (!computerBusy && !gledaUPredmet && inventory != null && inventory.HasItem(GiovanniInventory.ID_MOBITEL))
            {
                StartCoroutine(BlinkArrowRoutine());

                if (giovanniStatsRef != null)
                {
                    giovanniStatsRef.ReduceThreat(threatCost);
                }
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

    private IEnumerator BlinkArrowRoutine()
    {
        isTracking = true;

        Transform trenutniCilj = GetActiveTarget();

        // ZVUK GREŠKE (Ako nema cilja, svira točno jednom na početku):
        if (trenutniCilj == null && mobitelAudio != null)
        {
            mobitelAudio.PlayError();
        }

        for (int i = 0; i < brojBlicanja; i++)
        {
            PostaviIspravanSprite(trenutniCilj);

            arrowImage.enabled = true;
            yield return new WaitForSeconds(vrijemeBlicanja);

            arrowImage.enabled = false;
            yield return new WaitForSeconds(vrijemeBlicanja);
        }

        isTracking = false;
    }

    private void PostaviIspravanSprite(Transform trenutniCilj)
    {
        if (trenutniCilj == null)
        {
            arrowImage.sprite = krizic;
            return;
        }

        Vector3 smjerPremaCilju = trenutniCilj.position - playerTransform.position;
        smjerPremaCilju.y = 0;

        Vector3 igracNaprijed = playerTransform.forward;
        igracNaprijed.y = 0;

        float kut = Vector3.SignedAngle(igracNaprijed, smjerPremaCilju, Vector3.up);

        if (kut > -20f && kut < 20f)
        {
            // RAVNO (Centar pan = 0):
            if (mobitelAudio != null) mobitelAudio.PlayForwardPing();
            arrowImage.sprite = strelicaGore;
        }
        else if (kut <= -20f)
        {
            // LIJEVO (Pan = -0.65):
            if (mobitelAudio != null) mobitelAudio.PlayLeftPing();
            arrowImage.sprite = strelicaLijevo;
        }
        else if (kut >= 20f)
        {
            // DESNO (Pan = +0.65):
            if (mobitelAudio != null) mobitelAudio.PlayRightPing();
            arrowImage.sprite = strelicaDesno;
        }
    }
}