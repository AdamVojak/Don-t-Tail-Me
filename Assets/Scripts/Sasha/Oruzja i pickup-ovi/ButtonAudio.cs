using UnityEngine;

public class ButtonAudio : MonoBehaviour
{
    [Header("Audio Source (3D)")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip pressInClip; // Zvuk pritiska (prema unutra)
    [SerializeField] private AudioClip popOutClip;  // Zvuk vraćanja (prema van)
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

    public void PlayPressIn()
    {
        if (audioSource != null && pressInClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(pressInClip, volume);
        }
    }

    public void PlayPopOut()
    {
        if (audioSource != null && popOutClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(popOutClip, volume);
        }
    }
}