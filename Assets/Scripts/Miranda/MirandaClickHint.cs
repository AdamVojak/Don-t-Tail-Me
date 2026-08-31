using UnityEngine;

public class ClickHintMiranda : MonoBehaviour
{
    public Sprite hintClick; // Sličica tipke (npr. 'F')

    private void OnTriggerStay(Collider other)
    {
        MirandaController miranda = other.GetComponentInParent<MirandaController>();

        // Ako je Miranda tu i pod kontrolom igrača
        if (miranda != null && miranda.isControlled)
        {
            // Predajemo 'this' kao vlasnika i sličicu koju želimo prikazati
            miranda.PrikaziHint(this, hintClick);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MirandaController miranda = other.GetComponentInParent<MirandaController>();
        if (miranda != null)
        {
            // Gasimo hint (Miranda će ga ugasiti samo ako smo mi vlasnici)
            miranda.SakrijHint(this);
        }
    }
}