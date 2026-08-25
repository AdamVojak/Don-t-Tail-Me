using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class HeadlessAudio : MonoBehaviour
{
    [Header("3D Audio Source")]
    [Tooltip("Dovuci AudioSource sa ovog objekta ili child objekta")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvukovi Udaraca Oružja (Impacts)")]
    [SerializeField] private AudioClip[] pajserHitClips;
    [SerializeField] private AudioClip[] electricMeleeHitClips;

    [Range(0f, 1f)][SerializeField] private float hitVolume = 0.75f;

    [Header("Glas Ozljede (Krik Boli)")]
    [SerializeField] private AudioClip[] hurtVoiceClips;

    [Range(0f, 1f)][SerializeField] private float hurtVolume = 0.65f;

    [Header("Napad (Deranje / Krik)")]
    [Tooltip("Ovdje stavi svoja 2 zvuka deranja pri napadu")]
    [SerializeField] private AudioClip[] attackShoutClips;
    [Range(0f, 1f)][SerializeField] private float attackVolume = 0.7f;

    [Header("Elektrošok (Struja)")]
    [SerializeField] private AudioClip shockClip;
    [Range(0f, 1f)][SerializeField] private float shockVolume = 0.75f;

    [Header("Postavke Realizma")]
    [SerializeField] private bool useRandomPitch = true;
    [Range(0.85f, 1f)][SerializeField] private float minPitch = 0.92f;
    [Range(1f, 1.15f)][SerializeField] private float maxPitch = 1.08f;
    [Range(0.05f, 0.3f)][SerializeField] private float hurtDelay = 0.12f;
    private Coroutine hurtVoiceCoroutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }


    private void PlaySoundWithPitch(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;

        if (useRandomPitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(clip, volume);
    }

    private void PlayRandomFromArray(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;
        int randomIndex = Random.Range(0, clips.Length);
        PlaySoundWithPitch(clips[randomIndex], volume);
    }
    public void PlayAttackShout()
    {
        PlayRandomFromArray(attackShoutClips, attackVolume);
    }


    public void PlayShockSound()
    {
        // Za struju pitch možemo ostaviti malo luđim radi efekta
        PlaySoundWithPitch(shockClip, shockVolume);
    }

    public void PlayPajserHit()
    {
        PlayRandomFromArray(pajserHitClips, hitVolume);
    }

    public void PlayElectricMeleeHit()
    {
        PlayRandomFromArray(electricMeleeHitClips, hitVolume);
    }

    public void PlayHurtSound()
    {
        if (hurtVoiceClips == null || hurtVoiceClips.Length == 0) return;

        if (hurtVoiceCoroutine != null) StopCoroutine(hurtVoiceCoroutine);

        hurtVoiceCoroutine = StartCoroutine(PlayHurtDelayedRoutine());
    }

    private IEnumerator PlayHurtDelayedRoutine()
    {
        yield return new WaitForSeconds(hurtDelay);

        PlayRandomFromArray(hurtVoiceClips, hurtVolume);
        hurtVoiceCoroutine = null;
    }
}
