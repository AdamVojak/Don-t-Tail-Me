using UnityEngine;
using System.Collections;

public class GumbSasha : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite normalan;
    public Sprite pritisnut;

    public float cekanjeSek = 0.25f;


    public bool aktiviran;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normalan;
        aktiviran = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha") || other.CompareTag("Melee") || other.CompareTag("Gun") || other.CompareTag("Minigun") || other.CompareTag("Projectile") || other.CompareTag("Bullet") || other.CompareTag("Pajser"))
        {
            spriteRenderer.sprite = pritisnut;
            Debug.Log("Gumb pritisnut!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha") || other.CompareTag("Melee") || other.CompareTag("Gun") || other.CompareTag("Minigun") || other.CompareTag("Projectile") || other.CompareTag("Bullet") || other.CompareTag("Pajser"))
        {
            spriteRenderer.sprite = pritisnut;
            Debug.Log("Gumb pritisnut!");
            if (CompareTag("Projectile") || CompareTag("Bullet"))
            {
                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha") || other.CompareTag("Melee") || other.CompareTag("Gun") || other.CompareTag("Minigun") || other.CompareTag("Projectile") || other.CompareTag("Bullet") || other.CompareTag("Pajser"))
        {
            Čekanje();
            spriteRenderer.sprite = normalan;
            aktiviran = !aktiviran;
            Debug.Log("Gumb otpušten!");
        }
    }

    private IEnumerator Čekanje()
    {
        yield return new WaitForSeconds(cekanjeSek);
    }
}
