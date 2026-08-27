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

    public TimerUI timerUI;
    private GameObject igrac;
    private bool isSpotted = false;

    // NOVO: Zajednički brojač za SVE kamere u igri!
    private static int brojKameraKojeVideMirandu = 0;

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
                brojKameraKojeVideMirandu++; // Dodajemo ovu kameru u brojač

                // LOKALNE PROMJENE (Samo za ovu kameru)
                if (okoAnimator != null) okoAnimator.enabled = false;

                // GLOBALNE PROMJENE (Vrijeme i Zvuk) - Palimo samo ako je ovo PRVA kamera koja ju je vidjela
                if (brojKameraKojeVideMirandu == 1)
                {
                    if (timerUI != null) timerUI.SetTimeMultiplier(ubrzanjeVremena);
                    if (!SFX.zvucniEfekti.zvukMiniAlert.isPlaying) SFX.zvucniEfekti.zvukMiniAlert.Play();
                }
            }
            
            PratiOčima();
        }
        else
        {
            if (isSpotted)
            {
                isSpotted = false;
                brojKameraKojeVideMirandu--; // Ova kamera ju više ne vidi

                // LOKALNE PROMJENE (Samo za ovu kameru)
                svijetloR.SetActive(false);
                svijetloL.SetActive(false);
                svijetloFwd.SetActive(false);
                if (okoAnimator != null) okoAnimator.enabled = true;
                if (okoSpriteRenderer != null) okoSpriteRenderer.sprite = spriteIspred;

                // GLOBALNE PROMJENE - Vraćamo u normalu SAMO ako ju NIJEDNA kamera više ne vidi
                if (brojKameraKojeVideMirandu <= 0)
                {
                    brojKameraKojeVideMirandu = 0; // Osigurač
                    if (timerUI != null) timerUI.SetTimeMultiplier(1f);
                    SFX.zvucniEfekti.zvukMiniAlert.Stop();
                }
            }
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

    // SIGURNOSNA MREŽA: Ako se kamera uništi ili ugasi dok gleda Mirandu
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
                if (SFX.zvucniEfekti != null) SFX.zvucniEfekti.zvukMiniAlert.Stop();
            }
        }
    }
}