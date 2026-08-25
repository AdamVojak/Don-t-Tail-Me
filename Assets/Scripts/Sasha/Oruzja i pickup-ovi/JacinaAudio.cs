using UnityEngine;

public class JacinaAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [Tooltip("Ako je prazno, automatski će uzeti AudioSource s ovog objekta")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zvukovi Punjenja (Kad se dosegne razina)")]
    [Tooltip("Zvuk kad se dosegne Razina 1 (npr. tihi 'ping' ili 'whoosh')")]
    [SerializeField] private AudioClip chargeLevel1;

    [Tooltip("Zvuk kad se dosegne Razina 2 (npr. glasniji 'ping' / jača rezonanca)")]
    [SerializeField] private AudioClip chargeLevel2;

    [Tooltip("Zvuk kad se dosegne Razina 3 (Maksimalna snaga - npr. 'flash' / električni zvuk)")]
    [SerializeField] private AudioClip chargeLevel3;

    [Header("Postavke")]
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Osiguravamo 2D zvuk za UI
        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
        }
    }


    public void PlayChargeLevel(int level)
    {
        if (audioSource == null) return;

        switch (level)
        {
            case 1:
                if (chargeLevel1 != null) audioSource.PlayOneShot(chargeLevel1, volume);
                break;
            case 2:
                if (chargeLevel2 != null) audioSource.PlayOneShot(chargeLevel2, volume);
                break;
            case 3:
                if (chargeLevel3 != null) audioSource.PlayOneShot(chargeLevel3, volume);
                break;
        }
    }
}
