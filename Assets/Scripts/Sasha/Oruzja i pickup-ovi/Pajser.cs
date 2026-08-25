using UnityEngine;

public class Pajser : MonoBehaviour
{
    public float baseDamage = 30f;

    private Animator animacijaMelee;

    [Header("Poveži u Inspectoru")]
    public Collider meleeCollider;

    private SashaAudio sashaAudio;

    void Awake()
    {
        sashaAudio = GetComponentInParent<SashaAudio>();
        animacijaMelee = GetComponent<Animator>();

        if (meleeCollider != null) meleeCollider.enabled = false;
    }

    void Update()
    {
        // --- SIGURNOSNA BRAVA ---
        // Provjeravamo vrti li se trenutno animacija udarca
        bool isAttacking = false;
        if (animacijaMelee != null)
        {
            isAttacking = animacijaMelee.GetCurrentAnimatorStateInfo(0).IsName("Udarac_Pajser");
        }

        // Ako ne napadamo, a collider je ostao upaljen (npr. zbog spama) -> ugasi ga!
        if (!isAttacking && meleeCollider != null && meleeCollider.enabled)
        {
            meleeCollider.enabled = false;
        }
        // ------------------------

        if (Input.GetMouseButtonDown(0))
        {
            // OVDJE JE BILA GREŠKA: Collider se mora UPALITI kad klikneš, a ne ugasiti!
            if (meleeCollider != null) meleeCollider.enabled = true;
            if (sashaAudio != null) sashaAudio.PlaySwing();

            if (animacijaMelee != null)
            {
                animacijaMelee.SetBool("udarac", true);

                // Korištenje '-1' osigurava da Animator UVIJEK posluša ovu komandu i krene od nule, 
                // čak i ako brzo spammaš klikove!
                animacijaMelee.Play("Udarac_Pajser", -1, 0f);
            }
        }
    }

    public void PrisilnoPrekiniNapad()
    {
        if (meleeCollider != null) meleeCollider.enabled = false;
        if (animacijaMelee != null)
        {
            animacijaMelee.SetBool("udarac", false);
            animacijaMelee.Play("Pajser");
        }
    }

    public void ZavrsiUdarac()
    {
        // Gasimo collider na kraju udarca
        if (meleeCollider != null) meleeCollider.enabled = false;

        if (animacijaMelee != null)
        {
            animacijaMelee.SetBool("udarac", false);

            // PRISILNI POVRATAK U IDLE: 
            // Ovo garantira da animacija nikada neće ostati zaleđena na zadnjem frameu!
            animacijaMelee.Play("Pajser", -1, 0f);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!this.enabled) return;

        if (meleeCollider == null || !meleeCollider.enabled)
        {
            return;
        }

        var damageable = collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log("Pajser je uspješno udario zombija!");
            damageable.TakeDamage(Mathf.RoundToInt(baseDamage), DamageType.Physical);

            // Gasimo collider nakon prvog pogotka da ne udari 5 puta u jednom zamahu
            if (meleeCollider != null) meleeCollider.enabled = false;
        }
    }
}