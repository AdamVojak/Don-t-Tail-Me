using UnityEngine;

public class Pajser : MonoBehaviour
{
    public float baseDamage = 10f;

    private Animator animacijaMelee;

    [Header("Poveži u Inspectoru")]
    public Collider meleeCollider;

    void Awake()
    {
        animacijaMelee = GetComponent<Animator>();

        if (meleeCollider != null) meleeCollider.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (meleeCollider != null) meleeCollider.enabled = false;

            animacijaMelee.Play("Udarac_Pajser", 0);

            animacijaMelee.SetBool("udarac", true);
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

        if (animacijaMelee != null)
        {
            animacijaMelee.SetBool("udarac", false);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        var damageable = collision.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(Mathf.RoundToInt(baseDamage));
            //if (meleeCollider != null) meleeCollider.enabled = false;
        }
    }
}