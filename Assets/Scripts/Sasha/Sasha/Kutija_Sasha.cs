using UnityEngine;

public class Kutija_Sasha : MonoBehaviour
{
    [Header("Ovisnost o Glavnoj Kutiji (Opcionalno za sporedne kutije)")]
    public Kutija_Sasha glavnaKutija; // Ovdje na sporednim kutijama povučeš Glavnu Kutiju

    [Header("Trigger Objekt")]
    public GameObject triggerObjekt;

    [Header("Objekti koje želimo Aktivirati / Deaktivirati (SetActive)")]
    public GameObject[] objectsToControl;

    [Header("Skripte / Komponente koje želimo Enable-ati (enabled)")]
    public Behaviour[] scriptsToControl;

    [Header("Obrnuta Logika (Opcionalno)")]
    public bool invertLogic = false;

    private GumbSasha gumbRef;
    private LeverSasha leverRef;

    private bool lastState = false;

    private void Awake()
    {
        ApplyPowerState(false);
    }

    void Start()
    {
        if (triggerObjekt != null)
        {
            gumbRef = triggerObjekt.GetComponent<GumbSasha>();
            leverRef = triggerObjekt.GetComponent<LeverSasha>();

            if (gumbRef == null && leverRef == null)
            {
                Debug.LogWarning("Na objektu '" + triggerObjekt.name + "' nije pronađena nijedna trigger skripta!", this);
            }
        }

        lastState = IsPowerActive();
        ApplyPowerState(lastState);
    }

    void Update()
    {
        bool currentState = IsPowerActive();

        if (currentState != lastState)
        {
            lastState = currentState;
            ApplyPowerState(currentState);
        }
    }

    // =========================================================================
    // GLAVNA LOGIKA: 2 FAZE (Prije i Poslije Zamke)
    // =========================================================================
    public bool IsPowerActive()
    {
        // 1. Provjeravamo je li se dogodio nestanak struje u Worms Sobi
        bool nestanakStrujeSeDogodio = SashaSvjetlaKontroler.Instance != null && SashaSvjetlaKontroler.Instance.upravljanjeAktivno;

        // AKO SE NESTANAK STRUJE DOGODIO -> Struja u EnergyManageru MORA biti veća od 0!
        if (nestanakStrujeSeDogodio)
        {
            bool imaGlobalneStruje = EnergyManager.Instance != null && EnergyManager.Instance.struja > 0;
            if (!imaGlobalneStruje)
            {
                return false; // Nema struje -> gasi sve kutije!
            }
        }
        // (Ako se nestanak struje još NIJE dogodio, gornji uvjet se preskače i struja se ignorira!)


        // 2. Je li Glavna kutija upaljena? (Ovo pravilo vrijedi UVIJEK - i prije i poslije zamke!)
        if (glavnaKutija != null && !glavnaKutija.IsPowerActive())
        {
            return false;
        }

        // 3. Je li prekidač/lever na samoj ovoj kutiji uključen?
        return GetTriggerState();
    }

    private bool GetTriggerState()
    {
        if (triggerObjekt == null) return false;

        bool isTriggerActive = false;

        if (gumbRef != null)
        {
            isTriggerActive = gumbRef.aktiviran;
        }
        else if (leverRef != null)
        {
            isTriggerActive = leverRef.aktiviran;
        }

        return invertLogic ? !isTriggerActive : isTriggerActive;
    }

    private void ApplyPowerState(bool state)
    {
        if (objectsToControl != null)
        {
            for (int i = 0; i < objectsToControl.Length; i++)
            {
                if (objectsToControl[i] != null)
                {
                    objectsToControl[i].SetActive(state);
                }
            }
        }

        if (scriptsToControl != null)
        {
            for (int i = 0; i < scriptsToControl.Length; i++)
            {
                if (scriptsToControl[i] != null)
                {
                    scriptsToControl[i].enabled = state;
                }
            }
        }
    }
}