using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    [SerializeField] private bool vrijemeTece = true;

    [Header("Audio (2D)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clockTickingClip;
    [Range(0f, 1f)][SerializeField] private float clockVolume = 0.15f;

    [SerializeField] private AudioClip alarmClip;
    [Range(0f, 1f)][SerializeField] private float alarmVolume = 1f;

    [Header("Climax Reference")]
    [SerializeField] private MirandaClimaxManager climaxManager;

    [Header("Fade Out Reference")]
    [SerializeField] private Image[] clockSprites;

    private float currentTime;
    private bool timerRunning = false;
    private float timeMultiplier = 1f;

    // NOVO: Zaključava sat kad istekne vrijeme da ga Update ne restartira preko alarma!
    private bool isTimerFinished = false;

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
        vrijemeTece = true;
        isTimerFinished = false;
        if (ventWorm != null) ventWorm.SetActive(false);
    }

    void Update()
    {
        if (gameManager == null) return;

        // 1. AKO JE SAT ZAVRŠIO (ALARM SVIRA) -> NE DIRAJ NIŠTA I PREKINI UPDATE!
        if (isTimerFinished) return;

        if (gameManager.currChar == GameManager.ActiveCharacter.Miranda && !gameManager.isLoading)
        {
            if (!timerRunning)
            {
                StartTimerInternal();
            }

            if (timerRunning)
            {
                if (vrijemeTece)
                {
                    currentTime -= Time.deltaTime * timeMultiplier;
                }

                if (audioSource != null)
                {
                    float ciljaniPitch = Mathf.Clamp(timeMultiplier, 1.0f, 3.0f);
                    audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, ciljaniPitch, 2.5f * Time.deltaTime);
                }

                float rotationAngle = (currentTime / totalTimeInSeconds) * 360f;
                handRectTransform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

                // KADA VRIJEME ISTEKNE
                if (currentTime <= 0)
                {
                    currentTime = 0;
                    Debug.Log("<color=red>Game Over: Vrijeme je isteklo!</color>");

                    ZaustaviIUgasiTimer();

                    if (climaxManager != null)
                    {
                        climaxManager.PokreniKlimaksPrekoTajmera();
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
        if (!timerRunning && !isTimerFinished)
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
        if (gameManager != null && gameManager.currChar == GameManager.ActiveCharacter.Miranda && !isTimerFinished)
        {
            StartTimerInternal();
        }
    }

    public void StopTimer()
    {
        StopTimerInternal();
    }

    public void ResetTimer()
    {
        isTimerFinished = false;
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
    }

    public float GetRemainingTime() => currentTime;
    public bool IsTimerRunning() => timerRunning;
    public void SetTimeMultiplier(float multiplier) => timeMultiplier = multiplier;

    private void OnDisable()
    {
        // Gasi zvuk samo ako sat NIJE završio (jer ako je završio, coroutine se brine za alarm!)
        if (audioSource != null && !isTimerFinished)
        {
            audioSource.Stop();
        }
    }

    // =========================================================================
    // POZIVA SE KAD ISTEKNE VRIJEME ILI KAD SE SAT UGASI
    // =========================================================================
    public void ZaustaviIUgasiTimer()
    {
        if (isTimerFinished) return; // Osigurač da se ne pozove dvaput
        isTimerFinished = true;

        timerRunning = false;
        vrijemeTece = false;

        // 1. GASIMO KUCANJE I PUŠTAMO ALARM
        if (audioSource != null)
        {
            audioSource.Stop(); // Gasi kucanje
            audioSource.pitch = 1f;

            if (alarmClip != null)
            {
                audioSource.PlayOneShot(alarmClip, alarmVolume); // PUSTI ALARM
            }
        }

        // 2. POKREĆEMO NESTANKA SLIKE I GAŠENJE
        StartCoroutine(FadeOutAndShutdownRoutine());
    }

    private IEnumerator FadeOutAndShutdownRoutine()
    {
        if (clockSprites == null || clockSprites.Length == 0)
        {
            clockSprites = GetComponentsInChildren<Image>();
        }

        Color[] initialColors = new Color[clockSprites.Length];
        for (int i = 0; i < clockSprites.Length; i++)
        {
            if (clockSprites[i] != null) initialColors[i] = clockSprites[i].color;
        }

        float elapsed = 0f;
        float fadeDuration = 1.0f;

        // 1. Fade-out slika kroz 1 sekundu
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            for (int i = 0; i < clockSprites.Length; i++)
            {
                if (clockSprites[i] != null)
                {
                    Color c = initialColors[i];
                    c.a = Mathf.Lerp(initialColors[i].a, 0f, t);
                    clockSprites[i].color = c;
                }
            }
            yield return null;
        }

        // 2. Čekaj da alarm odsvira do kraja
        float remainingAudioTime = 0f;
        if (alarmClip != null)
        {
            remainingAudioTime = Mathf.Max(0f, alarmClip.length - fadeDuration);
        }
        if (remainingAudioTime > 0f)
        {
            yield return new WaitForSeconds(remainingAudioTime);
        }

        // 3. Tek kad je alarm odsvirao do kraja, ugasi cijeli objekt
        gameObject.SetActive(false);
    }
}