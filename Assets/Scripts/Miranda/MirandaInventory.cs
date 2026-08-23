using UnityEngine;

public class MirandaInventory : MonoBehaviour
{
    [Header("Posjed Predmeta (0 - 5)")]
    [SerializeField] private bool imaZutiKljuc = false;       // ID 0
    [SerializeField] private bool imaGun = false;              // ID 1
    [SerializeField] private bool imaMinigun = false;          // ID 2
    [SerializeField] private bool imaRuku = false;             // ID 3
    [SerializeField] private bool imaLjubicastiKljuc = false;  // ID 4
    [SerializeField] private bool imaPajser = false;           // ID 5

    // Javni getteri za provjeru iz drugih skripti (npr. MirandaController, Kljucanice)
    public bool ImaZutiKljuc => imaZutiKljuc;
    public bool ImaGun => imaGun;
    public bool ImaMinigun => imaMinigun;
    public bool ImaRuku => imaRuku;
    public bool ImaLjubicastiKljuc => imaLjubicastiKljuc;
    public bool ImaPajser => imaPajser;

    // Univerzalna provjera posjeda bilo kojeg predmeta preko ID-a
    public bool HasItem(int itemType)
    {
        switch (itemType)
        {
            case 0: return imaZutiKljuc;
            case 1: return imaGun;
            case 2: return imaMinigun;
            case 3: return imaRuku;
            case 4: return imaLjubicastiKljuc;
            case 5: return imaPajser;
            default: return false;
        }
    }

    // Univerzalno dodavanje predmeta u inventar preko ID-a
    public void CollectItem(int itemType)
    {
        switch (itemType)
        {
            case 0:
                imaZutiKljuc = true;
                Debug.Log("Miranda inventar: Pokupljen Žuti ključ (ID 0)");
                break;
            case 1:
                imaGun = true;
                Debug.Log("Miranda inventar: Pokupljen Gun (ID 1)");
                break;
            case 2:
                imaMinigun = true;
                Debug.Log("Miranda inventar: Pokupljen Minigun (ID 2)");
                break;
            case 3:
                imaRuku = true;
                Debug.Log("Miranda inventar: Pokupljena Robotska ruka (ID 3)");
                break;
            case 4:
                imaLjubicastiKljuc = true;
                Debug.Log("Miranda inventar: Pokupljen Ljubičasti ključ (ID 4)");
                break;
            case 5:
                imaPajser = true;
                Debug.Log("Miranda inventar: Pokupljen Pajser (ID 5)");
                break;
            default:
                Debug.LogWarning("Miranda inventar: Primljen nepoznat ID predmeta: " + itemType);
                break;
        }
    }

    // Univerzalno uklanjanje / trošenje predmeta iz inventara preko ID-a
    public void RemoveItem(int itemType)
    {
        switch (itemType)
        {
            case 0:
                imaZutiKljuc = false;
                Debug.Log("Miranda inventar: Iskorišten Žuti ključ (ID 0)");
                break;
            case 1:
                imaGun = false;
                break;
            case 2:
                imaMinigun = false;
                break;
            case 3:
                imaRuku = false;
                break;
            case 4:
                imaLjubicastiKljuc = false;
                Debug.Log("Miranda inventar: Iskorišten Ljubičasti ključ (ID 4)");
                break;
            case 5:
                imaPajser = false;
                break;
            default:
                Debug.LogWarning("Miranda inventar: Pokušaj brisanja nepoznatog ID-a: " + itemType);
                break;
        }
    }

    // Debug ispis trenutnog stanja cijelog inventara
    public void DebugInventory()
    {
        Debug.Log($"Miranda Inventar Stanje: Žuti Kljuc={imaZutiKljuc}, Ljubičasti Kljuc={imaLjubicastiKljuc}, Ruka={imaRuku}, Gun={imaGun}, Minigun={imaMinigun}, Pajser={imaPajser}");
    }
}