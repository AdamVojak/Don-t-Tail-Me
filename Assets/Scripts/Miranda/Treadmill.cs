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

    void Start()
    {
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

        if (isPlayerInZone && playerController.isControlled)
        {
            float brzinaTrcanja = Mathf.Abs(playerController.currentZSpeed);

            if (brzinaTrcanja > 0.1f)
            {
                float kolicinaZaPunjenje = brzinaTrcanja * mnoziteljPunjenja * Time.deltaTime;

                // Šaljemo struju u centralni EnergyManager
                if (EnergyManager.Instance != null)
                {
                    EnergyManager.Instance.DodajStruju(kolicinaZaPunjenje);
                }
            }
        }
    }
}