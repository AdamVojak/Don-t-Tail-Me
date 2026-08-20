using UnityEngine;

public class MinskoPolje : MonoBehaviour
{
    [Header("Postavke Upozorenja")]
    [SerializeField] private int minUpozorenja = 3;
    [SerializeField] private int maxUpozorenja = 6;
    private int potrebnoDodira;
    private int trenutnoDodira = 0;

    [Header("Vizualni Efekt Eksplozije (Opcionalno)")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private GiovanniController giovanniControllerRef;
    private bool isExploded = false;

    void Start()
    {
        if (giovanniControllerRef == null)
        {
            giovanniControllerRef = FindFirstObjectByType<GiovanniController>();
        }

        // Određujemo ukupan broj dodira za cijelo minsko polje (od 3 do 6)
        potrebnoDodira = Random.Range(minUpozorenja, maxUpozorenja + 1);
        trenutnoDodira = 0;
    }

    // Poziva se čim BILO KOJA bomba registrira dodir
    public void RegistrirajDodir(Vector3 pozicijaBombe, GameObject bombaKojaJeEksplodirala)
    {
        if (isExploded) return;

        trenutnoDodira++;

        if (trenutnoDodira < potrebnoDodira)
        {
            // ZVUK UPOZORENJA
            if (SFX.zvucniEfekti != null && SFX.zvucniEfekti.ZvukNeuspjehaUdarcaVent != null)
            {
                SFX.zvucniEfekti.ZvukNeuspjehaUdarcaVent.Play();
            }

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

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, pozicijaBombe, Quaternion.identity);
        }

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