using UnityEngine;

public class Kutija_Sasha : MonoBehaviour
{
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
        if (objectsToControl != null)
        {
            for (int i = 0; i < objectsToControl.Length; i++)
            {
                if (objectsToControl[i] != null)
                {
                    objectsToControl[i].SetActive(false);
                }
            }
        }

        if (scriptsToControl != null)
        {
            for (int i = 0; i < scriptsToControl.Length; i++)
            {
                if (scriptsToControl[i] != null)
                {
                    scriptsToControl[i].enabled = false;
                }
            }
        }
    }
    void Start()
    {
        if (triggerObjekt != null)
        {
            gumbRef = triggerObjekt.GetComponent<GumbSasha>();
            leverRef = triggerObjekt.GetComponent<LeverSasha>();

            if (gumbRef == null && leverRef == null)
            {
                Debug.LogWarning("Na objektu '" + triggerObjekt.name + "' nije pronađena nijedna od 2 trigger skripte!", this);
            }
        }

        lastState = GetTriggerState();
        ApplyPowerState(lastState);
    }

    void Update()
    {
        if (triggerObjekt == null) return;

        bool currentState = GetTriggerState();

        if (currentState != lastState)
        {
            lastState = currentState;
            ApplyPowerState(currentState);
        }
    }


    private bool GetTriggerState()
    {
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