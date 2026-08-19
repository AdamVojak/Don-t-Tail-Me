using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Točke kretanja")]
    public Transform tockaZatvoreno;
    public Transform tockaOtvoreno;

    [Header("Postavke")]
    public float brzina = 3f;

    public bool jeOtvoreno = false;

    private Coroutine trenutnoKretanje;

    public void AktivirajVrata()
    {
        jeOtvoreno = !jeOtvoreno;

        Vector3 ciljnaPozicija = jeOtvoreno ? tockaOtvoreno.position : tockaZatvoreno.position;

        if (trenutnoKretanje != null)
        {
            StopCoroutine(trenutnoKretanje);
        }

        trenutnoKretanje = StartCoroutine(Pomakni(ciljnaPozicija));
    }

    private IEnumerator Pomakni(Vector3 cilj)
    {
        while (Vector3.Distance(transform.position, cilj) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                cilj,
                brzina * Time.deltaTime
            );

            yield return null;
        }

        transform.position = cilj;
        trenutnoKretanje = null;
    }
}