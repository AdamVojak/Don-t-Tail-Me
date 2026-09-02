using UnityEngine;

public class MinskoPolje : MonoBehaviour
{
    [Header("Postavke Upozorenja")]
    [SerializeField] private int minUpozorenja = 3;
    [SerializeField] private int maxUpozorenja = 6;
    private int potrebnoDodira;
    private int trenutnoDodira = 0;

    private GiovanniController giovanniControllerRef;
    private bool isExploded = false;

    void Start()
    {
        if (giovanniControllerRef == null)
        {
            giovanniControllerRef = FindFirstObjectByType<GiovanniController>();
        }

        // Određujemo ukupan broj dodira (3 do 6)
        potrebnoDodira = Random.Range(minUpozorenja, maxUpozorenja + 1);
        trenutnoDodira = 0;
    }

    public void RegistrirajDodir(Vector3 pozicijaBombe, GameObject bombaKojaJeEksplodirala)
    {
        if (isExploded) return;

        trenutnoDodira++;

        if (trenutnoDodira < potrebnoDodira)
        {
            int preostalo = potrebnoDodira - trenutnoDodira;
            Debug.Log("<color=yellow>MINSKO POLJE:</color> Dodir na bombi! Preostalo još dodira: " + preostalo);
        }
        else
        {
            // DOSEGNUT JE LIMIT -> BUM!
            EksplodirajPolje(pozicijaBombe, bombaKojaJeEksplodirala);
        }
    }

    private void EksplodirajPolje(Vector3 pozicijaBombe, GameObject bombaKojaJeEksplodirala)
    {
        if (isExploded) return;
        isExploded = true;

        Debug.Log("<color=red>MINSKO POLJE:</color> BUM! Bomba je eksplodirala!");

        // Pokreni Death Screen za bombu (0 = Bomba)
        if (giovanniControllerRef != null)
        {
            giovanniControllerRef.Die(0);
        }

        if (bombaKojaJeEksplodirala != null)
        {
            Destroy(bombaKojaJeEksplodirala);
        }
    }
}