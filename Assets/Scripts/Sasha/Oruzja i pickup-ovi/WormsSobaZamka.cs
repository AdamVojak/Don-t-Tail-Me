using Unity.VisualScripting;
using UnityEngine;

public class WormsSobaZamka : MonoBehaviour
{
    [Header("Predmet koji pokreće zamku")]
    public GameObject predmetZaPokupiti;

    [Header("Elementi Zamke u Sobi")]
    public GameObject svijetlo;
    public LeverSasha[] lever;
    public GumbSasha[] gumbi;
    public Door vrata;
    private bool trapActivated = false;

    void Update()
    {
        if (!trapActivated && predmetZaPokupiti == null)
        {
            AktivirajZamku();
        }
    }
    void AktivirajZamku()
    {
        trapActivated = true;

        Destroy(svijetlo.gameObject);

        foreach (GumbSasha g in gumbi)
        {
            g.aktiviran = false;
        }
        foreach (LeverSasha l in lever)
        {
            l.aktiviran = false;
        }

        vrata.enabled = true;
        vrata.OdrediSmjerIPokreni();
    }
}