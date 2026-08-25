using UnityEngine;
using UnityEngine.UI;

public class JacinaUdarcaUI : MonoBehaviour
{
    [Header("Spriteovi za jacinu")]
    [SerializeField] private Sprite slabUdarac;
    [SerializeField] private Sprite obicanUdarac;
    [SerializeField] private Sprite jaciUdarac;
    [SerializeField] private Sprite najjaciUdarac;

    [Header("Reference")]
    [SerializeField] private Melee playerMelee;
    [SerializeField] private JacinaAudio zvukovi;

    private Image powerImage;
    private int zadnjaRazina = 0; // 0 = slab, 1 = običan, 2 = jači, 3 = najjači

    void Start()
    {
        powerImage = GetComponent<Image>();
        if (powerImage == null)
        {
            Debug.LogError("Nema Image komponente na ovom GameObjectu!");
            enabled = false;
            return;
        }

        if (playerMelee == null)
        {
            Debug.LogError("Referenca na Melee skriptu nije postavljena u Inspectoru!");
            enabled = false;
            return;
        }

        if (zvukovi == null)
        {
            zvukovi = GetComponent<JacinaAudio>();
        }

        // Postavi početni sprite bez puštanja zvuka na startu igre
        powerImage.sprite = slabUdarac;
    }

    void Update()
    {
        UpdateJacinaUdarcaUI();
    }

    void UpdateJacinaUdarcaUI()
    {
        if (playerMelee == null || powerImage == null) return;

        float currentMult = playerMelee.currentMultiplier;
        int trenutnaRazina = 0;

        // 1. Određujemo razinu na temelju RASPONA, a ne točnog broja
        if (currentMult >= 3.0f)
        {
            trenutnaRazina = 3;
            powerImage.sprite = najjaciUdarac;
        }
        else if (currentMult >= 2.0f)
        {
            trenutnaRazina = 2;
            powerImage.sprite = jaciUdarac;
        }
        else if (currentMult >= 1.0f)
        {
            trenutnaRazina = 1;
            powerImage.sprite = obicanUdarac;
        }
        else
        {
            trenutnaRazina = 0;
            powerImage.sprite = slabUdarac;
        }

        // 2. Ako se razina NIJE promijenila, prekidamo (da ne spama zvuk)
        if (trenutnaRazina == zadnjaRazina)
        {
            return;
        }

        // 3. Pusti zvuk SAMO ako se razina povećala (npr. s 0 na 1, s 1 na 2, itd.)
        if (trenutnaRazina > zadnjaRazina && zvukovi != null)
        {
            if (trenutnaRazina == 1) zvukovi.PlayChargeLevel(1);
            else if (trenutnaRazina == 2) zvukovi.PlayChargeLevel(2);
            else if (trenutnaRazina == 3) zvukovi.PlayChargeLevel(3);
        }

        // 4. Spremi novu razinu
        zadnjaRazina = trenutnaRazina;
    }
}