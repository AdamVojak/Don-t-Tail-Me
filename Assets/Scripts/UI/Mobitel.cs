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

    [Header("Spriteovi Strelice")]
    [SerializeField] private Sprite strelicaGore;
    [SerializeField] private Sprite strelicaLijevo;
    [SerializeField] private Sprite strelicaDesno;
    [SerializeField] private Sprite krizic;

    [Header("Ciljevi (redoslijedno)")]
    [SerializeField] private Transform[] ciljevi;
    private int trenutniCiljIndex = 0;

    private bool isTracking = false;

    [Header("Animacija mobitela")]
    [SerializeField] private int brojBlicanja = 8;
    [SerializeField] private float vrijemeBlicanja = 0.2f;

    void Start()
    {
        if(giovanniControllerRef == null)
        {
            giovanniControllerRef = FindFirstObjectByType<GiovanniController>();
        }
        if (cursorManager == null)
        {
            cursorManager = FindFirstObjectByType<CursorManager>();
        }
        if (arrowImage != null)
        {
            arrowImage.enabled = false;
        }

        if (playerTransform == null)
        {
            playerTransform = this.transform;
        }
        if (giovanniStatsRef != null)
        {
            giovanniStatsRef = FindFirstObjectByType<GiovanniStats>();
        }
    }

    void Update()
    {
        bool aktivan = giovanniControllerRef.isControlled;

        if (giovanniControllerRef.currentState == GiovanniState.Dead || !aktivan)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isTracking && aktivan)
        {

            bool computerBusy = computer != null && computer.IsInteracting();
            bool gledaUPredmet = cursorManager != null && cursorManager.IsTargetingItem;

            if (!computerBusy &&  !gledaUPredmet && inventory != null && inventory.HasItem(GiovanniInventory.ID_MOBITEL))
            {

                StartCoroutine(BlinkArrowRoutine());

                if (giovanniStatsRef != null)
                {
                    giovanniStatsRef.ReduceThreat(threatCost);
                }
            }
        }
    }

    private IEnumerator BlinkArrowRoutine()
    {
        isTracking = true;

        AzurirajTrenutniCilj();

        if (trenutniCiljIndex >= ciljevi.Length || ciljevi[trenutniCiljIndex] == null)
        {
            Debug.Log("Svi ciljevi su pokupljeni ili nema više ciljeva!");
            isTracking = false;
            yield break;
        }

        for (int i = 0; i < brojBlicanja; i++)
        {
            PostaviIspravanSprite();

            arrowImage.enabled = true;
            yield return new WaitForSeconds(vrijemeBlicanja);

            arrowImage.enabled = false;
            yield return new WaitForSeconds(vrijemeBlicanja);
        }

        isTracking = false;
    }

    private void AzurirajTrenutniCilj()
    {
        while (trenutniCiljIndex < ciljevi.Length && ciljevi[trenutniCiljIndex] == null)
        {
            trenutniCiljIndex++;
        }
    }

    private void PostaviIspravanSprite()
    {
        Transform trenutniCilj = ciljevi[trenutniCiljIndex];
        if (trenutniCilj == null) return;

        Vector3 smjerPremaCilju = trenutniCilj.position - playerTransform.position;
        smjerPremaCilju.y = 0;

        Vector3 igracNaprijed = playerTransform.forward;
        igracNaprijed.y = 0;

        float kut = Vector3.SignedAngle(igracNaprijed, smjerPremaCilju, Vector3.up);

            if (kut > -20f && kut < 20f)
            {
                SFX.zvucniEfekti.ZvukMobitelaFwd.Play();
                arrowImage.sprite = strelicaGore;
            }
            else if (kut <= -20f)
            {
                SFX.zvucniEfekti.ZvukMobitelaL.Play();
                arrowImage.sprite = strelicaLijevo;
            }
            else if (kut >= 20f)
            {
                SFX.zvucniEfekti.ZvukMobitelaR.Play();
                arrowImage.sprite = strelicaDesno;
            }
        }
    }