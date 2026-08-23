using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WormsState
{
    Active,
    Stunned,
    Shocked,
    Dead
}

public class Worms : MonoBehaviour, IDamageable
{
    [Header("Postavke Kretanja (Korak po korak)")]
    [SerializeField] float brzina = 1f;
    [SerializeField] float vrijemeKoraka = 0.6f;
    [SerializeField] float vrijemeStajanja = 1.0f;
    [SerializeField] private SashaController sashaControllerRef;
    public bool aktivan;

    [Header("Postavke zaustavljanja")]
    [SerializeField] private Collider triggerArea;
    private bool uDometu = false;

    [Header("Worms - Spawning")]
    [SerializeField] GameObject maliCrvPrefab;
    [SerializeField] float intervalSpawna = 4f;
    [SerializeField] float udaljenostSpawna = 1.3f;
    [SerializeField] int maksimalnoCrvi = 10;

    [Header("Worms - Smrt")]
    [SerializeField] GameObject zemljaKupPrefab;

    [Header("Postavke Zdravlja")]
    [SerializeField] int health = 80;
    [SerializeField] float vrijemeOsamucenosti = 0.5f;
    [SerializeField] float stetaCooldown = 0.15f;

    [Header("Postavke Strujnog Udara (Melee Stun)")]
    [SerializeField] float vrijemeStrujnogUdara = 1f;
    [SerializeField] Sprite sokiraniSprite;
    [SerializeField] int brojTreptajaBoje = 8;

    [Header("Reference")]
    [SerializeField] private string playerTag = "Sasha";

    private SpriteRenderer tijelo;
    private WormsState currentState = WormsState.Active;
    private Transform igracMeta;
    private Sprite originalniSprite;
    private Animator animator;
    private float zadnjeVrijemeStete = -1f;

    private bool mozeSeKretati = false;
    private Coroutine kretanjeCoroutine;
    private float tajmerSpawna;
    private List<GameObject> ziviCrvi = new List<GameObject>();
    private Bullet bullet;

    void Awake()
    {
        DohvatiIgraca();

        if (sashaControllerRef == null)
        {
            GameObject igracObjekt = GameObject.FindGameObjectWithTag(playerTag);
            if (igracObjekt != null)
            {
                sashaControllerRef = igracObjekt.GetComponent<SashaController>();
            }
        }

        tijelo = GetComponent<SpriteRenderer>();
        if (tijelo == null)
        {
            Debug.LogError("SpriteRenderer nije pronađen na Worms objektu: " + gameObject.name);
        }

        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (bullet == null)
        {
            bullet = FindFirstObjectByType<Bullet>();
        }
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

    void OnEnable()
    {
        currentState = WormsState.Active;
        mozeSeKretati = false;

        DohvatiIgraca();

        RestartajKretanje();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        kretanjeCoroutine = null;
    }

    IEnumerator KretanjePetlja()
    {
        while (true)
        {
            mozeSeKretati = true;
            yield return new WaitForSeconds(vrijemeKoraka);

            mozeSeKretati = false;
            yield return new WaitForSeconds(vrijemeStajanja);
        }
    }

    void RestartajKretanje()
    {
        if (kretanjeCoroutine != null) StopCoroutine(kretanjeCoroutine);
        kretanjeCoroutine = StartCoroutine(KretanjePetlja());
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

    void Update()
    {
        if (sashaControllerRef == null || igracMeta == null)
        {
            GameObject igracObjekt = GameObject.FindGameObjectWithTag(playerTag);
            if (igracObjekt != null)
            {
                igracMeta = igracObjekt.transform;
                sashaControllerRef = igracObjekt.GetComponent<SashaController>();
            }
        }

        if (sashaControllerRef != null)
        {
            aktivan = sashaControllerRef.isControlled;
        }


        if (aktivan == false || currentState != WormsState.Active || igracMeta == null || sashaControllerRef.currentState == SashaController.SashaState.Dead)
        {
            mozeSeKretati = false;
            return;
        }



        Vector3 smjerDoIgraca = igracMeta.position - transform.position;
        smjerDoIgraca.z = 0;
        float kut = Mathf.Atan2(smjerDoIgraca.y, smjerDoIgraca.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, kut);

        if (triggerArea != null && igracMeta != null)
        {
            uDometu = triggerArea.bounds.Contains(igracMeta.position);
        }

        if (mozeSeKretati && !uDometu)
        {
            Vector3 targetPosition = new Vector3(igracMeta.position.x, igracMeta.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, brzina * Time.deltaTime);
        }


        if (maliCrvPrefab != null)
        {
            tajmerSpawna += Time.deltaTime;
            if (tajmerSpawna >= intervalSpawna)
            {
                PokusajSpawnatiCrve();
                tajmerSpawna = 0f;
            }
        }
    }

    void PokusajSpawnatiCrve()
    {
        ziviCrvi.RemoveAll(crv => crv == null);

        if (ziviCrvi.Count + 4 <= maksimalnoCrvi)
        {
            float[] kutovi = { 0f, 90f, 180f, 270f };

            foreach (float kut in kutovi)
            {
                float radijan = kut * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(radijan), Mathf.Sin(radijan), 0f) * udaljenostSpawna;
                Vector3 spawnPozicija = transform.position + offset;

                GameObject noviCrv = Instantiate(maliCrvPrefab, spawnPozicija, Quaternion.identity);
                ziviCrvi.Add(noviCrv);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentState == WormsState.Dead) return;

        if (Time.time < zadnjeVrijemeStete + stetaCooldown) return;

        if (other.CompareTag("Projectile"))
        {
            Projektil projectile = other.GetComponent<Projektil>();
            if (projectile != null)
            {
                PrimiUdarac(projectile.damage);
                Destroy(other.gameObject);
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
                    PrimiUdarac(bullet.damage);
                }
            }
        }
    }

    void PrimiUdarac(int steta)
    {
        if (currentState == WormsState.Dead) return;

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
        if (currentState == WormsState.Dead) return;


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
        currentState = WormsState.Dead;
        mozeSeKretati = false;

        if (zemljaKupPrefab != null)
        {
            Instantiate(zemljaKupPrefab, transform.position, Quaternion.identity);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        this.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    IEnumerator EfektUdarca()
    {
        if (tijelo != null && tijelo.sprite != sokiraniSprite)
        {
            originalniSprite = tijelo.sprite;
        }

        if (tijelo != null && originalniSprite != null)
        {
            tijelo.sprite = originalniSprite;
        }

        currentState = WormsState.Stunned;

        if (animator != null) animator.enabled = false;

        if (tijelo != null) tijelo.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

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

        if (tijelo != null) tijelo.color = Color.white;

        SFX.zvucniEfekti.ZvukElektrosoka.Play();

        currentState = WormsState.Shocked;

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

        currentState = WormsState.Active;
        RestartajKretanje();
    }

    public WormsState GetCurrentState()
    {
        return currentState;
    }
}