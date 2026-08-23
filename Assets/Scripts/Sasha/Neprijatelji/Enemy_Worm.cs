using UnityEngine;
using System.Collections;

public enum WormState
{
    Active,
    Stunned,
    Shocked,
    Dead
}

public class Enemy_Worm : MonoBehaviour, IDamageable
{
    [Header("Postavke Kretanja")]
    [SerializeField] float brzina = 3f;
    [SerializeField] private SashaController sashaControllerRef;
    public bool aktivan;

    [Header("Postavke Zdravlja")]
    [SerializeField] int health = 30;
    [SerializeField] float vrijemeOsamucenosti = 0.5f;
    [SerializeField] float stetaCooldown = 0.15f;

    [Header("Postavke Strujnog Udara (Melee Stun)")]
    [SerializeField] float vrijemeStrujnogUdara = 1f;
    [SerializeField] Sprite sokiraniSprite;
    [SerializeField] int brojTreptajaBoje = 8;

    private SpriteRenderer tijelo;
    private WormState currentState = WormState.Active;
    private Transform igracMeta;
    private Sprite originalniSprite;
    private Animator animator;
    private float zadnjeVrijemeStete = -1f;
    private bool mozeSeKretati = false;

    void Awake()
    {
        DohvatiIgraca();

        tijelo = GetComponent<SpriteRenderer>();
        if (tijelo == null)
        {
            Debug.LogError("SpriteRenderer nije pronađen na objektu: " + gameObject.name);
        }

        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        currentState = WormState.Active;
        mozeSeKretati = true;

        DohvatiIgraca();

        if (animator != null)
        {
            animator.SetBool("Move", true);
            animator.Play("Move", 0, 0f);
        }
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
    }

    public void PocetakKoraka()
    {
        if (currentState == WormState.Active && aktivan)
        {
            mozeSeKretati = true;
        }
    }

    public void KrajKoraka()
    {
        mozeSeKretati = false;
    }

    void Update()
    {
        if (sashaControllerRef == null)
        {
            DohvatiIgraca();
        }

        if (sashaControllerRef != null)
        {
            aktivan = sashaControllerRef.isControlled;
        }
        else
        {
            aktivan = false;
        }
        if (aktivan == false || currentState != WormState.Active || igracMeta == null)
        {
            if (animator != null)
            {
                animator.enabled = false;
                animator.SetBool("Move", false);

            }
            Debug.Log("Zombi stoji jer Sasha nije kontrolirana");
            mozeSeKretati = false;
            return;
        }

        if (animator != null)
        {
                animator.enabled = true;
                animator.SetBool("Move", true);
        }

        Vector3 smjerDoIgraca = igracMeta.position - transform.position;
        smjerDoIgraca.z = 0;

        float kut = Mathf.Atan2(smjerDoIgraca.y, smjerDoIgraca.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, kut);

        if (mozeSeKretati)
        {
            Vector3 targetPosition = new Vector3(igracMeta.position.x, igracMeta.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, brzina * Time.deltaTime);
        }
    }

    public void TakeDamage(int amount, DamageType damageType = DamageType.Physical)
    {
        SFX.zvucniEfekti.ZvukUdarca.Play();
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
        if (currentState == WormState.Dead) return;

        if (Time.time < zadnjeVrijemeStete + stetaCooldown) return;

        if (other.CompareTag("Projectile"))
        {
            Projektil projectile = other.GetComponent<Projektil>();
            if (projectile != null)
            {
                PrimiUdarac(projectile.damage);
            }
        }

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                PrimiUdarac(bullet.damage);
            }
        }
    }

    void PrimiUdarac(int steta)
    {
        if (currentState == WormState.Dead) return;

        zadnjeVrijemeStete = Time.time;
        health -= steta;
        mozeSeKretati = false;

        if (currentState == WormState.Shocked || currentState != WormState.Stunned)
        {
            StopAllCoroutines();
            StartCoroutine(EfektUdarca());
        }

        if (health <= 0) Umri();
    }

    void PrimiStrujniUdar(int steta)
    {
        if (currentState == WormState.Dead) return;

        if (currentState == WormState.Shocked)
        {
            health -= steta;
            if (health <= 0) Umri();
            return;
        }

        zadnjeVrijemeStete = Time.time;
        health -= steta;
        mozeSeKretati = false;

        StopAllCoroutines();
        StartCoroutine(EfektStruje());

        if (health <= 0) Umri();
    }

    void Umri()
    {
        currentState = WormState.Dead;
        mozeSeKretati = false;

        if (animator != null)
        {
            animator.SetBool("Move", false);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        this.enabled = false;
        Destroy(gameObject, 0.5f);
    }

    IEnumerator EfektUdarca()
    {
        if (tijelo != null && tijelo.sprite != sokiraniSprite)
        {
            originalniSprite = tijelo.sprite;
        }

        currentState = WormState.Stunned;

        if (animator != null) animator.enabled = false;

        if (tijelo != null && sokiraniSprite != null)
        {
            tijelo.sprite = sokiraniSprite;
            tijelo.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);
        if (tijelo != null) tijelo.color = Color.white;

        if (vrijemeOsamucenosti > 0.1f)
        {
            yield return new WaitForSeconds(vrijemeOsamucenosti - 0.1f);
        }

        Resetiraj();
    }

    IEnumerator EfektStruje()
    {
        if (tijelo != null && tijelo.sprite != sokiraniSprite)
        {
            originalniSprite = tijelo.sprite;
        }

        SFX.zvucniEfekti.ZvukElektrosoka.Play();

        currentState = WormState.Shocked;

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

        Resetiraj();
    }

    void Resetiraj()
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

        currentState = WormState.Active;
    }

    public WormState GetCurrentState()
    {
        return currentState;
    }
}