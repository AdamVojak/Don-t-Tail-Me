using UnityEngine;

public class ZombieDeath : MonoBehaviour
{
    [Header("Komponente")]
    public Transform glava;
    public Transform kaciga;

    [Header("Postavke Animacije")]
    public float brzinaAnimacije = 2f;
    public float udaljenostGlave = 0.5f;
    public float udaljenostKacige = 0.8f;
    public float vrijemeDoUnistenja = 2f;

    private Vector3 pocetnaPozicija;
    private Vector3 ciljGlave;
    private Vector3 ciljKacige;
    private float timer = 0f;
    private bool animacijaZavrsena = false;

    public SFX zvucniEfekti;
    private AudioSource zvukKacige;
    private AudioSource zvukDecap;
    private AudioSource zvukDecap2;

    private int rand;

    private Vector3 pocetnaPozicijaGlave;
    private Vector3 pocetnaPozicijaKacige;


    void Start()
    {
        if (zvucniEfekti == null)
        {
            zvucniEfekti = FindFirstObjectByType<SFX>();
        }

        if (zvucniEfekti != null)
        {
            zvukKacige = zvucniEfekti.ZvukKacige;
            zvukDecap = zvucniEfekti.ZvukRibljeGlave;
            zvukDecap2 = zvucniEfekti.ZvukRibljeGlave2;

            // Zvukove pokrećemo OVDJE (samo jednom), a ne u Update-u!
            if (zvukKacige != null) zvukKacige.Play();
            rand = Random.Range(1, 5);
            if (rand > 3 && zvukDecap != null) zvukDecap.Play();
            else if (zvukDecap2 != null) zvukDecap2.Play();
        }

        Vector3 smjerIza = -transform.up;

        // Pamtimo točne fiksne početne i ciljne pozicije
        pocetnaPozicijaGlave = glava.position;
        pocetnaPozicijaKacige = kaciga.position;

        ciljGlave = glava.position + (smjerIza * udaljenostGlave);
        ciljKacige = kaciga.position + (smjerIza * udaljenostKacige);

        glava.localRotation = Quaternion.Euler(-180f, -180f, 180f);
        pocetnaPozicija = transform.position;
    }

    void Update()
    {
        if (timer < 1f)
        {
            timer += Time.deltaTime * brzinaAnimacije;

            // Koristimo FIKSNU početnu poziciju (ovo sprječava trzanje u zadnjem frameu)
            glava.position = Vector3.Lerp(pocetnaPozicijaGlave, ciljGlave, timer);
            kaciga.position = Vector3.Lerp(pocetnaPozicijaKacige, ciljKacige, timer);
        }
        else if (!animacijaZavrsena)
        {
            animacijaZavrsena = true;

            // Osiguravamo točne krajnje pozicije
            glava.position = ciljGlave;
            kaciga.position = ciljKacige;

            // ODVAJAMO GLAVU: Glava više nije dijete ovog objekta i NEĆE se uništiti!
            glava.SetParent(null);

            // Pokrećemo postepeni fade out za tijelo i kacigu
            StartCoroutine(NestaniIUnistiTijelo());
        }
}

    private System.Collections.IEnumerator NestaniIUnistiTijelo()
    {
        yield return new WaitForSeconds(vrijemeDoUnistenja);

        SpriteRenderer[] spriteoviZaNestajanje = GetComponentsInChildren<SpriteRenderer>();
        float trajanjeNestajanja = 1.0f;
        float proteklo = 0f;

        while (proteklo < trajanjeNestajanja)
        {
            proteklo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, proteklo / trajanjeNestajanja);

            foreach (SpriteRenderer sr in spriteoviZaNestajanje)
            {
                if (sr != null && sr.material != null)
                {
                    Color c = sr.material.color;
                    c.a = alpha;
                    sr.material.color = c;
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}