using UnityEngine;

public class WarningLightFade : MonoBehaviour
{
    [Header("Lista Svjetala")]
    [Tooltip("Ovdje ubaci svoja svjetla (možeš ih dodati koliko god želiš)")]
    public Light[] lights;
    
    [Header("Postavke Intenziteta")]
    [Tooltip("Minimalni intenzitet svjetla (fade out)")]
    public float minIntensity = 0f;

    [Tooltip("Maksimalni intenzitet svjetla (fade in)")]
    public float maxIntensity = 15f;

    [Header("Postavke Vremena")]
    [Tooltip("Vrijeme jednog punog ciklusa (od 0 do 15 i nazad do 0) u sekundama")]
    public float fadeDuration = 3f;

    void Update()
    {
        if (lights == null || lights.Length == 0 || fadeDuration <= 0f)
            return;

        float frequency = (2f * Mathf.PI) / fadeDuration;
        float t = (Mathf.Sin(Time.time * frequency - Mathf.PI / 2f) + 1f) / 2f;

        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].intensity = currentIntensity;
            }
        }
    }
}
