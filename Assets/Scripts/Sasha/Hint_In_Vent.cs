using UnityEngine;

public class HintBreakVent : MonoBehaviour
{
    [Header("UI Elementi Hinta")]
    public SpriteRenderer hintRenderer; // Ovdje povuci SpriteRenderer hinta iznad glave
    public Sprite hintClick;             // Sličica lijevog klika
    public Sprite hintF;                 // Sličica tipke F
    public Ventilacija_In_Sasha ventInRef;

    void Start()
    {
        if (hintRenderer != null)
        {
            hintRenderer.enabled = false; // Samo gasimo crtanje, objekt ostaje aktivan!
        }

        if (ventInRef == null)
        {
            ventInRef = FindFirstObjectByType<Ventilacija_In_Sasha>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();

        // Radi samo ako je Sasha pod kontrolom igrača i nije mrtav
        if (sasha != null && sasha.isControlled && sasha.currentState != SashaController.SashaState.Dead)
        {
            if (hintRenderer != null)
            {
                bool otvoren = (ventInRef != null) && ventInRef.jeOtvorena;
                hintRenderer.sprite = otvoren ? hintF : hintClick;
                hintRenderer.enabled = true; // Pali samo sliku
            }
        }
        else
        {
            if (hintRenderer != null && hintRenderer.enabled)
            {
                hintRenderer.enabled = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null && hintRenderer != null)
        {
            hintRenderer.enabled = false; // Gasi samo sliku
        }
    }
}