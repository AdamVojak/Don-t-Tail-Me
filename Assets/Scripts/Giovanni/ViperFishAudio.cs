using UnityEngine;

public class ViperFishAudio : MonoBehaviour
{
    [Header("3D Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Finalni Jumpscare Krik")]
    [Tooltip("Stravičan vrisak/urlik iz dubine kada krene u finalni napad")]
    [SerializeField] private AudioClip jumpscareScreamClip;

    [Header("Postavke")]
    [Range(0f, 1f)][SerializeField] private float screamVolume = 1f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Postavke za 3D prostorni zvuk iz daljine
        audioSource.spatialBlend = 1f; // 100% 3D zvuk
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.minDistance = 3f;  // Na 3m je maksimalno glasan (u licu)
        audioSource.maxDistance = 60f; // Čuje se iz dubine do 60 metara
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    /// <summary>
    /// Pušta finalni 3D urlik koji juri prema igraču
    /// </summary>
    public void PlayDeathScream()
    {
        if (audioSource != null && jumpscareScreamClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(jumpscareScreamClip, screamVolume);
        }
    }
}