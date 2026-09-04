using UnityEngine;

public class VB_Ponasanje : MonoBehaviour
{
    [Header("Djeca i Komponente")]
    [SerializeField] private SpriteRenderer okoSpriteRenderer;
    [SerializeField] private Animator okoAnimator;
    [SerializeField] private Collider vidokrugCollider;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteIspred;
    [SerializeField] private Sprite spriteLijevo;
    [SerializeField] private Sprite spriteDesno;

    [Header("Postavke")]
    [SerializeField] private float zonaIspred = 1.5f;
    [SerializeField] private float ubrzanjeVremena = 5f;
    private MirandaController mirandaControllerRef;
    private CharacterController mirandaCollider; // Za točnu detekciju tijela
    private bool aktivna;

    [Header("Svijetla")]
    [SerializeField] private GameObject svijetloL;
    [SerializeField] private GameObject svijetloR;
    [SerializeField] private GameObject svijetloFwd;

    [Header("Audio (Sirena Oka)")]
    [SerializeField] private AudioSource alertAudioSource;
    [SerializeField] private AudioClip alertClip;
    [Range(0f, 1f)][SerializeField] private float alertVolume = 0.8f;

    public TimerUI timerUI;
    private bool isSpotted = false;

    // Zajednički brojač za SVE kamere u igri
    private static int brojKameraKojeVideMirandu = 0;

    void Awake()
    {
        // 1. AUTOMATSKO PRONALAŽENJE SVOJEG VIDOKRUGA (Garantira da ne gleda tuđe oko!)
        if (vidokrugCollider == null)
        {
            Collider[] childColliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in childColliders)
            {
                if (c.isTrigger && c.gameObject != this.gameObject)
                {
                    vidokrugCollider = c;
                    break;
                }
            }
        }

        // 2. Automatsko pronalaženje komponenti ako nedostaju
        if (okoSpriteRenderer == null) okoSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (okoAnimator == null) okoAnimator = GetComponentInChildren<Animator>();

        if (alertAudioSource == null) alertAudioSource = GetComponent<AudioSource>();
        if (alertAudioSource == null) alertAudioSource = gameObject.AddComponent<AudioSource>();

        alertAudioSource.spatialBlend = 1f; // 3D zvuk
        alertAudioSource.playOnAwake = false;
        alertAudioSource.minDistance = 2f;
        alertAudioSource.maxDistance = 20f;
    }

    void Start()
    {
        DohvatiMirandu();

        if (timerUI == null) timerUI = FindFirstObjectByType<TimerUI>();

        if (svijetloR != null) svijetloR.SetActive(false);
        if (svijetloL != null) svijetloL.SetActive(false);
        if (svijetloFwd != null) svijetloFwd.SetActive(false);
    }

    void DohvatiMirandu()
    {
        if (mirandaControllerRef == null)
        {
            mirandaControllerRef = FindFirstObjectByType<MirandaController>();
        }

        if (mirandaControllerRef != null)
        {
            mirandaCollider = mirandaControllerRef.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (mirandaControllerRef == null || mirandaCollider == null)
        {
            DohvatiMirandu();
            return;
        }

        if (vidokrugCollider == null) return;

        aktivna = mirandaControllerRef.isControlled;

        // NOVO: Provjerava siječe li se cijelo Mirandino TIJELO s vidokrugom (100% točno!)
        bool trenutnoUnutra = vidokrugCollider.bounds.Intersects(mirandaCollider.bounds);

        if (trenutnoUnutra && aktivna)
        {
            if (!isSpotted)
            {
                isSpotted = true;
                brojKameraKojeVideMirandu++;

                // 1. LOKALNO: Svako oko pali svoj 3D zvuk
                if (alertAudioSource != null && alertClip != null)
                {
                    alertAudioSource.clip = alertClip;
                    alertAudioSource.loop = true;
                    alertAudioSource.volume = alertVolume;
                    if (!alertAudioSource.isPlaying) alertAudioSource.Play();
                }

                // 2. GLOBALNO: Ubrzaj sat (TimerUI)
                if (timerUI != null)
                {
                    timerUI.SetTimeMultiplier(ubrzanjeVremena);
                }
            }

            // Gasi animator dok je prati očima
            if (okoAnimator != null) okoAnimator.enabled = false;

            PratiOčima();
        }
        else
        {
            if (isSpotted)
            {
                isSpotted = false;
                brojKameraKojeVideMirandu = Mathf.Max(0, brojKameraKojeVideMirandu - 1);

                // Gašenje svjetala ovog oka
                if (svijetloR != null) svijetloR.SetActive(false);
                if (svijetloL != null) svijetloL.SetActive(false);
                if (svijetloFwd != null) svijetloFwd.SetActive(false);
                if (okoSpriteRenderer != null) okoSpriteRenderer.sprite = spriteIspred;

                // Prirodni završetak sirene
                if (alertAudioSource != null && alertAudioSource.isPlaying)
                {
                    alertAudioSource.loop = false;
                }

                // Vrati sat na 1x ako je nijedno drugo oko ne vidi
                if (brojKameraKojeVideMirandu == 0)
                {
                    if (timerUI != null) timerUI.SetTimeMultiplier(1f);
                }
            }

            // Animator šara lijevo-desno kada ovo oko ne vidi Mirandu
            if (okoAnimator != null)
            {
                okoAnimator.enabled = aktivna;
            }
        }
    }

    void PratiOčima()
    {
        if (okoSpriteRenderer == null || mirandaControllerRef == null) return;

        float zRazlika = mirandaControllerRef.transform.position.z - transform.position.z;

        if (zRazlika > zonaIspred)
        {
            okoSpriteRenderer.sprite = spriteDesno;
            if (svijetloR != null) svijetloR.SetActive(true);
            if (svijetloL != null) svijetloL.SetActive(false);
            if (svijetloFwd != null) svijetloFwd.SetActive(false);
        }
        else if (zRazlika < -zonaIspred)
        {
            okoSpriteRenderer.sprite = spriteLijevo;
            if (svijetloR != null) svijetloR.SetActive(false);
            if (svijetloL != null) svijetloL.SetActive(true);
            if (svijetloFwd != null) svijetloFwd.SetActive(false);
        }
        else
        {
            okoSpriteRenderer.sprite = spriteIspred;
            if (svijetloR != null) svijetloR.SetActive(false);
            if (svijetloL != null) svijetloL.SetActive(false);
            if (svijetloFwd != null) svijetloFwd.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (isSpotted)
        {
            isSpotted = false;
            brojKameraKojeVideMirandu = Mathf.Max(0, brojKameraKojeVideMirandu - 1);

            if (brojKameraKojeVideMirandu == 0)
            {
                if (timerUI != null) timerUI.SetTimeMultiplier(1f);
            }

            if (alertAudioSource != null && alertAudioSource.isPlaying)
            {
                alertAudioSource.Stop();
            }
        }
    }
}