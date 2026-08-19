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

    private float currentTime;
    private bool timerRunning = false;

    private float timeMultiplier = 1f;

    void Awake()
    {
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

        if (gameManager.currChar == GameManager.ActiveCharacter.Miranda)
        {
            if (!timerRunning)
            {
                StartTimerInternal();
            }

            if (timerRunning)
            {
                currentTime -= Time.deltaTime * timeMultiplier;

                float rotationAngle = (currentTime / totalTimeInSeconds) * 360f;
                handRectTransform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    StopTimerInternal();
                    Debug.Log("Game Over: Vrijeme je isteklo");

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
            Debug.Log("Timer je pokrenut!");
        }
    }

    private void StopTimerInternal()
    {
        if (timerRunning)
        {
            timerRunning = false;
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
}