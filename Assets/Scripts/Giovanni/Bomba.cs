using UnityEngine;

public class Bomba : MonoBehaviour
{
    [Header("Postavke Upozorenja")]
    [SerializeField] private int minUpozorenja = 3;
    [SerializeField] private int maxUpozorenja = 6;
    private int preostaloUpozorenja;

    [Header("Cooldown Trzanja Lanca")]
    [Tooltip("Koliko sekundi mora proći prije nego lanac može ponovno zveckati")]
    [SerializeField] private float cooldownTrzanja = 1.0f;
    private float zadnjeTrzanje = -1f;

    [Header("Vizualni Efekt Eksplozije (Opcionalno)")]
    //[SerializeField] private GameObject explosionEffectPrefab; // Prefab čestica/vatre ako ga imaš

    private GiovanniController giovanniControllerRef;
    private bool isExploded = false;

    void Start()
    {
        if (giovanniControllerRef == null)
        {
            giovanniControllerRef = FindFirstObjectByType<GiovanniController>();
        }

        // NOVO: Nasumičan broj upozorenja (od 3 do 6 uključivo)
        preostaloUpozorenja = Random.Range(minUpozorenja, maxUpozorenja + 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isExploded) return;

        // Provjera je li Giovanni dotaknuo senzor/lanac
        if (other.CompareTag("Giovanni"))
        {
            // Provjera cooldowna da se ne potroše sva upozorenja u jednoj milisekundi
            if (Time.time < zadnjeTrzanje + cooldownTrzanja) return;

            zadnjeTrzanje = Time.time;

            if (preostaloUpozorenja > 1)
            {
                preostaloUpozorenja--;

                // ZVUK TRZAJA SENZORA
                if (SFX.zvucniEfekti != null && SFX.zvucniEfekti.ZvukNeuspjehaUdarcaVent != null)
                {
                    SFX.zvucniEfekti.ZvukNeuspjehaUdarcaVent.Play();
                }

                Debug.Log("Trzaj senzora na bombi! Preostalo dodira: " + preostaloUpozorenja);
            }
            else
            {
                // Nema više upozorenja -> BUM!
                Explode();
            }
        }
    }

    void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        Debug.Log("BUM! Bomba je eksplodirala.");

        // Stvori efekt eksplozije na poziciji bombe
        /*if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }*/

        // Poveznica s Death Screenom: Šaljemo 0 (Bomba) kako bi se upalio točan ekran smrti!
        if (giovanniControllerRef != null)
        {
            giovanniControllerRef.Die(0);
        }

        Destroy(gameObject);
    }
}