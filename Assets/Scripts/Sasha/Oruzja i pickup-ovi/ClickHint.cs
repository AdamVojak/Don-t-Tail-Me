using UnityEngine;

public class ClickHint : MonoBehaviour
{
    public Sprite hintClick;
    private SashaController zapamceniSasha;
    private int collidersInside = 0; // NOVO: Broji koliko je Sashinih collidera unutra!

    private void OnTriggerEnter(Collider other)
    {
        // Ignoriramo druge triggere (npr. zone za detekciju oružja ili crva na Sashi)
        if (other.isTrigger) return;

        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null)
        {
            collidersInside++; // Sasha je ušao
            zapamceniSasha = sasha;

            if (sasha.isControlled)
            {
                sasha.PrikaziHint(this, hintClick);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;

        // Ako igrač tek dobije kontrolu dok već stoji u zoni, upali hint
        if (zapamceniSasha != null && zapamceniSasha.isControlled)
        {
            zapamceniSasha.PrikaziHint(this, hintClick);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null && sasha == zapamceniSasha)
        {
            collidersInside = Mathf.Max(0, collidersInside - 1);

            // =========================================================
            // KLJUČNO: Hint se gasi TEK kad je cijeli Sasha izašao van!
            // (Nema više štekanja i titranja dok hoda!)
            // =========================================================
            if (collidersInside == 0)
            {
                sasha.SakrijHint(this);
                zapamceniSasha = null;
            }
        }
    }

    private void OnDisable()
    {
        // Gasimo SAMO svoj hint, ne diramo druge objekte na mapi!
        if (zapamceniSasha != null)
        {
            zapamceniSasha.SakrijHint(this);
            zapamceniSasha = null;
        }
        collidersInside = 0;
    }
}