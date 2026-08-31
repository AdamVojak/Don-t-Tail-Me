using UnityEngine;
using UnityEngine.UI;

public class GiovanniStats : MonoBehaviour
{
    [Header("UI Slike - Okviri")]
    [SerializeField] private Image staminaImage;
    [SerializeField] private Image threatImage;

    [Header("Stamina Spriteovi")]
    [SerializeField] private Sprite staminaNormalSprite;
    [SerializeField] private Sprite staminaNightmareSprite;

    [Header("Threat Spriteovi")]
    [SerializeField] private Sprite threatNormalSprite;
    [SerializeField] private Sprite threatNightmareSprite;
    [SerializeField] private GameObject threatNightmareDodatak;

    [Header("UI Reference")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider threatSlider;

    [Header("Stamina Postavke")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainSprinting = 25f;
    public float staminaDrainWalking = 7f;
    public float staminaRegenIdle = 20f;

    [Header("Threat (Loudness) Postavke")]
    public float maxThreat = 100f;
    public float currentThreat;
    public float threatDrainSprinting = 10f;
    public float threatDrainWalking = 2f;
    public float threatRegenIdle = 5f;
    public float threatDrainFlashlight = 4f;

    [Header("Viper Fish Reference")]
    [SerializeField] private ViperFishController viperFish;

    public bool isThreatLocked = false;

    void Start()
    {
        currentStamina = maxStamina;
        currentThreat = maxThreat;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        if (threatSlider != null)
        {
            threatSlider.maxValue = maxThreat;
            threatSlider.value = currentThreat;
        }
    }

    public void UpdateStats(bool isMoving, bool isSprinting, bool isFlashlightOn)
    {
        if (isMoving && isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDrainSprinting * Time.deltaTime;
        }
        else if (isMoving)
        {
            currentStamina -= staminaDrainWalking * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenIdle * Time.deltaTime;
        }

        // NOVO: Threat se mijenja SAMO ako nije zaključan na nuli
        if (!isThreatLocked)
        {
            if (isMoving)
            {
                float currentDrain = isSprinting ? threatDrainSprinting : threatDrainWalking;
                currentThreat -= currentDrain * Time.deltaTime;
            }
            else
            {
                currentThreat += threatRegenIdle * Time.deltaTime;
            }

            if (isFlashlightOn)
            {
                currentThreat -= threatDrainFlashlight * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        currentThreat = Mathf.Clamp(currentThreat, 0, maxThreat);

        UpdateUI();

        if (currentThreat <= 0 && !isThreatLocked)
        {
            isThreatLocked = true;
            currentThreat = 0f;

            if (viperFish != null)
            {
                viperFish.TriggerThreatEvent();
            }
        }
    }

    private void UpdateUI()
    {
        if (staminaSlider != null) staminaSlider.value = currentStamina;
        if (threatSlider != null) threatSlider.value = currentThreat;

        if (staminaImage != null)
        {
            if (currentStamina <= maxStamina * 0.2f)
                staminaImage.sprite = staminaNightmareSprite;
            else
            {
                staminaImage.sprite = staminaNormalSprite;
            }
        }

        if (threatImage != null)
        {
            if (currentThreat <= maxThreat * 0.2f)
            {
                threatNightmareDodatak.SetActive(true);
                threatImage.sprite = threatNightmareSprite;
            }
            else
            {
                threatNightmareDodatak.SetActive(false);
                threatImage.sprite = threatNormalSprite;
            }
        }
    }

    public bool CanSprint()
    {
        return currentStamina > 1f;
    }

    public void ReduceThreat(float amount)
    {
        if (isThreatLocked) return;

        currentThreat -= amount;
        currentThreat = Mathf.Clamp(currentThreat, 0, maxThreat);
        UpdateUI();

        if (currentThreat <= 0 && !isThreatLocked)
        {
            isThreatLocked = true;
            currentThreat = 0f;

            if (viperFish != null)
            {
                viperFish.TriggerThreatEvent();
            }
        }
    }

    // NOVO: Ovu metodu poziva Viper kada završi prelet/upozorenje
    public void UnlockThreat()
    {
        isThreatLocked = false;
    }
}