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

    [Header("Angler Fish Referenca")]
    [SerializeField] private AnglerFishController anglerFish;

    public bool isThreatLocked = false;

    [Header("Cooldown Nakon Napada")]
    public float postAttackCooldown = 5f;
    private float cooldownTimer = 0f;



    void Start()
    {
        if (anglerFish == null) anglerFish = FindFirstObjectByType<AnglerFishController>();
        if (anglerFish == null) viperFish = FindFirstObjectByType<ViperFishController>();

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

        // NOVO: Provjera je li igrač izvan granica mape
        bool isOutOfBounds = (anglerFish != null && anglerFish.IsOutOfBounds);

        if (!isThreatLocked)
        {
            // AKO JE OUT OF BOUNDS: Threat se NIKADA ne troši, samo se puni/regenerira!
            if (isOutOfBounds)
            {
                currentThreat += threatRegenIdle * Time.deltaTime;
            }
            // NORMALNO STANJE (Unutar mape)
            else
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
                    currentThreat += threatRegenIdle * Time.deltaTime;
                }

                if (isFlashlightOn && cooldownTimer <= 0)
                {
                    currentThreat -= threatDrainFlashlight * Time.deltaTime;
                }
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        currentThreat = Mathf.Clamp(currentThreat, 0, maxThreat);

        UpdateUI();

        // Napad se NE MOŽE pokrenuti ako je igrač izvan granica (Angler preuzima!)
        if (currentThreat <= 0 && !isThreatLocked && cooldownTimer <= 0 && !isOutOfBounds)
        {
            isThreatLocked = true;
            currentThreat = 0f;

            if (viperFish != null)
            {
                viperFish.TriggerThreatEvent();
            }
        }
    }

    public void ReduceThreat(float amount)
    {
        bool isOutOfBounds = (anglerFish != null && anglerFish.IsOutOfBounds);
        // Ako je out of bounds, odbij bilo kakvo skidanje threata!
        if (isThreatLocked || cooldownTimer > 0 || isOutOfBounds) return;

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