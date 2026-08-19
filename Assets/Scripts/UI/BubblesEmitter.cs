using UnityEngine;

public class BubbleEmitter : MonoBehaviour
{
    [Header("Mali Mjehurići")]
    public GameObject smallBubblePrefab;
    public float smallSpawnRate = 0.02f;
    public int smallBurstCount = 4; // NOVO: Koliko malih mjehurića stvori ODJEDNOM!
    public float smallMinSize = 0.5f;
    public float smallMaxSize = 1.2f;

    [Header("Veliki Mjehurići")]
    public GameObject bigBubblePrefab;
    public float bigSpawnRate = 0.04f;
    public int bigBurstCount = 2; // NOVO: Koliko velikih mjehurića stvori ODJEDNOM!
    public float bigMinSize = 1.5f;
    public float bigMaxSize = 3.0f;

    [Header("Zajedničke Postavke")]
    public float minSpeed = 500f;
    public float maxSpeed = 1200f;
    public float lifetime = 2f;

    private float smallTimer;
    private float bigTimer;
    private bool isEmitting = false;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isEmitting) return;

        // Rafalno stvaranje malih mjehurića
        if (smallBubblePrefab != null)
        {
            smallTimer += Time.deltaTime;
            while (smallTimer >= smallSpawnRate)
            {
                for (int i = 0; i < smallBurstCount; i++)
                {
                    SpawnBubble(smallBubblePrefab, smallMinSize, smallMaxSize);
                }
                smallTimer -= smallSpawnRate; // Oduzimamo umjesto resetiranja na 0
            }
        }

        // Rafalno stvaranje velikih mjehurića
        if (bigBubblePrefab != null)
        {
            bigTimer += Time.deltaTime;
            while (bigTimer >= bigSpawnRate)
            {
                for (int i = 0; i < bigBurstCount; i++)
                {
                    SpawnBubble(bigBubblePrefab, bigMinSize, bigMaxSize);
                }
                bigTimer -= bigSpawnRate;
            }
        }
    }

    public void Play()
    {
        isEmitting = true;
        smallTimer = 0f;
        bigTimer = 0f;
    }

    public void Stop()
    {
        isEmitting = false;
    }

    void SpawnBubble(GameObject prefab, float minSize, float maxSize)
    {
        GameObject bubble = Instantiate(prefab, transform);
        RectTransform rect = bubble.GetComponent<RectTransform>();

        float width = rectTransform.rect.width;
        float randomX = Random.Range(-width / 2f, width / 2f);
        rect.anchoredPosition = new Vector2(randomX, 0);

        float size = Random.Range(minSize, maxSize);
        rect.localScale = new Vector3(size, size, 1f);

        UIBubble mover = bubble.GetComponent<UIBubble>();
        if (mover == null) mover = bubble.AddComponent<UIBubble>();

        mover.speed = Random.Range(minSpeed, maxSpeed);
        mover.lifetime = lifetime;
    }
}