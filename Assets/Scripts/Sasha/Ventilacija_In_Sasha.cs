using UnityEngine;

public class Ventilacija_In_Sasha : MonoBehaviour
{
    [Header("Vizualni elementi ventilacije")]
    [SerializeField] private GameObject otvorenSprite;
    [SerializeField] private GameObject zatvorenSprite;

    [Header("Pozicije za interakciju")]
    public Transform interactionAreaCenter;
    public float kutGledanjaSashe = -90f;

    [Header("Bitne skripte")]
    public Melee melee;
    public SFX zvucniEfekti;

    public bool jeOtvorena = false;
    public bool JeOtvorena => jeOtvorena;

    [Header("Primatelji (Izlazne ventilacije)")]
    public Ventilacija_Out_Miranda mirandinaVentilacija;
    public Ventilacija_Out_Giovanni giovanniVentilacija; // NOVO: Referenca na Giovannijev Out Vent

    // Izmijenjena funkcija: Vraća true ako je uspješno poslano, false ako je cijev puna/nema nikoga
    public bool PrimiItemUVentilaciju(int itemType)
    {
        // 1. PRIORITET: Šalji Mirandi ako je u igri
        if (GameManager.Instance != null && GameManager.Instance.mirandaOdabrana && mirandinaVentilacija != null)
        {
            if (mirandinaVentilacija.MozePrimiti())
            {
                mirandinaVentilacija.SpremiItemZaMirandu(itemType);
                Debug.Log($"Sasha je poslao item ID: {itemType} Mirandi.");
                return true;
            }
            else
            {
                Debug.LogWarning("Mirandina ventilacija je PUNA! Sasha ne može poslati dok ona ne pokupi.");
                return false;
            }
        }
        // 2. FALLBACK: Ako nema Mirande u igri, šalji Giovanniju
        else if (GameManager.Instance != null && GameManager.Instance.giovanniOdabran && giovanniVentilacija != null)
        {
            // (Pazi: kod tebe je MozePrimiti na Giovanniju možda property bez zagrada ili funkcija)
            if (giovanniVentilacija.MozePrimiti)
            {
                giovanniVentilacija.SpremiItemZaGiovannia(itemType);
                Debug.Log($"Mirande nema u igri. Sasha šalje item ID: {itemType} direktno Giovanniju.");
                return true;
            }
            else
            {
                Debug.LogWarning("Giovannijeva ventilacija je PUNA! Sasha ne može poslati.");
                return false;
            }
        }
        else
        {
            Debug.LogError("Nema dostupnog primatelja za Sashin item!");
            return false;
        }
    }

    void Start()
    {
        otvorenSprite.SetActive(false);
        zatvorenSprite.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!jeOtvorena && ((other.CompareTag("Melee")) && melee.currentMultiplier == 3f) || !jeOtvorena && other.CompareTag("Pajser"))
        {
            jeOtvorena = true;
            zatvorenSprite.SetActive(false);
            otvorenSprite.SetActive(true);
            zvucniEfekti.ZvukUspjehaUdarcaVent.Play();
            Debug.Log("Ventilacija je otvorena!");
        }
        else if ((other.CompareTag("Melee")) && !jeOtvorena)
        {
            zvucniEfekti.ZvukNeuspjehaUdarcaVent.Play();
            Debug.Log("Ventilacija nije otvorena!");
        }


        if (other.CompareTag("Sasha"))
        {
            SashaController sasha = other.GetComponent<SashaController>();
            if (sasha != null) sasha.trenutnaVentilacija = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            SashaController sasha = other.GetComponent<SashaController>();
            if (sasha != null) sasha.trenutnaVentilacija = null;
        }
    }
}