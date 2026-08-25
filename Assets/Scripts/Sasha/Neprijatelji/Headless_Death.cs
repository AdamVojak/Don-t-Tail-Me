using System.Collections;
using UnityEngine;

public class HeadlessDeath : MonoBehaviour
{
    [Header("Objekti")]
    public GameObject krv;
    public GameObject tijelo;

    [Header("Postavke Skaliranja Krvi")]
    [Tooltip("Koliko se scale poveća u jednom koraku (npr. 0.25 znači 4 koraka do 1.0)")]
    public float korakSkaliranja = 0.25f;

    [Tooltip("Vrijeme čekanja između svakog skoka/koraka (manje = brže)")]
    public float vrijemeIzmeduKoraka = 0.25f;

    [Header("Kraj")]
    [Tooltip("Vrijeme čekanja nakon što se krv skroz raširi, prije nego se tijelo uništi")]
    public float pauzaPrijeUnistenja = 0.5f;

    void Start()
    {
        // Pokrećemo logiku odmah čim se objekt stvori
        StartCoroutine(LogikaSmrti());
    }

    IEnumerator LogikaSmrti()
    {
        // 1. Postavi početni scale krvi na (0, 0, 1)
        float trenutniScale = 0f;
        if (krv != null)
        {
            krv.transform.localScale = new Vector3(0f, 0f, 1f);
        }

        // 2. "Jittery" skaliranje u koracima dok ne dođe do 1
        while (trenutniScale < 1f)
        {
            yield return new WaitForSeconds(vrijemeIzmeduKoraka);

            trenutniScale += korakSkaliranja;
            trenutniScale = Mathf.Clamp01(trenutniScale); // Osigurava da ne pređe 1.0

            if (krv != null)
            {
                krv.transform.localScale = new Vector3(trenutniScale, trenutniScale, 1f);
            }
        }

        // 3. Čekamo dodatno vrijeme (npr. 0.5s) nakon što je krv na (1,1,1)
        yield return new WaitForSeconds(pauzaPrijeUnistenja);

        // 4. Uništavamo tijelo
        if (tijelo != null)
        {
            Destroy(tijelo);
        }

        // (Opcionalno) Ako želiš uništiti i ovu cijelu skriptu/parent objekt poslije svega,
        // makni '//' s linije ispod:
        // Destroy(gameObject);
    }
}