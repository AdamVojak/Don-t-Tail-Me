using UnityEngine;
using System.Collections;

public class IskrenjeStruje : MonoBehaviour
{
    [Header("Komponente")]
    [SerializeField] private Light svjetlo; // Referenca na Light komponentu
    [SerializeField] private AudioSource zvucniEfekt; // Opcionalno za zvuk iskrenja

    [Header("Postavke Intenziteta Blicanja")]
    [SerializeField] private float minIntenzitet = 2f;
    [SerializeField] private float maxIntenzitet = 10f;

    [Header("Postavke Vremena (Brzina Iskrenja)")]
    [Tooltip("Trajanje jednog bljeska (u sekundama)")]
    [SerializeField] private float minTrajanjeBlica = 0.01f;
    [SerializeField] private float maxTrajanjeBlica = 0.05f;

    [Tooltip("Pauza između dva rafala iskrenja")]
    [SerializeField] private float minPauzaIzmedjuRafala = 0.2f;
    [SerializeField] private float maxPauzaIzmedjuRafala = 2.0f;

    [Header("Postavke Skakanja po X i Y osi")]
    [SerializeField] private float radijusPomakaX = 0.25f; // Koliko daleko po X osi iskre skaču
    [SerializeField] private float radijusPomakaY = 0.25f; // Koliko daleko po Y osi iskre skaču

    private Vector3 pocetnaPozicija;

    void Awake()
    {
        if (svjetlo == null)
        {
            svjetlo = GetComponent<Light>();
        }

        // Pamti originalnu poziciju strujne kutije
        pocetnaPozicija = transform.localPosition;
    }

    void OnEnable()
    {
        StartCoroutine(SimulirajIskrenje());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (svjetlo != null) svjetlo.enabled = false;
    }

    IEnumerator SimulirajIskrenje()
    {
        while (true)
        {
            // Odredi nasumičan broj brzih iskri u jednom "rafalu" (npr. 1 do 6 iskri zaredom)
            int brojIskriURafalu = Random.Range(1, 7);

            for (int i = 0; i < brojIskriURafalu; i++)
            {
                // 1. Postavi nasumičnu poziciju iskrenja unutar X i Y radijusa
                float nasumicniX = Random.Range(-radijusPomakaX, radijusPomakaX);
                float nasumicniY = Random.Range(-radijusPomakaY, radijusPomakaY);
                transform.localPosition = pocetnaPozicija + new Vector3(nasumicniX, nasumicniY, 0f);

                // 2. Upali svjetlo s nasumičnim intenzitetom
                svjetlo.intensity = Random.Range(minIntenzitet, maxIntenzitet);
                svjetlo.enabled = true;

                // 3. Ako imaš zvuk iskrenja, pusti ga s blago izmijenjenim pitch-om radi raznolikosti
                if (zvucniEfekt != null && zvucniEfekt.clip != null)
                {
                    zvucniEfekt.pitch = Random.Range(0.85f, 1.25f);
                    zvucniEfekt.PlayOneShot(zvucniEfekt.clip);
                }

                // 4. Čekaj kratki trenutak (trajanje bljeska)
                yield return new WaitForSeconds(Random.Range(minTrajanjeBlica, maxTrajanjeBlica));

                // 5. Ugasi svjetlo između mikro-iskri
                svjetlo.enabled = false;

                // Mikropauza između dvije iskre u istom rafalu
                yield return new WaitForSeconds(Random.Range(0.01f, 0.04f));
            }

            // Vrati svjetlo na početnu poziciju dok miruje
            transform.localPosition = pocetnaPozicija;

            // Nasumična pauza prije sljedećeg rafala iskri (kutija "šuti" neko vrijeme)
            yield return new WaitForSeconds(Random.Range(minPauzaIzmedjuRafala, maxPauzaIzmedjuRafala));
        }
    }
}