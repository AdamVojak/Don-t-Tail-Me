using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private RectTransform handRectTransform;

    [Header("Postavke timera")]
    [SerializeField] private float totalTimeInSeconds = 60f;

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager;

    [Header("Worm Reference")]
    [SerializeField] private GameObject ventWorm;

    [Header("Debug / Development Postavke")]
    [SerializeField] private bool vrijemeTece = true; // <-- TVOJ NOVI BOOLEAN (početno uvijek true)

    [Header("Audio (2D)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clockTickingClip;
    [Range(0f, 1f)][SerializeField] private float clockVolume = 0.15f;

    private float currentTime;
    private bool timerRunning = false;

    private float timeMultiplier = 1f;

    [SerializeField] private AudioClip alarmClip;
    [Range(0f, 1f)][SerializeField] private float alarmVolume = 1f;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = clockTickingClip;
        audioSource.volume = clockVolume;

        if (handRectTransform == null)
        {
            Debug.LogError("RectTransform kazaljke nije postavljen! Molimo povucite RectTransform kazaljke u 'Hand Rect Transform' polje.");
            enabled = false;
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager referenca nije postavljena u Inspectoru! Molimo povucite GameManager objekt u 'Game Manager' polje.");
            enabled = false;
            return;
        }

        currentTime = totalTimeInSeconds;
        vrijemeTece = true; // Osiguravamo da je uvijek upaljen pri pokretanju igre
        ventWorm.SetActive(false);
    }

    void Update()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager referenca je izgubljena! Onemogućujem TimerUI.");
            enabled = false;
            return;
        }



        if (gameManager.currChar == GameManager.ActiveCharacter.Miranda && !gameManager.isLoading)
        {
            if (!timerRunning)
            {
                StartTimerInternal();
            }

            if (timerRunning)
            {
                // Vrijeme se oduzima SAMO ako je vrijemeTece = true
                if (vrijemeTece)
                {
                    currentTime -= Time.deltaTime * timeMultiplier;
                }

                float rotationAngle = (currentTime / totalTimeInSeconds) * 360f;
                handRectTransform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    StopTimerInternal();
                    Debug.Log("Game Over: Vrijeme je isteklo");

                    if (audioSource != null && alarmClip != null)
                    {
                        audioSource.PlayOneShot(alarmClip, alarmVolume);
                    }

                    if (ventWorm != null)
                    {
                        ventWorm.SetActive(true);
                    }
                }
            }
        }
        else
        {
            if (timerRunning)
            {
                StopTimerInternal();
            }
        }
    }

    private void StartTimerInternal()
    {
        if (!timerRunning)
        {
            timerRunning = true;

            if (audioSource != null && clockTickingClip != null)
            {
                audioSource.Play();
            }

            Debug.Log("Timer je pokrenut!");
        }
    }

    private void StopTimerInternal()
    {
        if (timerRunning)
        {
            timerRunning = false;

            if (audioSource != null)
            {
                audioSource.Stop();
            }

            Debug.Log("Timer je zaustavljen!");
        }
    }

    public void StartTimer()
    {
        if (gameManager != null && gameManager.currChar == GameManager.ActiveCharacter.Miranda)
        {
            StartTimerInternal();
        }
        else
        {
            Debug.Log("Timer se ne može pokrenuti jer Miranda nije aktivni lik ili GameManager nije postavljen.");
        }
    }

    public void StopTimer()
    {
        StopTimerInternal();
    }

    public void ResetTimer()
    {
        currentTime = totalTimeInSeconds;
        handRectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        StopTimerInternal();
        Debug.Log("Timer je resetiran!");
    }

    public void AddTime(float secondsToAdd)
    {
        currentTime += secondsToAdd;
        if (currentTime > totalTimeInSeconds)
        {
            currentTime = totalTimeInSeconds;
        }
        Debug.Log($"Dodano {secondsToAdd} sekundi. Trenutno vrijeme: {currentTime:F2}");
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public bool IsTimerRunning()
    {
        return timerRunning;
    }

    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = multiplier;
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}