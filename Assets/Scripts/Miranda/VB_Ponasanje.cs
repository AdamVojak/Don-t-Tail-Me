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
    [SerializeField] private string playerTag = "Miranda";
    private MirandaController mirandaControllerRef;
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
    private GameObject igrac;
    private bool isSpotted = false;

    // Zajednički brojač za SVE kamere u igri
    private static int brojKameraKojeVideMirandu = 0;

    void Start()
    {
        if (igrac == null) igrac = GameObject.FindWithTag(playerTag);

        if (mirandaControllerRef == null)
        {
            mirandaControllerRef = FindFirstObjectByType<MirandaController>();
        }

        if (timerUI == null) timerUI = GameObject.FindAnyObjectByType<TimerUI>();

        // Audio Setup
        if (alertAudioSource == null) alertAudioSource = GetComponent<AudioSource>();
        if (alertAudioSource == null) alertAudioSource = gameObject.AddComponent<AudioSource>();

        alertAudioSource.spatialBlend = 1f; // 3D zvuk oka u prostoru
        alertAudioSource.playOnAwake = false;
        alertAudioSource.minDistance = 2f;
        alertAudioSource.maxDistance = 20f;

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
                brojKameraKojeVideMirandu++;

                // GLOBALNE PROMJENE: Palimo samo ako je ovo PRVA kamera koja ju je vidjela
                if (brojKameraKojeVideMirandu == 1)
                {
                    if (timerUI != null) timerUI.SetTimeMultiplier(ubrzanjeVremena);

                    // PALIMO SIRENU U LOOPU:
                    if (alertAudioSource != null && alertClip != null)
                    {
                        alertAudioSource.clip = alertClip;
                        alertAudioSource.loop = true;
                        alertAudioSource.volume = alertVolume;
                        if (!alertAudioSource.isPlaying) alertAudioSource.Play();
                    }
                }
            }

            // KLJUČNO: Animator je 100% ugašen dok te gleda tako da PratiOčima() ima punu kontrolu!
            if (okoAnimator != null) okoAnimator.enabled = false;

            PratiOčima();
        }
        else
        {
            if (isSpotted)
            {
                isSpotted = false;
                brojKameraKojeVideMirandu--;

                svijetloR.SetActive(false);
                svijetloL.SetActive(false);
                svijetloFwd.SetActive(false);
                if (okoSpriteRenderer != null) okoSpriteRenderer.sprite = spriteIspred;

                // GLOBALNE PROMJENE: Vraćamo u normalu ako ju NIJEDNA kamera više ne vidi
                if (brojKameraKojeVideMirandu <= 0)
                {
                    brojKameraKojeVideMirandu = 0;
                    if (timerUI != null) timerUI.SetTimeMultiplier(1f);

                    // PRIRODNO GAŠENJE: Isključujemo loop da sirena odsvira krug do kraja
                    if (alertAudioSource != null && alertAudioSource.isPlaying)
                    {
                        alertAudioSource.loop = false;
                    }
                }
            }

            // Animator se pali SAMO kada te oko NE VIDI (i kad je Miranda aktivna) kako bi ponovno šarao lijevo-desno!
            if (okoAnimator != null)
            {
                okoAnimator.enabled = aktivna;
            }
        }
    }

    void PratiOčima()
    {
        if (okoSpriteRenderer == null) return;

        float zRazlika = igrac.transform.position.z - transform.position.z;

        if (zRazlika > zonaIspred)
        {
            okoSpriteRenderer.sprite = spriteDesno;
            svijetloR.SetActive(true);
            svijetloL.SetActive(false);
            svijetloFwd.SetActive(false);
        }
        else if (zRazlika < -zonaIspred)
        {
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

    private void OnDisable()
    {
        if (isSpotted)
        {
            isSpotted = false;
            brojKameraKojeVideMirandu--;
            if (brojKameraKojeVideMirandu <= 0)
            {
                brojKameraKojeVideMirandu = 0;
                if (timerUI != null) timerUI.SetTimeMultiplier(1f);

                if (alertAudioSource != null && alertAudioSource.isPlaying)
                {
                    alertAudioSource.Stop();
                }
            }
        }
    }
}