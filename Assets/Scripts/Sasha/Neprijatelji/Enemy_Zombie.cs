using UnityEngine;
using System.Collections;
public enum ZombieState
{
    Active,
    Stunned,
    Shocked,
    Dead
}
public class Zombi : MonoBehaviour, IDamageable
{

    [Header("Postavke Brzine i Juriša")]
[SerializeField] float pocetnaBrzina = 6f;      // Brzina čim se stvori
    [SerializeField] float maksimalnaBrzina = 14f;  // Ekstremna brzina trka
    [SerializeField] float brzinaUbrzavanja = 20f;  // Koliko brzo skače na max brzinu
    [HideInInspector] public float zadnjeVrijemeTranzicije = 0f; // Dodati na vrh klase Zombi

    [Header("Postavke Zakoraka (Impuls)")]
    [SerializeField] float snagaZakoraka = 0.3f;    // Dodatni mali "push" pri koraku
    [SerializeField] float trajanjeZakoraka = 0.06f;
    private float trenutnaBrzina;
    private Coroutine korakCoroutine;
    [SerializeField] private SashaController sashaControllerRef;
    [SerializeField] private GameManager gameManagerRef;
    public bool aktivan;

    [Header("Postavke Izbjegavanja Prepreka")]
    [SerializeField] private LayerMask obstacleLayerMask; // Dodijelite slojeve "Obstacle" i "Terrain" u Inspectoru
    [SerializeField] private float detectionDistance = 1.5f;
    private System.Collections.Generic.Queue<Vector3> tranzicijskeTocke = new System.Collections.Generic.Queue<Vector3>();
    private Vector3 trenutnaMetaKretanja; // Ovu smo dodali ranije za rotaciju

    [Header("Postavke Rotacije")]
    [SerializeField] private float brzinaRotacije = 5.0f; // Manja vrijednost = sporije okretanje (veće kašnjenje)

    private Rigidbody rb;
    private Vector3 moveDirection;

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
    public GameObject deadZombiePrefab;

    [Header("NOGE")]
    public Animator animacijaNogu;
    public GameObject noge;

    private ZombieState currentState = ZombieState.Active;

    private Transform igracMeta;
    private Sprite originalniSprite;
    private Animator animator;
    private float zadnjeVrijemeStete = -1f;

    [Header("Audio")]
    [SerializeField] private ZombieAudio zombieAudio;

    void Awake()
    {
        DohvatiIgraca();
        if (tijelo == null) tijelo = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (gameManagerRef == null) gameManagerRef = FindFirstObjectByType<GameManager>();
        if (zombieAudio == null) zombieAudio = GetComponent<ZombieAudio>();

        rb = GetComponent<Rigidbody>();

        if (igracMeta != null) trenutnaMetaKretanja = igracMeta.position;
        OkreniSePremaMeti(trenutnaMetaKretanja);
    }

