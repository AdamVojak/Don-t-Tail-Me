using UnityEngine;
using Unity.Cinemachine;

public class SashaRoomManager : MonoBehaviour
{
    [Header("Početno Stanje (Gdje Sasha počinje)")]
    public GameObject pocetnaSoba;
    public CinemachineCamera pocetnaKamera;

    [Header("Sve Sobe i Kamere u Igri")]
    public GameObject[] sveSobe;
    public CinemachineCamera[] sveKamere;

    void Awake()
    {
        // 1. Ugasi sve sobe OSIM početne
        foreach (GameObject soba in sveSobe)
        {
            if (soba != null && soba != pocetnaSoba)
            {
                soba.SetActive(false);
            }
        }

        // 2. Osiguraj da je početna upaljena (bez da ju gasimo pa palimo)
        if (pocetnaSoba != null) pocetnaSoba.SetActive(true);

        // 3. RESET KAMERA
        foreach (CinemachineCamera kamera in sveKamere)
        {
            if (kamera != null) kamera.Priority = 0;
        }
        if (pocetnaKamera != null) pocetnaKamera.Priority = 10;

        // 4. AUTOMATIKA ZA GAMEMANAGER
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.sashaCam = pocetnaKamera;
        }
    }
}