using UnityEngine;

public class VentNetworkManager : MonoBehaviour
{
    public static VentNetworkManager Instance;

    public enum Lik { Sasha, Miranda, Giovanni }

    [Header("Stanje Cijevi (Čeka li item u tranzitu)")]
    public bool sashaCekaItem = false;
    public int sashaPendingItemID = -1;

    public bool mirandaCekaItem = false;
    public int mirandaPendingItemID = -1;

    public bool giovanniCekaItem = false;
    public int giovanniPendingItemID = -1;

    [Header("Stanje na Podu (Leži li item nepreuzet ispred venta)")]
    public bool sashaItemNaPodu = false;
    public bool mirandaItemNaPodu = false;
    public bool giovanniItemNaPodu = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. PROVJERA: Može li primatelj primiti item (je li cijev/pod slobodan)?
    public bool MozePrimiti(Lik primatelj)
    {
        switch (primatelj)
        {
            case Lik.Sasha:
                return !sashaCekaItem && !sashaItemNaPodu;
            case Lik.Miranda:
                return !mirandaCekaItem && !mirandaItemNaPodu;
            case Lik.Giovanni:
                return !giovanniCekaItem && !giovanniItemNaPodu;
            default:
                return false;
        }
    }

    // 2. GLAVNA AKCIJA: Slanje itema kroz mrežu (NEMA LIMITA - šalji koliko god puta želiš ako imaš item!)
    public bool PosaljiItem(Lik posiljatelj, Lik primatelj, int itemID)
    {
        // Provjeri samo je li cijev primatelja slobodna
        if (!MozePrimiti(primatelj))
        {
            Debug.LogWarning($"[MREŽA] Cijev/Pod kod {primatelj} je puna! Čeka se da preuzme prethodni item.");
            return false;
        }

        // Ubaci item u cijev primatelja
        if (primatelj == Lik.Sasha)
        {
            sashaCekaItem = true;
            sashaPendingItemID = itemID;
        }
        else if (primatelj == Lik.Miranda)
        {
            mirandaCekaItem = true;
            mirandaPendingItemID = itemID;
        }
        else if (primatelj == Lik.Giovanni)
        {
            giovanniCekaItem = true;
            giovanniPendingItemID = itemID;
        }

        Debug.Log($"[MREŽA] {posiljatelj} je poslao item ID {itemID} -> {primatelj}.");
        return true;
    }

    // 3. GLAVNA AKCIJA: Kada bilo koja izlazna ventilacija u sobi izbacuje item
    public int PreuzmiItemIzCijevi(Lik lik)
    {
        int itemID = -1;

        if (lik == Lik.Sasha && sashaCekaItem)
        {
            itemID = sashaPendingItemID;
            sashaCekaItem = false;
            sashaPendingItemID = -1;
            sashaItemNaPodu = true;
        }
        else if (lik == Lik.Miranda && mirandaCekaItem)
        {
            itemID = mirandaPendingItemID;
            mirandaCekaItem = false;
            mirandaPendingItemID = -1;
            mirandaItemNaPodu = true;
        }
        else if (lik == Lik.Giovanni && giovanniCekaItem)
        {
            itemID = giovanniPendingItemID;
            giovanniCekaItem = false;
            giovanniPendingItemID = -1;
            giovanniItemNaPodu = true;
        }

        return itemID;
    }

    // 4. OBAVIJEST: Kada lik pokupi item s poda (oslobađa se mjesto za novi paket)
    public void ItemPokupljenSPoda(Lik lik)
    {
        if (lik == Lik.Sasha) sashaItemNaPodu = false;
        if (lik == Lik.Miranda) mirandaItemNaPodu = false;
        if (lik == Lik.Giovanni) giovanniItemNaPodu = false;

        Debug.Log($"[MREŽA] {lik} je pokupio item s poda. Ventilacija je ponovno slobodna za prijem!");
    }
}