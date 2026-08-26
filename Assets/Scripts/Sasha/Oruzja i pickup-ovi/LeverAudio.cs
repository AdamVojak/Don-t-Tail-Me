using UnityEngine;

public class LeverAudio : MonoBehaviour
{
    [Header("Audio Source (3D)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip switchOnClip;
    [SerializeField] private AudioClip switchOffClip;
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f; // 3D
        audioSource.playOnAwake = false;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 15f;
    }

    public void PlaySwitch(bool isOn)
    {
        AudioClip clip = isOn ? switchOnClip : switchOffClip;
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip, volume);
        }
    }
}