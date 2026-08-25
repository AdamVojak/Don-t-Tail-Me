using UnityEngine;
using System.Collections;


public enum HeadlessState
{
    Active,
    Stunned,
    Shocked,
    Dead
}

public class Headless : MonoBehaviour, IDamageable {

    [Header("Audio")]
    [SerializeField] private HeadlessAudio enemyAudio;

    [Header("Postavke Brzine i Juriša")]
    [SerializeField] float pocetnaBrzina = 6f;      // Brzina čim se stvori
    [SerializeField] float maksimalnaBrzina = 14f;  // Ekstremna brzina trka
    [SerializeField] float brzinaUbrzavanja = 20f;  // Koliko brzo skače na max brzinu

    [Header("Postavke Zakoraka (Impuls)")]
    [SerializeField] float snagaZakoraka = 0.3f;    // Dodatni mali "push" pri koraku
    [SerializeField] float trajanjeZakoraka = 0.06f;
    private float trenutnaBrzina;
    private Coroutine korakCoroutine;
    [SerializeField] private SashaController sashaControllerRef;
    [SerializeField] private GameManager gameManagerRef;
    public bool aktivan;

    [Header("Postavke Napada")]
    [SerializeField] private Collider triggerArea;
    [SerializeField] private GameObject fist;
    private bool uDometu = false;

    [Header("Postavke Zdravlja")]
    [SerializeField] int health = 100;
    [SerializeField] float vrijemeOsamucenosti = 0.5f;
    [SerializeField] float stetaCooldown = 1.15f;

    [Header("Postavke Strujnog Udara (Melee Stun)")]
    [SerializeField] float vrijemeStrujnogUdara = 1f;
    [SerializeField] Sprite sokiraniSprite;
    [SerializeField] int brojTreptajaBoje = 8;

    [Header("Reference")]
    public SpriteRenderer tijelo;

    [Header("Death Settings")]
    public GameObject deadHeadlessPrefab;

    [Header("NOGE")]
    public Animator animacijaNogu;
    public GameObject noge;

    private HeadlessState currentState = HeadlessState.Active;

    private Transform igracMeta;
    private Sprite originalniSprite;
    private Animator animator;
    private float zadnjeVrijemeStete = -1f;

    void Awake()
    {
        DohvatiIgraca();
        if (tijelo == null) tijelo = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (fist == null) fist = GetComponent<GameObject>();
        if (gameManagerRef == null) gameManagerRef = FindFirstObjectByType<GameManager>();
        if (enemyAudio == null) enemyAudio = GetComponent<HeadlessAudio>();

        OkreniSePremaIgracu();
    }

    void OnEnable()
    {
        currentState = HeadlessState.Active;
        DohvatiIgraca();
        OkreniSePremaIgracu();

        ResetirajUbrzanje();

        if (enemyAudio != null) enemyAudio.PlayAttackShout();
    }

