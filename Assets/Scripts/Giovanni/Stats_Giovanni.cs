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

    [Header("Cooldown Nakon Napada")]
    public float postAttackCooldown = 5f;
    private float cooldownTimer = 0f;



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

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Threat se puni ili prazni
        if (!isThreatLocked)
        {
            if (isMoving)
            {
                if (cooldownTimer <= 0)
                {
                    float currentDrain = isSprinting ? threatDrainSprinting : threatDrainWalking;
                    currentThreat -= currentDrain * Time.deltaTime;
                }
            }
            else
            {
                // OVDJE SE POLAKO PUNI OD NULE PREMA GORE DOK IGRAČ MIRUJE!
                currentThreat += threatRegenIdle * Time.deltaTime;
            }

            if (isFlashlightOn && cooldownTimer <= 0)
            {
                currentThreat -= threatDrainFlashlight * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        currentThreat = Mathf.Clamp(currentThreat, 0, maxThreat);

        UpdateUI();

        // NOVO: Napad se može pokrenuti SAMO ako je threat na nuli, nije zaključan I ISTEKAO JE COOLDOWN!
        // Ovo omogućava da threat počne od 0 i raste bez da se riba odmah zaleti!
        if (currentThreat <= 0 && !isThreatLocked && cooldownTimer <= 0)
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
        if (isThreatLocked || cooldownTimer > 0) return;

        currentThreat -= amount;
        currentThreat = Mathf.Clamp(currentThreat, 0, maxThreat);
        UpdateUI();

        if (currentThreat <= 0 && !isThreatLocked && cooldownTimer <= 0)
        {
            isThreatLocked = true;
            currentThreat = 0f;

            if (viperFish != null)
            {
                viperFish.TriggerThreatEvent();
            }
        }
    }

    public bool IsInCooldown()
    {
        return cooldownTimer > 0;
    }

    public void UnlockThreat()
    {
        isThreatLocked = false;
        cooldownTimer = postAttackCooldown;
    }
}