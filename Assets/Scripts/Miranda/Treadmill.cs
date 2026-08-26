using UnityEngine;

public class Treadmill : MonoBehaviour
{
    [Header("Postavke Punjenja")]
    public float mnoziteljPunjenja = 2f;

    [Header("Reference")]
    public Collider triggerZona;

    private MirandaController playerController;
    private CharacterController playerCollider;
    private bool isPlayerInZone = false;

    [Header("Audio")]
    [SerializeField] private TreadmillAudio treadmillAudio;
    private bool isActivelyRunning = false;

    void Start()
    {
        if (treadmillAudio == null) treadmillAudio = GetComponent<TreadmillAudio>();

        playerController = Object.FindAnyObjectByType<MirandaController>();

        if (playerController != null)
        {
            playerCollider = playerController.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (playerController == null || playerCollider == null || triggerZona == null) return;

        isPlayerInZone = triggerZona.bounds.Intersects(playerCollider.bounds);

        // Provjera: Miranda je u zoni, pod kontrolom je i stišće tipke za kretanje
        bool isRunningNow = isPlayerInZone && playerController.isControlled && (Mathf.Abs(playerController.currentZSpeed) > 0.1f);

        // --- AUDIO LOGIKA (Početak, Loop, Zaustavljanje) ---
        if (isRunningNow && !isActivelyRunning)
        {
            // Upravo je počela trčati
            isActivelyRunning = true;
            if (treadmillAudio != null) treadmillAudio.StartRunning();
        }
        else if (!isRunningNow && isActivelyRunning)
        {
            // Upravo se zaustavila ili sišla s trake
            isActivelyRunning = false;
            if (treadmillAudio != null) treadmillAudio.StopRunning();
        }
        // ---------------------------------------------------

        // Punjenje struje dok trči
        if (isRunningNow)
        {
            float brzinaTrcanja = Mathf.Abs(playerController.currentZSpeed);
            float kolicinaZaPunjenje = brzinaTrcanja * mnoziteljPunjenja * Time.deltaTime;

            // Šaljemo struju u centralni EnergyManager
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.DodajStruju(kolicinaZaPunjenje);
            }
        }
    }

    private void OnDisable()
    {
        if (isActivelyRunning)
        {
            isActivelyRunning = false;
            if (treadmillAudio != null) treadmillAudio.StopRunning();
        }
    }
}