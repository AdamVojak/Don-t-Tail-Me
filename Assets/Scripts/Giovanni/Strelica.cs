using UnityEngine;

public class Strelica : MonoBehaviour
{
    [Header("Postavke Rotacije")]
    public float brzinaRotacije = 150f;
    public float brzinaTrzanja = 4f;
    public float visinaTrzanja = 0.5f;

    [Header("Reference i Uvjeti")]
    public GameManager gameManager;
    public string giovanniTag = "Giovanni";

    private float pocetniY;
    private bool giovanniUBlizini = false;

    // NOVO: Vlastito vrijeme kako strelica ne bi "trznula" kad se odmrzne
    private float trenutnoVrijeme = 0f;

    void Start()
    {
        pocetniY = transform.localPosition.y;

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    void Update()
    {
        bool giovanniAktivan = false;
        if (gameManager != null && gameManager.currChar == GameManager.ActiveCharacter.Giovanni)
        {
            giovanniAktivan = true;
        }

        // Ako je uvjet ispunjen, strelica se miče
        if (giovanniAktivan || giovanniUBlizini)
        {
            // 1. Rotacija oko Y osi
            transform.Rotate(0, brzinaRotacije * Time.deltaTime, 0);

            // 2. Trzanje po Y osi (koristimo naše vrijeme umjesto Time.time)
            trenutnoVrijeme += Time.deltaTime;
            float noviY = pocetniY + Mathf.Sin(trenutnoVrijeme * brzinaTrzanja) * visinaTrzanja;
            transform.localPosition = new Vector3(transform.localPosition.x, noviY, transform.localPosition.z);
        }
        // Obrisali smo 'else' blok! 
        // Sada, ako uvjet nije ispunjen, kod jednostavno ne radi ništa i strelica ostaje zaleđena.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(giovanniTag))
        {
            giovanniUBlizini = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(giovanniTag))
        {
            giovanniUBlizini = false;
        }
    }
}