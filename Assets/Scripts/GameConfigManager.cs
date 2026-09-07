using System;
using UnityEngine;

public class GameModeConfigurator : MonoBehaviour
{
    public static GameModeConfigurator Instance;

    public enum GameMode
    {
        AllThree,           // Sva 3 lika (Default)
        SashaAndMiranda,    // Nema Giovannija
        SashaAndGiovanni,   // Nema Mirande
        MirandaAndGiovanni  // Nema Sashe
    }

    [Header("Trenutni Prepoznati Mod (Info)")]
    public GameMode activeMode = GameMode.AllThree;

    // Struktura koja ti omogućuje da u Inspectoru samo povučeš objekte za svaki mod
    [Serializable]
    public class ModeObjectSetup
    {
        [Tooltip("Objekti koji se potpuno brišu iz memorije (Destroy)")]
        public GameObject[] objectsToDestroy;

        [Tooltip("Objekti koji se pale za ovaj mod (SetActive true)")]
        public GameObject[] objectsToActivate;

        [Tooltip("Objekti koji se gase za ovaj mod (SetActive false)")]
        public GameObject[] objectsToDeactivate;
    }

    [Header("=== 1. MOD: SASHA + MIRANDA (Nema Giovannija) ===")]
    public ModeObjectSetup sashaMirandaSetup;

    [Header("=== 2. MOD: SASHA + GIOVANNI (Nema Mirande) ===")]
    public ModeObjectSetup sashaGiovanniSetup;

    [Header("=== 3. MOD: MIRANDA + GIOVANNI (Nema Sashe) ===")]
    public ModeObjectSetup mirandaGiovanniSetup;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Ovu metodu poziva GameManager na početku
    public void ApplyConfiguration(bool sasha, bool miranda, bool giovanni)
    {
        // 1. AUTOMATSKA DETEKCIJA MODA
        if (sasha && miranda && giovanni)
        {
            activeMode = GameMode.AllThree;
            Debug.Log("<color=cyan>[CONFIG] Pokrenut DEFAULTNI mod sa sva 3 lika. Nema izmjena na mapi.</color>");
            return; // U defaultnom modu ne diramo ništa!
        }
        else if (sasha && miranda && !giovanni)
        {
            activeMode = GameMode.SashaAndMiranda;
            Debug.Log("<color=yellow>[CONFIG] Pokrenut mod: SASHA + MIRANDA (Bez Giovannija).</color>");
            ApplyModeSetup(sashaMirandaSetup);
            ConfigureSashaMirandaVentilations();
        }
        else if (sasha && !miranda && giovanni)
        {
            activeMode = GameMode.SashaAndGiovanni;
            Debug.Log("<color=yellow>[CONFIG] Pokrenut mod: SASHA + GIOVANNI (Bez Mirande).</color>");
            ApplyModeSetup(sashaGiovanniSetup);
            ConfigureSashaGiovanniVentilations();
        }
        else if (!sasha && miranda && giovanni)
        {
            activeMode = GameMode.MirandaAndGiovanni;
            Debug.Log("<color=yellow>[CONFIG] Pokrenut mod: MIRANDA + GIOVANNI (Bez Sashe).</color>");
            ApplyModeSetup(mirandaGiovanniSetup);
            ConfigureMirandaGiovanniVentilations();
        }
    }

    // Pomoćna metoda koja pali, gasi i uništava objekte za odabrani mod
    private void ApplyModeSetup(ModeObjectSetup setup)
    {
        if (setup == null) return;

        // 1. Uništavamo nepotrebne objekte (prepreke, nemoguće zagonetke...)
        if (setup.objectsToDestroy != null)
        {
            foreach (GameObject go in setup.objectsToDestroy)
            {
                if (go != null) Destroy(go);
            }
        }

        // 2. Palimo alternativne rute, ključeve i prečace
        if (setup.objectsToActivate != null)
        {
            foreach (GameObject go in setup.objectsToActivate)
            {
                if (go != null) go.SetActive(true);
            }
        }

        // 3. Gasimo objekte koji ne trebaju postojati
        if (setup.objectsToDeactivate != null)
        {
            foreach (GameObject go in setup.objectsToDeactivate)
            {
                if (go != null) go.SetActive(false);
            }
        }
    }


    // =========================================================================
    // OVDJE PODEŠAVAMO VENTILACIJE I REPERTOAR ITEMA ZA SVAKI MOD ZASEBNO:
    // =========================================================================

    private void ConfigureSashaMirandaVentilations()
    {
        // PRIMJER: Budući da nema Giovannija, Sasha ne mora slati Gun Giovanniju.
        // Ovdje možeš promijeniti kamo vode ventilacije ili koje iteme prihvaćaju!
        Debug.Log("[CONFIG] Ventilacije preusmjerene isključivo između Sashe i Mirande.");
    }

    private void ConfigureSashaGiovanniVentilations()
    {
        // PRIMJER: Nema Mirande, pa ventilacija izravno spaja Sashu i Giovannija.
        Debug.Log("[CONFIG] Ventilacije preusmjerene isključivo između Sashe i Giovannija.");
    }

    private void ConfigureMirandaGiovanniVentilations()
    {
        // PRIMJER: Nema Sashe, itemi koji su inače kod Sashe sada se nalaze negdje kod Mirande.
        Debug.Log("[CONFIG] Ventilacije preusmjerene isključivo između Mirande i Giovannija.");
    }
}