    void OnEnable()
    {
        currentState = ZombieState.Active;
        DohvatiIgraca();

        if (igracMeta != null) trenutnaMetaKretanja = igracMeta.position;
        OkreniSePremaMeti(trenutnaMetaKretanja);

        ResetirajUbrzanje();
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

    void FixedUpdate()
    {
        bool sashaMrtva = (sashaControllerRef == null || sashaControllerRef.currentState == SashaController.SashaState.Dead);
        bool loading = (gameManagerRef != null && gameManagerRef.isLoading);

        if (currentState == ZombieState.Active && aktivan && !uDometu && !sashaMrtva && !loading && rb != null)
        {
            Vector3 targetVelocity = moveDirection * trenutnaBrzina;
            targetVelocity.z = rb.linearVelocity.z;
            rb.linearVelocity = targetVelocity;
        }
        else if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, 0, rb.linearVelocity.z);
        }
    }

    private void OkreniSePremaMeti(Vector3 meta)
    {
        Vector3 smjerGledanja = meta - transform.position;

        if (moveDirection != Vector3.zero)
        {
            smjerGledanja = moveDirection;
        }

        smjerGledanja.z = 0;

        if (smjerGledanja != Vector3.zero)
        {
            float ciljaniKut = Mathf.Atan2(smjerGledanja.y, smjerGledanja.x) * Mathf.Rad2Deg - 90f;
            Quaternion ciljanaRotacija = Quaternion.Euler(0f, 0f, ciljaniKut);

            float stvarnaBrzinaRotacije = (brzinaRotacije > 0f) ? brzinaRotacije : 15f;

            transform.rotation = Quaternion.Slerp(transform.rotation, ciljanaRotacija, stvarnaBrzinaRotacije * Time.deltaTime);
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

        if (!aktivan || sashaMrtva || currentState != ZombieState.Active || igracMeta == null || loading)
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


        // 1. PROVJERA DOMETA I AKTIVACIJA ZAMAHA (Originalna logika)
        if (triggerArea != null && igracMeta != null)
        {
            bool sashaJeBlizu = triggerArea.bounds.Contains(igracMeta.position);

            if (sashaJeBlizu && !uDometu)
            {
                uDometu = true;
                if (noge != null) noge.SetActive(false);
                if (animator != null) animator.SetBool("uDometu", true);

                IzvediZamah();
            }
            else if (!sashaJeBlizu && uDometu)
            {
                uDometu = false;
                if (noge != null) noge.SetActive(true);
                if (animator != null) animator.SetBool("uDometu", false);
                // Ovdje NE gasimo animaciju na silu - puštamo je da sama dođe do KrajZamaha()
            }
        }

        // 2. KONTROLA NOGU
        if (uDometu)
        {
            if (noge != null) noge.SetActive(false);
            trenutnaMetaKretanja = igracMeta.position;
        }
        else
        {
            if (noge != null) noge.SetActive(true);
            animacijaNogu.Play("Idle", 0);
        }

        // 3. KRETANJE (Izvršava se kad god zombi NIJE u dometu)
        if (!uDometu)
        {
            Vector3 trenutnaMeta = igracMeta.position;

            // TRANZICIJE
            if (tranzicijskeTocke.Count > 0)
            {
                trenutnaMeta = tranzicijskeTocke.Peek();
                if (Vector3.Distance(transform.position, trenutnaMeta) < 0.5f)
                {
                    tranzicijskeTocke.Dequeue();
                }
            }
            else
            {
                // MRVICE / SASHA
                Vector3 smjerDoSase = igracMeta.position - transform.position;
                float udaljenostDoSase = smjerDoSase.magnitude;
                bool sasaBlokirana = Physics.Raycast(transform.position, smjerDoSase.normalized, udaljenostDoSase, obstacleLayerMask);

                if (sasaBlokirana && SashaPath.Instance != null && SashaPath.Instance.points.Count > 0)
                {
                    bool nasaoVidljivuTocku = false;
                    for (int i = SashaPath.Instance.points.Count - 1; i >= 0; i--)
                    {
                        Vector3 tockaPatha = SashaPath.Instance.points[i];
                        Vector3 smjerDoTocke = tockaPatha - transform.position;
                        if (!Physics.Raycast(transform.position, smjerDoTocke.normalized, smjerDoTocke.magnitude, obstacleLayerMask))
                        {
                            trenutnaMeta = tockaPatha;
                            nasaoVidljivuTocku = true;
                            break;
                        }
                    }
                    if (!nasaoVidljivuTocku) trenutnaMeta = SashaPath.Instance.points[0];
                }
            }

            trenutnaMetaKretanja = trenutnaMeta;

            Vector3 desiredDirection = (trenutnaMeta - transform.position);
            desiredDirection.z = 0f;
            desiredDirection.Normalize();

            // PRAVILO DESNE RUKE: Lijevi zombi potpuno staje i čeka
            float ciljanaBrzina = maksimalnaBrzina;
            Collider[] nearby = Physics.OverlapSphere(transform.position, 1.0f);

            foreach (var col in nearby)
            {
                if (col.gameObject != gameObject && col.GetComponent<Zombi>() != null)
                {
                    Vector3 dirToOther = col.transform.position - transform.position;
                    dirToOther.z = 0f;
                    Vector3 myRight = Vector3.Cross(desiredDirection, Vector3.forward).normalized;

                    // Ako je drugi zombi s naše desne strane, mi smo lijevo -> STAJEMO NA MJESTU (0 brzina)
                    if (Vector3.Dot(dirToOther, myRight) > 0.1f)
                    {
                        ciljanaBrzina = 0f;
                        break;
                    }
                }
        }

            trenutnaBrzina = Mathf.MoveTowards(trenutnaBrzina, ciljanaBrzina, brzinaUbrzavanja * Time.deltaTime);
            moveDirection = FindAvoidanceDirection(desiredDirection, Vector3.Distance(transform.position, trenutnaMeta));

            if (animacijaNogu != null)
            {
                animacijaNogu.speed = Mathf.Clamp(trenutnaBrzina / 5f, 1f, 2.5f);
            }
        }

        // 4. ROTACIJA
        if (aktivan && currentState == ZombieState.Active && igracMeta != null && !loading)
        {
            OkreniSePremaMeti(trenutnaMetaKretanja);
        }
    }

    public void PostaviTranziciju(Vector3 ulaz, Vector3 izlaz)
    {
        tranzicijskeTocke.Clear();
        tranzicijskeTocke.Enqueue(ulaz);
        tranzicijskeTocke.Enqueue(izlaz);
    }

    public void TakeDamage(int amount, DamageType damageType = DamageType.Physical)
    {
        if (zombieAudio != null)
        {
            if (damageType == DamageType.Physical)
            {
                zombieAudio.PlayPajserHit();
            }
            else if (damageType == DamageType.Electric)
            {
                zombieAudio.PlayElectricMeleeHit();
            }

            zombieAudio.PlayHurtSound();
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
        if (currentState == ZombieState.Dead) return;

        // Ako cooldown još traje, odmah prekidamo metodu
        if (Time.time < zadnjeVrijemeStete + stetaCooldown) return;

        if (other.CompareTag("Projectile"))
        {
            Projektil projectile = other.GetComponent<Projektil>();
            if (projectile != null)
            {
                Destroy(other.gameObject);
                zombieAudio.PlayHurtSound();
                PrimiUdarac(projectile.damage);
            }
        }

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                Destroy(other.gameObject);
                zombieAudio.PlayHurtSound();
                PrimiUdarac(bullet.damage);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentState == ZombieState.Dead) return;

        if (other.CompareTag("Sasha") || (igracMeta != null && other.transform == igracMeta))
        {
            uDometu = false;
            if (noge != null) noge.SetActive(true);
            if (animator != null) animator.SetBool("uDometu", false);
        }
    }

    void IzvediZamah()
    {
        if (animator == null || currentState != ZombieState.Active || aktivan == false) return;

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

        if (currentState != ZombieState.Active) return;

        if (uDometu)
        {
            IzvediZamah(); // Sasha je još u dometu -> započni novi zamah (Loop)
        }
        else
        {
            // Sasha je izašla -> zamah je gotov, vrati gornji dio u Idle (zombi već hoda)
            if (animator != null) animator.Play("Idle", 0, 0f);
        }
    }

    void PrimiUdarac(int steta)
    {
        if (currentState == ZombieState.Dead) return;

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
        if (currentState == ZombieState.Dead) return;

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
        currentState = ZombieState.Dead;
        if (deadZombiePrefab != null)
        {
            Instantiate(deadZombiePrefab, transform.position, transform.rotation);
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

        currentState = ZombieState.Stunned;

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

        if (zombieAudio != null) zombieAudio.PlayShockSound();

        currentState = ZombieState.Shocked;

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

        currentState = ZombieState.Active;

        noge.SetActive(true);
        animacijaNogu.Play("Idle", 0);

        ResetirajUbrzanje();
    }

    private Vector3 FindAvoidanceDirection(Vector3 desiredDir, float distanceToTarget)
    {
        float checkDistance = Mathf.Min(detectionDistance, distanceToTarget);
        RaycastHit hit;
        bool hasObstacle = Physics.Raycast(transform.position, desiredDir, out hit, checkDistance, obstacleLayerMask);

        Vector3 bypassDir = desiredDir;

        // Izbjegava SAMO zidove (nema više kruženja oko drugih zombija)
        if (hasObstacle)
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(desiredDir, hit.normal);
            slideDir.z = 0f;

            if (slideDir != Vector3.zero)
            {
                bypassDir = slideDir.normalized;
            }
            else
            {
                bypassDir = hit.normal;
            }
        }

        return bypassDir.normalized;
    }

    private void OkreniSePremaIgracu()
    {
        if (igracMeta == null) return;

        Vector3 smjerDoIgraca = igracMeta.position - transform.position;
        smjerDoIgraca.z = 0;

        if (smjerDoIgraca != Vector3.zero)
        {
            float ciljaniKut = Mathf.Atan2(smjerDoIgraca.y, smjerDoIgraca.x) * Mathf.Rad2Deg - 90f;
            Quaternion ciljanaRotacija = Quaternion.Euler(0f, 0f, ciljaniKut);


            transform.rotation = Quaternion.Slerp(transform.rotation, ciljanaRotacija, brzinaRotacije * Time.deltaTime);
        }
    }

    public void NapraviKorak()
    {
        if (currentState != ZombieState.Active || igracMeta == null || !aktivan) return;

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
            if (currentState != ZombieState.Active) yield break;

            if (rb != null)
            {
                rb.MovePosition(Vector3.Lerp(pocetnaPozicija, ciljanaPozicija, protekloVrijeme / trajanjeZakoraka));
            }

            protekloVrijeme += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetirajUbrzanje()
    {
        trenutnaBrzina = pocetnaBrzina;
        if (animacijaNogu != null) animacijaNogu.speed = 1f;
    }

    public ZombieState GetCurrentState()
    {
        return currentState;
    }
}