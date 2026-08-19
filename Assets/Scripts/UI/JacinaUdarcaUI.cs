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

    private Image powerImage;

    void Start()
    {
        powerImage = GetComponent<Image>();
        if (powerImage == null)
        {
            Debug.LogError("Nema Image komponente na ovom GameObjectu! Molimo dodajte Image komponentu na UI element.");
            enabled = false;
            return;
        }

        if (playerMelee == null)
        {
            Debug.LogError("Referenca na Melee skriptu nije postavljena u Inspectoru! Molimo povucite Melee objekt u 'Player Melee' polje.");
            enabled = false;
            return;
        }

        UpdateJacinaUdarcaUI();
    }

    void Update()
    {
        UpdateJacinaUdarcaUI();
    }

    void UpdateJacinaUdarcaUI()
    {
            if (playerMelee == null || powerImage == null)
            {
                return;
            }

            float currentMult = playerMelee.currentMultiplier;

            if (currentMult <= 0.5f)
            {
                powerImage.sprite = slabUdarac;
            }
            else if (currentMult == 1.0f)
            {
                powerImage.sprite = obicanUdarac;
            }
            else if (currentMult == 2.0f)
            {
                powerImage.sprite = jaciUdarac;
            }
            else if (currentMult >= 3.0f)
            {
                powerImage.sprite = najjaciUdarac;
            }
        }
    }