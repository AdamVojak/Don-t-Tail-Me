using UnityEngine;

public class ClickHint : MonoBehaviour
{
    [Header("UI Elementi Hinta")]
    public SpriteRenderer hintRenderer; // Ovdje povuci SpriteRenderer hinta iznad glave
    public Sprite hintClick;             // Sličica tipke

    void Start()
    {
        if (hintRenderer != null)
        {
            hintRenderer.enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();

        if (sasha != null && sasha.isControlled && sasha.currentState != SashaController.SashaState.Dead)
        {
            if (hintRenderer != null)
            {
                if (hintClick != null) hintRenderer.sprite = hintClick;
                hintRenderer.enabled = true;
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
            hintRenderer.enabled = false;
        }
    }
}