using UnityEngine;

public class MenuAtmosphereController : MonoBehaviour
{
    [Header("Podvodne Postavke (Giovanni Postavke)")]
    [SerializeField] private Color underwaterColor = new Color(0f, 0.05f, 0.15f); // Tamno modra
    [SerializeField] private float fogDensity = 0.04f;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
    [SerializeField][Range(0f, 1f)] private float ambientMultiplier = 0.5f;

    [Header("Sun Light (Ugašeno na dnu mora)")]
    [SerializeField] private GameObject sunLight;

    private void Awake()
    {
        ApplyUnderwaterAtmosphere();
    }

    private void OnValidate()
    {
        // Omogućava ti da mijenjaš boje i gustoću magle u stvarnom vremenu dok uređuješ u Editoru!
        if (Application.isPlaying)
        {
            ApplyUnderwaterAtmosphere();
        }
    }

    public void ApplyUnderwaterAtmosphere()
    {
        // 1. Magla (Fog)
        RenderSettings.fog = true;
        RenderSettings.fogColor = underwaterColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogMode = fogMode;

        // 2. Glavna Kamera (Pozadina mora biti identična magli radi nevidljivog horizonta)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = underwaterColor;
        }

        // 3. Svjetlo okoline (Ambijent)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = underwaterColor * ambientMultiplier;

        // 4. Gašenje sunčevog svjetla
        if (sunLight != null)
        {
            sunLight.SetActive(false);
        }
    }
}