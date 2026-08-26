using UnityEngine;
using System.Collections;

public class GumbSasha : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite normalan;
    public Sprite pritisnut;

    [Header("Postavke")]
    public float trajanjePritiska = 0.5f; // Minimalno vrijeme koliko gumb ostaje stisnut
    public bool aktiviran = false;

    [Header("Audio")]
    [SerializeField] private ButtonAudio buttonAudio;

    private int tijelaNaGumbu = 0; // Broji stoji li Sasha fizički na gumbu
    private bool isPressed = false;
    private Coroutine releaseCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (buttonAudio == null) buttonAudio = GetComponent<ButtonAudio>();
    }

    void Start()
    {
        spriteRenderer.sprite = normalan;
        aktiviran = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. MELE OROŽJA I METCI (Trenutni impuls)
        if (other.CompareTag("Melee") || other.CompareTag("Pajser") || other.CompareTag("Projectile") || other.CompareTag("Bullet"))
        {
            if (other.CompareTag("Projectile") || other.CompareTag("Bullet"))
            {
                Destroy(other.gameObject);
            }

            PritisniGumb();
        }
        // 2. SASHA ILI FIZIČKI PREDMETI (Ostaju na gumbu)
        else if (other.CompareTag("Sasha"))
        {
            tijelaNaGumbu++;
            PritisniGumb();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            tijelaNaGumbu = Mathf.Max(0, tijelaNaGumbu - 1);
            // Ako je Sasha sišao, pokreni proceduru vraćanja ako već nije u tijeku
            if (isPressed && releaseCoroutine == null)
            {
                releaseCoroutine = StartCoroutine(VratiGumbRoutine());
            }
        }
    }

    private void PritisniGumb()
    {
        if (!isPressed)
        {
            isPressed = true;
            spriteRenderer.sprite = pritisnut;

            // ZVUK PRITISKA (Preko unutra):
            if (buttonAudio != null) buttonAudio.PlayPressIn();

            Debug.Log("Gumb je pritisnut!");
        }

        // Pokreni ili restartaj odbrojavanje za vraćanje
        if (releaseCoroutine != null) StopCoroutine(releaseCoroutine);
        releaseCoroutine = StartCoroutine(VratiGumbRoutine());
    }

    private IEnumerator VratiGumbRoutine()
    {
        // Čekaj minimalno definirano vrijeme
        yield return new WaitForSeconds(trajanjePritiska);

        // Čekaj sve dok igrač fizički stoji na gumbu
        while (tijelaNaGumbu > 0)
        {
            yield return null;
        }

        // Vraćanje gumba u normalno stanje
        isPressed = false;
        aktiviran = !aktiviran;
        spriteRenderer.sprite = normalan;

        // ZVUK VRAĆANJA (Prema van):
        if (buttonAudio != null) buttonAudio.PlayPopOut();

        Debug.Log("Gumb se vratio!");
        releaseCoroutine = null;
    }
}