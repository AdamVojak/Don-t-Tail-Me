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

    [Header("Limiti Slanja")]
    public int sashaPoslanoUkupno = 0;      // Sasha smije poslati MAX 1 item
    public int giovanniPoslanoUkupno = 0;   // Giovanni smije poslati MAX 1 item

    // NOVO: Omogućava da sashaJePoslaoItem i giovanniJePoslaoItem rade automatski!
    public bool sashaJePoslaoItem => sashaPoslanoUkupno >= 1;
    public bool giovanniJePoslaoItem => giovanniPoslanoUkupno >= 1;

    public bool mirandaPoslalaSashi = false;
    public bool mirandaPoslalaGiovanniju = false;

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

    // 2. PROVJERA: Smije li pošiljatelj još slati?
    public bool SmijePoslati(Lik posiljatelj, Lik primatelj)
    {
        if (posiljatelj == Lik.Sasha)
        {
            return sashaPoslanoUkupno < 1;
        }
        else if (posiljatelj == Lik.Giovanni)
        {
            return giovanniPoslanoUkupno < 1;
        }
        else if (posiljatelj == Lik.Miranda)
        {
            if (primatelj == Lik.Sasha) return !mirandaPoslalaSashi;
            if (primatelj == Lik.Giovanni) return !mirandaPoslalaGiovanniju;
        }
        return false;
    }

    // 3. GLAVNA AKCIJA: Slanje itema kroz mrežu
    public bool PosaljiItem(Lik posiljatelj, Lik primatelj, int itemID)
    {
        // Provjeri limit pošiljatelja
        if (!SmijePoslati(posiljatelj, primatelj))
        {
            Debug.LogWarning($"[MREŽA] {posiljatelj} je već iskoristio limit slanja prema {primatelj}!");
            return false;
        }

        // Provjeri je li cijev primatelja slobodna
        if (!MozePrimiti(primatelj))
        {
            Debug.LogWarning($"[MREŽA] Cijev/Pod kod {primatelj} je puna! Čeka se preuzimanje.");
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

        // Zabilježi iskorišteni limit
        if (posiljatelj == Lik.Sasha) sashaPoslanoUkupno++;
        else if (posiljatelj == Lik.Giovanni) giovanniPoslanoUkupno++;
        else if (posiljatelj == Lik.Miranda)
        {
            if (primatelj == Lik.Sasha) mirandaPoslalaSashi = true;
            if (primatelj == Lik.Giovanni) mirandaPoslalaGiovanniju = true;
        }

        Debug.Log($"[MREŽA] {posiljatelj} je uspješno poslao item ID {itemID} -> {primatelj}.");
        return true;
    }

    // 4. GLAVNA AKCIJA: Kada bilo koja izlazna ventilacija u sobi izbacuje item
    public int PreuzmiItemIzCijevi(Lik lik)
    {
        int itemID = -1;

        if (lik == Lik.Sasha && sashaCekaItem)
        {
            itemID = sashaPendingItemID;
            sashaCekaItem = false;
            sashaPendingItemID = -1;
            sashaItemNaPodu = true; // Sada leži na podu sobe
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

    // 5. OBAVIJEST: Kada lik pokupi item s poda (oslobađa se mjesto za novi paket)
    public void ItemPokupljenSPoda(Lik lik)
    {
        if (lik == Lik.Sasha) sashaItemNaPodu = false;
        if (lik == Lik.Miranda) mirandaItemNaPodu = false;
        if (lik == Lik.Giovanni) giovanniItemNaPodu = false;

        Debug.Log($"[MREŽA] {lik} je pokupio item s poda. Njegova ventilacija je ponovno potpuno slobodna!");
    }
}