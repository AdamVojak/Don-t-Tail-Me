using UnityEngine;
using UnityEngine.UI; // Obavezno za UI Image

public class MinigunUI : MonoBehaviour
{
    [Header("Reference")]
    public Minigun minigun;
    public Image uiImage;
    public float maxAmmo = Minigun.maxAmmo;

    [Header("Sličice (72 sprite-a)")]
    public Sprite[] ammoSprites;

    private void Start()
    {
        if (minigun == null)
        {
            minigun = FindFirstObjectByType<Minigun>();
            if (minigun == null) Debug.LogError("Minigun reference is not set in MinigunUI.");
        }
        if (uiImage == null)
        {
            Debug.LogError("UI Image reference is not set in MinigunUI.");
        }
        if (ammoSprites.Length == 0)
        {
            Debug.LogError("Ammo sprites array is empty in MinigunUI.");
        }

    }

    void Update()
    {
        if (minigun == null || uiImage == null || ammoSprites.Length == 0) return;


        float postotakMetaka = (float)minigun.ammo / (float)maxAmmo;

        int index = Mathf.RoundToInt((1f - postotakMetaka) * (ammoSprites.Length - 1));

        index = Mathf.Clamp(index, 0, ammoSprites.Length - 1);

        uiImage.sprite = ammoSprites[index];
    }
}