using UnityEngine;

public class Ventilacija_In_Sasha : MonoBehaviour
{
    [Header("Vizualni elementi ventilacije")]
    [SerializeField] private GameObject otvorenSprite;
    [SerializeField] private GameObject zatvorenSprite;

    [Header("Pozicije za interakciju")]
    public Transform interactionAreaCenter;

    [Header("Bitne skripte")]
    public Melee melee;
    public SFX zvucniEfekti;

    [Header("Poveznica s Mirandom")]
    public Ventilacija_Out_Miranda mirandinaVentilacija;

    public bool jeOtvorena = false;
    public bool JeOtvorena => jeOtvorena;

    void Start()
    {
        otvorenSprite.SetActive(false);
        zatvorenSprite.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!jeOtvorena && (other.CompareTag("Melee") || other.CompareTag("Pajser")) && melee.currentMultiplier == 3f)
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

    public void PrimiItemUVentilaciju(int itemType)
    {
        Debug.Log($"Sashina ventilacija je primila item tipa {itemType}. Šaljem Mirandi!");

        if (mirandinaVentilacija != null)
        {
            mirandinaVentilacija.SpremiItemZaMirandu(itemType);
        }
        else
        {
            Debug.LogWarning("Mirandina ventilacija nije povezana u Inspectoru!");
        }
    }
}