    private void Start()
    {
        fist.SetActive(false);
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void DohvatiIgraca()
    {
        if (sashaControllerRef == null)
        {
            sashaControllerRef = FindFirstObjectByType<SashaController>();
        }

        if (sashaControllerRef != null)
        {
            igracMeta = sashaControllerRef.transform;
        }
        else
        {
            return;
        }
    }

    void Update()
    {
        if (sashaControllerRef == null)
        {
            DohvatiIgraca();
        }

        aktivan = (sashaControllerRef != null) ? sashaControllerRef.isControlled : false;
        bool loading = gameManagerRef.isLoading;
        bool sashaMrtva = (sashaControllerRef == null || sashaControllerRef.currentState == SashaController.SashaState.Dead);

        if (!aktivan || sashaMrtva || currentState != HeadlessState.Active || igracMeta == null || loading)
        {
            if (noge != null && noge.activeSelf)
            {
                noge.SetActive(false);
            }

            if (animator != null && animator.isActiveAndEnabled && (!aktivan || sashaMrtva))
            {
                animator.SetBool("uDometu", false);
                animator.Play("Idle", 0, 0f);
            }

            return;
        }


        if (triggerArea != null)
        {
            bool sashaJeBlizu = triggerArea.bounds.Contains(igracMeta.position);

            if (sashaJeBlizu && !uDometu)
            {
                uDometu = true;
                if (noge != null) noge.SetActive(false);
                if (animator != null) animator.SetBool("uDometu", true);

                if (enemyAudio != null) enemyAudio.PlayAttackShout();

                IzvediZamah();
            }
            else if (!sashaJeBlizu && uDometu)
            {
                uDometu = false;
                if (noge != null) noge.SetActive(true);
                fist.SetActive(false);
                if (animator != null)
                {
                    animator.SetBool("uDometu", false);
                    animator.SetInteger("strana", 0);
                    animator.Play("Idle", 0, 0f);
                }
            }
        }

        if (uDometu)
        {
            if (noge != null) noge.SetActive(false);
        }
        else
        {
            if (noge != null) noge.SetActive(true);
            animacijaNogu.Play("Idle", 0);
            animator.Play("Idle", 0);
        }

        if (!uDometu)
        {
            trenutnaBrzina = Mathf.MoveTowards(trenutnaBrzina, maksimalnaBrzina, brzinaUbrzavanja * Time.deltaTime);

            Vector3 targetPosition = new Vector3(igracMeta.position.x, igracMeta.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, trenutnaBrzina * Time.deltaTime);

            if (animacijaNogu != null)
            {
                animacijaNogu.speed = Mathf.Clamp(trenutnaBrzina / 5f, 1f, 2.5f);
            }
        }

        OkreniSePremaIgracu();
    }

    public void TakeDamage(int amount, DamageType damageType = DamageType.Physical)
    {
        if (enemyAudio != null)
        {
            if (damageType == DamageType.Physical)
            {
                enemyAudio.PlayPajserHit();
            }
            else if (damageType == DamageType.Electric)
            {
                enemyAudio.PlayElectricMeleeHit();
            }
            enemyAudio.PlayHurtSound();
        }

        Debug.Log("Udarac sa " + amount + " štete. Tip štete: " + damageType);

        if (damageType == DamageType.Electric)
        {
            PrimiStrujniUdar(amount);
        }
        else
        {
            PrimiUdarac(amount);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentState == HeadlessState.Dead) return;

        if (Time.time < zadnjeVrijemeStete + stetaCooldown) return;

        if (other.CompareTag("Projectile"))
        {
            Projektil projectile = other.GetComponent<Projektil>();
            if (projectile != null)
            {
                Destroy(other.gameObject);

                if (Time.time >= zadnjeVrijemeStete + stetaCooldown)
                {
                    enemyAudio.PlayHurtSound();
                    PrimiUdarac(projectile.damage);
                }
            }
        }

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                Destroy(other.gameObject);

                if (Time.time >= zadnjeVrijemeStete + stetaCooldown)
                {
                    enemyAudio.PlayHurtSound();
                    PrimiUdarac(bullet.damage);
                }
            }
        }
    }

    void IzvediZamah()
    {
        if (animator == null || currentState != HeadlessState.Active || aktivan == false) return;

        int strana = Random.Range(1, 11);
        animator.SetInteger("strana", strana);

        if (strana < 5)
        {
            animator.Play("Punch_Left", 0, 0f);
        }
        else
        {
            animator.Play("Punch_Right", 0, 0f);
        }
    }

    public void KrajZamaha()
    {
        fist.SetActive(false);
        if (uDometu && currentState == HeadlessState.Active)
        {
            IzvediZamah();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentState == HeadlessState.Dead) return;

        if (other.CompareTag("Sasha") || (igracMeta != null && other.transform == igracMeta))
        {
            uDometu = false;
            if (noge != null) noge.SetActive(true);
            if (animator != null)
            {
                animator.SetBool("uDometu", false);
                animator.Play("Idle", 0, 0f);
            }
        }
    }

    void PrimiUdarac(int steta)
    {
        if (currentState == HeadlessState.Dead) return;

        if (korakCoroutine != null) StopCoroutine(korakCoroutine);

        zadnjeVrijemeStete = Time.time;
        health -= steta;

        if (health <= 0)
        {
            Umri();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(EfektUdarca());
    }

    void PrimiStrujniUdar(int steta)
    {
        if (currentState == HeadlessState.Dead) return;

        if (korakCoroutine != null) StopCoroutine(korakCoroutine);

        zadnjeVrijemeStete = Time.time;
        health -= steta;

        if (health <= 0)
        {
            Umri();
            return;
        }
        StopAllCoroutines();
        StartCoroutine(EfektStruje());
    }

        void Umri()
    {
        currentState = HeadlessState.Dead;
        if (deadHeadlessPrefab != null)
        {
            Instantiate(deadHeadlessPrefab, transform.position, transform.rotation);
        }
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        this.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    IEnumerator EfektUdarca()
    {
        animacijaNogu.StopPlayback();
        if (noge != null) noge.SetActive(false);

        if (tijelo != null && tijelo.sprite != sokiraniSprite)
        {
            originalniSprite = tijelo.sprite;
        }

        if (tijelo != null && originalniSprite != null)
        {
            tijelo.sprite = originalniSprite;
        }

        currentState = HeadlessState.Stunned;

        if (animator != null) animator.enabled = false;

        if (tijelo != null) tijelo.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (vrijemeOsamucenosti > 0.1f)
        {
            yield return new WaitForSeconds(vrijemeOsamucenosti - 0.1f);
        }

        ResetirajZombija();
    }

    IEnumerator EfektStruje()
    {
        animacijaNogu.StopPlayback();
        if (noge != null) noge.SetActive(false);

        if (tijelo != null && tijelo.sprite != sokiraniSprite)
        {
            originalniSprite = tijelo.sprite;
        }

        if (tijelo != null) tijelo.material.color = Color.white;

        if (enemyAudio != null) enemyAudio.PlayShockSound();

        currentState = HeadlessState.Shocked;

        if (animator != null) animator.enabled = false;

        if (tijelo != null && sokiraniSprite != null)
        {
            tijelo.sprite = sokiraniSprite;
        }

        float vrijemeTreptaja = vrijemeStrujnogUdara / (brojTreptajaBoje * 2);

        for (int i = 0; i < brojTreptajaBoje; i++)
        {
            if (tijelo != null) tijelo.material.color = Color.blue;
            yield return new WaitForSeconds(vrijemeTreptaja);

            if (tijelo != null) tijelo.material.color = Color.black;
            yield return new WaitForSeconds(vrijemeTreptaja);
        }

        ResetirajZombija();
    }

    void ResetirajZombija()
    {
        if (tijelo != null)
        {
            tijelo.material.color = Color.white;
            if (originalniSprite != null)
            {
                tijelo.sprite = originalniSprite;
            }
        }

        if (animator != null) animator.enabled = true;

        currentState = HeadlessState.Active;

        noge.SetActive(true);
        animacijaNogu.Play("Idle", 0);

        ResetirajUbrzanje();

        if (enemyAudio != null) enemyAudio.PlayAttackShout();
    }

    private void OkreniSePremaIgracu()
    {
        if (igracMeta == null) return;

        Vector3 smjerDoIgraca = igracMeta.position - transform.position;
        smjerDoIgraca.z = 0;
        float kut = Mathf.Atan2(smjerDoIgraca.y, smjerDoIgraca.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, kut);
    }

    public void NapraviKorak()
    {
        if (currentState != HeadlessState.Active || uDometu || igracMeta == null || !aktivan) return;

        if (korakCoroutine != null) StopCoroutine(korakCoroutine);
        korakCoroutine = StartCoroutine(GladakKorak());
    }

    private IEnumerator GladakKorak()
    {
        Vector3 pocetnaPozicija = transform.position;
        Vector3 ciljanaPozicija = transform.position + transform.up * snagaZakoraka;

        float protekloVrijeme = 0f;

        while (protekloVrijeme < trajanjeZakoraka)
        {
            if (currentState != HeadlessState.Active) yield break;

            transform.position = Vector3.Lerp(pocetnaPozicija, ciljanaPozicija, protekloVrijeme / trajanjeZakoraka);

            protekloVrijeme += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetirajUbrzanje()
    {
        trenutnaBrzina = pocetnaBrzina;
        if (animacijaNogu != null) animacijaNogu.speed = 1f;
    }

    public HeadlessState GetCurrentState()
    {
        return currentState;
    }
}