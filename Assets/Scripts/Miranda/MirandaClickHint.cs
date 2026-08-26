using UnityEngine;

public class ClickHintMiranda : MonoBehaviour
{
    [Header("UI Elementi Hinta")]
    public SpriteRenderer hintRenderer; // Povuci SpriteRenderer direktno umjesto GameObjecta!
    public Sprite hintClick;            // Sličica tipke (npr. 'F')

    void Start()
    {
        if (hintRenderer != null)
        {
            // GameObject OSTJE UPALJEN, samo gasimo vidljivost slike!
            hintRenderer.enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        MirandaController miranda = other.GetComponentInParent<MirandaController>();

        // Ako je Miranda tu, kontrolirana i živa
        if (miranda != null && miranda.isControlled && miranda.currentState != MirandaController.MirandaState.Dead)
        {
            if (hintRenderer != null)
            {
                if (hintClick != null) hintRenderer.sprite = hintClick;
                hintRenderer.enabled = true; // PALI SAMO SLIKU
            }
        }
        else
        {
            if (hintRenderer != null && hintRenderer.enabled)
            {
                hintRenderer.enabled = false; // GASI SAMO SLIKU
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MirandaController miranda = other.GetComponentInParent<MirandaController>();
        if (miranda != null)
        {
            if (hintRenderer != null)
            {
                hintRenderer.enabled = false; // GASI SAMO SLIKU
            }
        }
    }
}