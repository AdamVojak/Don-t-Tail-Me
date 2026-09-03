using UnityEngine;
using UnityEngine.UI;

public class UIConfettiEmitter : MonoBehaviour
{
    [Header("Konfeti Postavke")]
    public GameObject confettiPrefab; // Prefab bijelog kvadratića (UI Image)
    public float spawnRate = 0.05f;   // Koliko često izbacuje (manje = brže)
    public int burstCount = 5;        // Koliko konfeta izbaci u jednom ciklusu

    [Header("Fizika Konfeta")]
    public float minSpeed = 300f;     // Minimalna brzina padanja
    public float maxSpeed = 800f;     // Maksimalna brzina padanja
    public float minRotationSpeed = -300f; // Minimalna brzina rotacije
    public float maxRotationSpeed = 300f;  // Maksimalna brzina rotacije
    public float minSize = 0.5f;      // Najmanji konfet
    public float maxSize = 1.5f;      // Najveći konfet
    public float lifetime = 4f;       // Koliko dugo žive prije nego nestanu

    // Tvoje tražene boje!
    private Color[] confettiColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        new Color(1f, 0.4f, 0.7f), // Pink
        new Color(0.5f, 0f, 0.5f)  // Ljubičasta
    };

    private float timer;
    private bool isEmitting = false;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isEmitting || confettiPrefab == null) return;

        timer += Time.deltaTime;
        while (timer >= spawnRate)
        {
            for (int i = 0; i < burstCount; i++)
            {
                SpawnConfetti();
            }
            timer -= spawnRate;
        }
    }

    public void Play()
    {
        isEmitting = true;
        timer = 0f;
    }

    public void Stop()
    {
        isEmitting = false;
    }

    void SpawnConfetti()
    {
        // Stvaramo novi konfet kao dijete ovog objekta
        GameObject confetti = Instantiate(confettiPrefab, transform);
        RectTransform rect = confetti.GetComponent<RectTransform>();
        Image img = confetti.GetComponent<Image>();

        // Nasumična boja
        if (img != null)
        {
            img.color = confettiColors[Random.Range(0, confettiColors.Length)];
        }

        // Nasumična pozicija po širini ekrana (na vrhu)
        float width = rectTransform.rect.width;
        float randomX = Random.Range(-width / 2f, width / 2f);
        rect.anchoredPosition = new Vector2(randomX, 0); // Y je 0 jer je roditelj na vrhu ekrana

        // Nasumična veličina
        float size = Random.Range(minSize, maxSize);
        rect.localScale = new Vector3(size, size, 1f);

        // Dodajemo skriptu za padanje i rotaciju
        UIConfetti mover = confetti.GetComponent<UIConfetti>();
        if (mover == null) mover = confetti.AddComponent<UIConfetti>();

        mover.speed = Random.Range(minSpeed, maxSpeed);
        mover.rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        mover.lifetime = lifetime;
    }
}