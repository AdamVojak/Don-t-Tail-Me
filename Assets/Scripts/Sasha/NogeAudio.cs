using UnityEngine;
using UnityEngine.Audio;

public class NogeAudio : MonoBehaviour
{
    [Header("3D Audio Source")]
    [Tooltip("Dovuci AudioSource sa ovog objekta ili child objekta")]
    [SerializeField] private AudioSource audioSource;

    [Header("Koraci (Footsteps)")]
    [SerializeField] private AudioClip leftFootstepClips;
    [SerializeField] private AudioClip rightFootstepClips;
    [Range(0f, 1f)][SerializeField] private float footstepVolume = 0.5f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }

    public void PlayLeftFootstep()
    {
        audioSource.PlayOneShot(leftFootstepClips, footstepVolume);
    }

    public void PlayRightFootstep()
    {
        audioSource.PlayOneShot(rightFootstepClips, footstepVolume);
    }
}
