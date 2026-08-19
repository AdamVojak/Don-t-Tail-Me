using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MirandaInventory : MonoBehaviour
{
    [Header("Posjed kljuceva")]
    [SerializeField] private bool imaZutiKljuc = false;
    [SerializeField] private bool imaLjubicastiKljuc = false;
    [SerializeField] private bool imaRuku = false;
    [SerializeField] private bool imaGun = false;
    [SerializeField] private bool imaMinigun = false;
    [SerializeField] private bool imaPajser = false;
    public bool ImaZutiKljuc => imaZutiKljuc;
    public bool ImaLjubicastiKljuc => imaLjubicastiKljuc;
    public bool ImaGun => imaGun;
    public bool ImaMinigun => imaMinigun;
    public bool ImaRuku => imaRuku;
    public bool ImaPajser => imaPajser;


    public void CollectKey(int typeOfKey)
    {
        if (typeOfKey == (int)Kljucevi.TipKljuca.Zuti)
        {
            if (!imaZutiKljuc) // Provjeri da već nema žuti ključ
            {
                imaZutiKljuc = true;
                Debug.Log("Pokupljen žuti ključ! Miranda sada posjeduje žuti ključ.");
            }
        }
        else if (typeOfKey == (int)Kljucevi.TipKljuca.Ljubicasti)
        {
            if (!imaLjubicastiKljuc) // Provjeri da već nema ljubičasti ključ
            {
                imaLjubicastiKljuc = true;
                Debug.Log("Pokupljen ljubičasti ključ! Miranda sada posjeduje ljubičasti ključ.");
            }
        }
        else
        {
            Debug.LogWarning("Nepoznat tip item-a: " + typeOfKey);
        }
    }

    public bool HasKey(int typeOfKey)
    {
        if (typeOfKey == (int)Kljucevi.TipKljuca.Zuti)
        {
            return imaZutiKljuc;
        }
        else if (typeOfKey == (int)Kljucevi.TipKljuca.Ljubicasti)
        {
            return imaLjubicastiKljuc;
        }
        return false;
    }

    public bool HasItem(int itemType)
    {
        if (itemType == 1) return imaGun;
        if (itemType == 2) return imaMinigun;
        if (itemType == 3) return imaRuku;
        if (itemType == 5) return imaPajser;

        return false;
    }

    // Metoda za uklanjanje ključa iz inventara (koristi je LockDoor)
    public void RemoveKey(int typeOfKey)
    {
        if (typeOfKey == (int)Kljucevi.TipKljuca.Zuti)
        {
            if (imaZutiKljuc)
            {
                imaZutiKljuc = false;
                Debug.Log("Iskorišten žuti ključ. Miranda više nema žuti ključ.");
            }
        }
        else if (typeOfKey == (int)Kljucevi.TipKljuca.Ljubicasti)
        {
            if (imaLjubicastiKljuc)
            {
                imaLjubicastiKljuc = false;
                Debug.Log("Iskorišten ljubičasti ključ. Miranda više nema ljubičasti ključ.");
            }
        }
    }

    public void CollectItem(int itemType)
    {
        if (itemType == 0)
        {
            imaZutiKljuc = true;
            Debug.Log("Miranda dobila: Žuti ključ");
        }
        else if (itemType == 4)
        {
            imaLjubicastiKljuc = true;
            Debug.Log("Miranda dobila: Ljubičasti ključ");
        }
        else if (itemType == 1)
        {
            imaGun = true;
            Debug.Log("Miranda dobila: Gun");
        }
        else if (itemType == 2)
        {
            imaMinigun = true;
            Debug.Log("Miranda dobila: Minigun");
        }
        else if (itemType == 3)
        {
            imaRuku = true;
            Debug.Log("Miranda dobila: Robotsku ruku");
        }
        else if (itemType == 5)
        {
            imaPajser = true;
            Debug.Log("Miranda dobila: Pajser");
        }
        else
        {
            Debug.LogWarning("Miranda primila nepoznat ID predmeta: " + itemType);
        }
    }

    public void RemoveItem(int itemType)
    {
        if (itemType == 0) imaZutiKljuc = false;
        else if (itemType == 4) imaLjubicastiKljuc = false;
        else if (itemType == 1) imaGun = false;
        else if (itemType == 2) imaMinigun = false;
        else if (itemType == 3) imaRuku = false;
        else if (itemType == 5) imaPajser = false;
    }

    // Opcionalno: Metoda za prikaz trenutnog inventara (za debug)
    public void DebugInventory()
    {
        Debug.Log($"Miranda inventar: Žuti ključ: {imaZutiKljuc}, Ljubičasti ključ: {imaLjubicastiKljuc}");
    }
}