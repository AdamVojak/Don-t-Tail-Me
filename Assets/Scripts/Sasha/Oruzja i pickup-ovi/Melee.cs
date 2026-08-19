using UnityEngine;

public class Melee : MonoBehaviour
{
    [Header("Postavke Štete i Punjenja")]
    public float baseDamage = 10f;
    public float vrijemePunjenja = 1.5f;

    private float[] multipliers = { 0.5f, 1.0f, 2.0f, 3.0f };
    private int chargeStep = 0;
    private float chargeTimer = 0f;


    [HideInInspector]
    public float currentMultiplier = 0.5f;

    [Header("Animacije i Komponente")]
    public Animator animacijaMelee;
    public GameObject melee;
    public Collider meleeCollider;
    public SashaController sashaControllerRef;

    [HideInInspector] public bool isAttacking = false;

    void Awake()
    {
        animacijaMelee = transform.parent.GetComponent<Animator>();
        if (meleeCollider == null)
        {
            meleeCollider = GetComponent<Collider>();
            if (meleeCollider == null) meleeCollider = GetComponentInChildren<Collider>();
        }

        if (meleeCollider != null) meleeCollider.enabled = false;

        currentMultiplier = multipliers[0];
    }


    private void OnEnable()
    {
        if (meleeCollider != null)
        {
            meleeCollider.enabled = false;
        }
        isAttacking = false;
    }

    private void OnDisable()
    {
        if (meleeCollider != null)
        {
            meleeCollider.enabled = false;
        }
        isAttacking = false;
    }

    public void PrisilnoPrekiniNapad()
    {
        if (meleeCollider != null)
        {
            meleeCollider.enabled = false;
        }

        isAttacking = false;

        if (animacijaMelee != null)
        {
            animacijaMelee.SetBool("udarac", false);
            animacijaMelee.Play("Idle");
        }
    }

    void Update()
    {
        if (!isAttacking && meleeCollider != null && meleeCollider.enabled)
        {
            meleeCollider.enabled = false;
        }

        if (!isAttacking)
        {
            if (chargeStep < multipliers.Length - 1)
            {
                chargeTimer += Time.deltaTime;

                if (chargeTimer >= vrijemePunjenja)
                {
                    chargeStep++;
                    chargeTimer = 0f;

                    currentMultiplier = multipliers[chargeStep];
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            animacijaMelee.SetBool("udarac", true);
            animacijaMelee.Play("Udarac", 0, 0f);
        }
    }

    public void UkljuciHitbox()
    {
        isAttacking = true;
        SFX.zvucniEfekti.ZvukZamaha.Play();
        if (meleeCollider != null) meleeCollider.enabled = true;
    }

    public void IskljuciHitbox()
    {
        isAttacking = false;
        if (meleeCollider != null) meleeCollider.enabled = false;
        if (animacijaMelee != null) animacijaMelee.SetBool("udarac", false);

        chargeStep = 0;
        chargeTimer = 0f;
        currentMultiplier = multipliers[0];

    }

    private void OnTriggerEnter(Collider collision)
    {

        if (!this.enabled || !isAttacking) return;


        var damageable = collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            int finalDamage = Mathf.RoundToInt(baseDamage * currentMultiplier);
            damageable.TakeDamage(finalDamage);
        }
    }
}