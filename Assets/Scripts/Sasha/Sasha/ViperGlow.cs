using System.Collections.Generic;
using UnityEngine;

public class ViperFishGlow : MonoBehaviour
{
    [Header("Reference")]
    public Transform player;

    [Tooltip("Stavi ovdje sva 3 modela ribe koji trebaju svijetliti")]
    public Renderer[] fishRenderers;

    [Header("Postavke Boje")]
    [ColorUsage(true, true)]
    public Color baseGlowColor = Color.white;

    [Header("Postavke Udaljenosti")]
    public float innerRadius = 15f;
    public float fadeDistance = 30f;

    // Lista u koju ćemo spremiti sve materijale sa svih modela
    private List<Material> glowMaterials = new List<Material>();

    void Start()
    {
        // Prolazimo kroz sva 3 modela koja si dodao u Inspectoru
        foreach (Renderer rend in fishRenderers)
        {
            if (rend != null)
            {
                // Uzimamo materijal s modela (ovo stvara sigurnu kopiju za igru)
                glowMaterials.Add(rend.material);
            }
        }
    }

    void Update()
    {
        if (glowMaterials.Count == 0 || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float currentIntensity = 0f;

        if (distance <= innerRadius)
        {
            float t = distance / innerRadius;
            currentIntensity = Mathf.Lerp(4f, 3f, t);
        }
        else if (distance <= innerRadius + fadeDistance)
        {
            float t = (distance - innerRadius) / fadeDistance;
            currentIntensity = Mathf.Lerp(3f, 0f, t);
        }
        else
        {
            currentIntensity = 0f;
        }

        // Primjenjujemo izračunati intenzitet na SVE materijale u listi
        Color finalColor = baseGlowColor * currentIntensity;
        foreach (Material mat in glowMaterials)
        {
            mat.SetColor("_EmissionColor", finalColor);
        }
    }
}