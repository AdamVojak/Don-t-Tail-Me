using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    // Singleton - omogućuje bilo kojoj skripti da pristupi Manageru preko EnergyManager.Instance
    public static EnergyManager Instance { get; private set; }

    [Header("Postavke Struje")]
    public float struja = 0f;
    public float maxStruja = 100f;
    public float brzinaPraznjenja = 5f;

    private MirandaController mirandaController;
    private SashaController sashaController;

    private GameManager gameManager;

    private void Awake()
    {
        // Osiguravamo da postoji samo jedan EnergyManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (mirandaController == null) mirandaController = Object.FindAnyObjectByType<MirandaController>();
        if (sashaController == null) sashaController = Object.FindAnyObjectByType<SashaController>();
        if (gameManager == null) gameManager = Object.FindAnyObjectByType<GameManager>();

        bool mirandaKontrolirana = mirandaController != null && mirandaController.isControlled;
        bool sashaKontroliran = sashaController != null && sashaController.isControlled;
        bool loading = gameManager != null && gameManager.isLoading;

        // Struja se prazni samo ako je aktivan Sasha ili Miranda I NISMO u loadingu
        if ((mirandaKontrolirana || sashaKontroliran) && !loading)
        {
            if (struja > 0)
            {
                struja -= brzinaPraznjenja * Time.deltaTime;
            }
        }

        struja = Mathf.Clamp(struja, 0, maxStruja);
    }

    // Poziva se iz Treadmill skripte
    public void DodajStruju(float kolicina)
    {
        struja += kolicina;
        struja = Mathf.Clamp(struja, 0, maxStruja);
    }
}