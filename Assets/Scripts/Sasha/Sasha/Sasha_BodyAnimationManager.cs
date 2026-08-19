using UnityEngine;

public class TijeloAnimationEvents : MonoBehaviour
{
    [SerializeField] private Melee meleeOruzje;
    private SashaController sashaController;

    void Start()
    {
        sashaController = GetComponentInParent<SashaController>();

        if (sashaController == null)
        {
            Debug.LogError("SashaAnimacijaEventi ne može pronaći SashaController na roditelju!");
        }

        AžurirajMeleeReferencu();
    }

    public void AžurirajMeleeReferencu()
    {
        meleeOruzje = GetComponentInChildren<Melee>(true);
    }

    public void ProslijediKrajDodavanja()
    {
        if (sashaController != null)
        {
            sashaController.ZavrsiDodavanje();
        }
    }

    public void EventUkljuciHitbox()
    {
        if (meleeOruzje != null && meleeOruzje.gameObject.activeInHierarchy)
        {
            meleeOruzje.UkljuciHitbox();
        }
    }

    public void EventIskljuciHitbox()
    {
        if (meleeOruzje != null && meleeOruzje.gameObject.activeInHierarchy)
        {
            meleeOruzje.IskljuciHitbox();
        }
    }
}