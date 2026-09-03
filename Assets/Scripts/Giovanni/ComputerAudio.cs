using System.Collections;
using UnityEngine;

public class ComputerAudio : MonoBehaviour
{
    [Header("Audio Source (3D)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvukovi Kompjutera")]
    [Tooltip("Zvuk paljenja kompjutera (One-shot)")]
    [SerializeField] private AudioClip startupClip;

    [Tooltip("Zvuk koji se vrti u loopu dok je kompjuter upaljen (Zujanje / Ventilator)")]
    [SerializeField] private AudioClip runningLoopClip;

    [Tooltip("Zvuk gašenja kompjutera koji prekida sve prethodno")]
    [SerializeField] private AudioClip shutdownClip;

    [Header("Postavke")]
    [Range(0f, 1f)][SerializeField] private float volume = 0.85f;

    private Coroutine sequenceCoroutine;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f; // 100% 3D zvuk u sobi
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.minDistance = 1.5f;
        audioSource.maxDistance = 15f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>
    /// Pokreće sekvencu: Startup zvuk -> Automatski Loop rada
    /// </summary>
    public void PlayStartupSequence()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(StartupSequenceRoutine());
    }

    private IEnumerator StartupSequenceRoutine()
    {
        if (audioSource == null || startupClip == null) yield break;

        // 1. Puštamo zvuk paljenja
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = startupClip;
        audioSource.volume = volume;
        audioSource.Play();

        // Čekamo točno onoliko sekundi koliko traje startup audio zapis
        yield return new WaitForSeconds(startupClip.length);

        // 2. Prebacujemo se na stalni 3D loop rada/ventilatora
        if (runningLoopClip != null)
        {
            audioSource.clip = runningLoopClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        sequenceCoroutine = null;
    }

    /// <summary>
    /// Trenutno prekida bilo paljenje bilo loop i pušta zvuk gašenja
    /// </summary>
    public void PlayShutdown()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        if (audioSource != null && shutdownClip != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = shutdownClip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}