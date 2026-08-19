using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Točke kretanja")]
    public Transform tockaZatvoreno;
    public Transform tockaOtvoreno;

    [Header("Postavke")]
    public float brzina = 3f;

    private Coroutine trenutnoKretanje;

    // Ova funkcija se poziva automatski čim gumb stavi: skripta.enabled = true;
    private void OnEnable()
    {
        OdrediSmjerIPokreni();
    }

    public void OdrediSmjerIPokreni()
    {
        // 1. Mjerimo udaljenost vrata do obje točke
        float udaljenostDoZatvoreno = Vector3.Distance(transform.position, tockaZatvoreno.position);
        float udaljenostDoOtvoreno = Vector3.Distance(transform.position, tockaOtvoreno.position);

        Vector3 cilj;

        // 2. Ako su vrata bliže točki "Zatvoreno", šaljemo ih prema "Otvoreno" (i obrnuto)
        if (udaljenostDoZatvoreno < udaljenostDoOtvoreno)
        {
            cilj = tockaOtvoreno.position;
            Debug.Log("Vrata se OTVARAJU.");
        }
        else
        {
            cilj = tockaZatvoreno.position;
            Debug.Log("Vrata se ZATVARAJU.");
        }

        // 3. Pokrećemo kretanje
        if (trenutnoKretanje != null) StopCoroutine(trenutnoKretanje);
        trenutnoKretanje = StartCoroutine(PomakniPremaCilju(cilj));
    }

    private IEnumerator PomakniPremaCilju(Vector3 ciljnaPozicija)
    {
        while (Vector3.Distance(transform.position, ciljnaPozicija) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                ciljnaPozicija,
                brzina * Time.deltaTime
            );

            yield return null; // Čeka sljedeći frame
        }

        // Fiksiramo poziciju točno na cilj
        transform.position = ciljnaPozicija;

        // Skripta se sama gasi na kraju puta, spremna za novo paljenje preko gumba!
        this.enabled = false;
    }
}