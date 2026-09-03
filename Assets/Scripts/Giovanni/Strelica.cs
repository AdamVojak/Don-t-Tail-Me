using UnityEngine;

public class Strelica : MonoBehaviour
{
    [Header("Postavke Rotacije")]
    public float brzinaRotacije = 150f; // Brzina okretanja po Y osi

    [Header("Postavke Trzanja (Gore-Dolje)")]
    public float brzinaTrzanja = 4f;    // Koliko brzo ide gore-dolje
    public float visinaTrzanja = 0.5f;  // Koliko visoko/nisko ide

    private float pocetniY;

    void Start()
    {
        // Pamtimo početnu Y poziciju kako bi strelica uvijek lebdjela oko nje
        pocetniY = transform.localPosition.y;
    }

    void Update()
    {
        // 1. Rotacija oko Y osi
        transform.Rotate(0, brzinaRotacije * Time.deltaTime, 0);

        // 2. Trzanje po Y osi (koristeći Sinusoidu za glatko lebdenje gore-dolje)
        float noviY = pocetniY + Mathf.Sin(Time.time * brzinaTrzanja) * visinaTrzanja;

        // Primjenjujemo novu poziciju (X i Z ostaju isti, mijenja se samo Y)
        transform.localPosition = new Vector3(transform.localPosition.x, noviY, transform.localPosition.z);
    }
}