using UnityEngine;

public class ClickHint : MonoBehaviour
{
    public Sprite hintClick;
    private SashaController zapamceniSasha; // Varijabla koja pamti Sashu

    private void OnTriggerStay(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null && sasha.isControlled)
        {
            zapamceniSasha = sasha; // Spremamo referencu!
            sasha.PrikaziHint(this, hintClick);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null)
        {
            sasha.SakrijHint(this);
            zapamceniSasha = null;
        }
    }

    private void OnDisable()
    {
        // Ako se objekt ugasi/uništi dok Sasha stoji u njemu, prisilno gasi hint!
        if (zapamceniSasha != null)
        {
            zapamceniSasha.PrisilnoUgasiSveHintove();
            zapamceniSasha = null;
        }
    }
}