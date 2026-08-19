using UnityEngine;

public class VB_Ponasanje : MonoBehaviour
{
    [Header("Djeca i Komponente")]
    [SerializeField] private SpriteRenderer okoSpriteRenderer;
    [SerializeField] private Animator okoAnimator;
    [SerializeField] private Collider vidokrugCollider; // Povuci 'Vidokrug' dijete ovdje

    [Header("Sprites")]
    [SerializeField] private Sprite spriteIspred;
    [SerializeField] private Sprite spriteLijevo;
    [SerializeField] private Sprite spriteDesno;

    [Header("Postavke")]
    [SerializeField] private float zonaIspred = 1.5f;
    [SerializeField] private float ubrzanjeVremena = 5f;
    [SerializeField] private string playerTag = "Miranda";
    private MirandaController mirandaControllerRef;
    private bool aktivna;

    [Header("Svijetla")]
    [SerializeField] private GameObject svijetloL;
    [SerializeField] private GameObject svijetloR;
    [SerializeField] private GameObject svijetloFwd;


    public TimerUI timerUI;
    private GameObject igrac;
    private bool isSpotted = false;

    void Start()
    {
        if (igrac == null) igrac = GameObject.FindWithTag(playerTag);

        if (mirandaControllerRef == null)
        {
            mirandaControllerRef = FindFirstObjectByType<MirandaController>();
        }

        if (timerUI == null) timerUI = GameObject.FindAnyObjectByType<TimerUI>();

        if (vidokrugCollider == null) Debug.LogError("VB: Povuci 'Vidokrug' objekt u polje Vidokrug Collider!");

        svijetloR.SetActive(false);
        svijetloL.SetActive(false);
        svijetloFwd.SetActive(false);
    }

    void Update()
    {
        if (igrac == null || vidokrugCollider == null) return;

        aktivna = mirandaControllerRef.isControlled;

        bool trenutnoUnutra = vidokrugCollider.bounds.Contains(igrac.transform.position);

        if (trenutnoUnutra && aktivna)
        {
            if (!isSpotted)
            {
                isSpotted = true;
                AktivirajUocavanje(true);
            }
            if (!SFX.zvucniEfekti.zvukMiniAlert.isPlaying)
            {
                SFX.zvucniEfekti.zvukMiniAlert.Play();
            }
            PratiOčima();
        }
        else
        {
            if (isSpotted)
            {
                isSpotted = false;
                AktivirajUocavanje(false);
            }
            SFX.zvucniEfekti.zvukMiniAlert.Stop();
        }
    }

    void AktivirajUocavanje(bool uocena)
    {
        if (uocena)
        {
            if (okoAnimator != null) okoAnimator.enabled = false;
            if (timerUI != null) timerUI.SetTimeMultiplier(ubrzanjeVremena);
        }
        else
        {
            svijetloR.SetActive(false);
            svijetloL.SetActive(false);
            svijetloFwd.SetActive(false);
            if (okoAnimator != null) okoAnimator.enabled = true;
            if (timerUI != null) timerUI.SetTimeMultiplier(1f);
            if (okoSpriteRenderer != null) okoSpriteRenderer.sprite = spriteIspred;
        }
    }

    void PratiOčima()
    {
        if (okoSpriteRenderer == null) return;

        float zRazlika = igrac.transform.position.z - transform.position.z;

        if (zRazlika > zonaIspred) {
            okoSpriteRenderer.sprite = spriteDesno;
            svijetloR.SetActive(true);
            svijetloL.SetActive(false);
            svijetloFwd.SetActive(false);
        }
        else if (zRazlika < -zonaIspred) {
            okoSpriteRenderer.sprite = spriteLijevo;
            svijetloR.SetActive(false);
            svijetloL.SetActive(true);
            svijetloFwd.SetActive(false);
        }
        else
        {
            okoSpriteRenderer.sprite = spriteIspred;
            svijetloR.SetActive(false);
            svijetloL.SetActive(false);
            svijetloFwd.SetActive(true);
        }
    }
}