using System;
using UnityEngine;

public class GameModeConfigurator : MonoBehaviour
{
    public static GameModeConfigurator Instance;

    public enum GameMode
    {
        AllThree,
        SashaAndMiranda,
        SashaAndGiovanni,
        MirandaAndGiovanni
    }

    [Header("Trenutni Prepoznati Mod (Info)")]
    public GameMode activeMode = GameMode.AllThree;

    [Serializable]
    public class ModeObjectSetup
    {
        [Tooltip("Objekti koji se potpuno brišu iz memorije (Destroy)")]
        public GameObject[] objectsToDestroy;

        [Tooltip("Objekti koji se pale za ovaj mod (SetActive true)")]
        public GameObject[] objectsToActivate;

        [Tooltip("Objekti koji se gase za ovaj mod (SetActive false)")]
        public GameObject[] objectsToDeactivate;

        [Tooltip("Novi redoslijed ciljeva za Giovannijev mobitel u ovom modu")]
        public Transform[] giovanniMobitelCiljevi; // NOVO: Lista ciljeva za mobitel!
    }

    [Header("=== 0. MOD: SVA 3 LIKA (Default) ===")]
    public ModeObjectSetup allThreeSetup;

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

    public void ApplyConfiguration(bool sasha, bool miranda, bool giovanni)
    {
        if (sasha && miranda && giovanni)
        {
            activeMode = GameMode.AllThree;
            Debug.Log("<color=cyan>[CONFIG] Pokrenut DEFAULTNI mod sa sva 3 lika.</color>");
            ApplyModeSetup(allThreeSetup);
        }
        else if (sasha && miranda && !giovanni)
        {
            activeMode = GameMode.SashaAndMiranda;
            Debug.Log("<color=yellow>[CONFIG] Pokrenut mod: SASHA + MIRANDA (Bez Giovannija).</color>");
            ApplyModeSetup(sashaMirandaSetup);
        }
        else if (sasha && !miranda && giovanni)
        {
            activeMode = GameMode.SashaAndGiovanni;
            Debug.Log("<color=yellow>[CONFIG] Pokrenut mod: SASHA + GIOVANNI (Bez Mirande).</color>");
            ApplyModeSetup(sashaGiovanniSetup);
        }
        else if (!sasha && miranda && giovanni)
        {
            activeMode = GameMode.MirandaAndGiovanni;
            Debug.Log("<color=yellow>[CONFIG] Pokrenut mod: MIRANDA + GIOVANNI (Bez Sashe).</color>");
            ApplyModeSetup(mirandaGiovanniSetup);
        }
    }

    private void ApplyModeSetup(ModeObjectSetup setup)
    {
        if (setup == null) return;

        if (setup.objectsToDestroy != null)
        {
            foreach (GameObject go in setup.objectsToDestroy)
            {
                if (go != null) Destroy(go);
            }
        }

        if (setup.objectsToActivate != null)
        {
            foreach (GameObject go in setup.objectsToActivate)
            {
                if (go != null) go.SetActive(true);
            }
        }

        if (setup.objectsToDeactivate != null)
        {
            foreach (GameObject go in setup.objectsToDeactivate)
            {
                if (go != null) go.SetActive(false);
            }
        }

        // =========================================================================
        // NOVO: ŠALJEMO NOVE CILJEVE U MOBITEL (Ako postoje za ovaj mod)
        // =========================================================================
        if (setup.giovanniMobitelCiljevi != null && setup.giovanniMobitelCiljevi.Length > 0)
        {
            MobitelTracker mobitel = FindFirstObjectByType<MobitelTracker>();
            if (mobitel != null)
            {
                mobitel.PostaviNoveCiljeve(setup.giovanniMobitelCiljevi);
            }
        }
    }
}