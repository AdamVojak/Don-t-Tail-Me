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
        }
        else
        {
            Debug.LogWarning(gameObject.name + " ne može pronaći SFX u sceni!");
        }

        Vector3 smjerIza = -transform.up;

        ciljGlave = glava.position + (smjerIza * udaljenostGlave);
        ciljKacige = kaciga.position + (smjerIza * udaljenostKacige);

        glava.localRotation = Quaternion.Euler(-180f, -180f, 180f);

        pocetnaPozicija = transform.position;

        rand = Random.Range(1,5);
    }

    void Update()
    {
        if (timer < 1f)
        {
            timer += Time.deltaTime * brzinaAnimacije;

            glava.position = Vector3.Lerp(glava.position, ciljGlave, timer);
            zvukKacige.Play();
            kaciga.position = Vector3.Lerp(kaciga.position, ciljKacige, timer);
            if (rand > 3)
            {
                zvukDecap.Play();
            }
            else zvukDecap2.Play();

        }
        else if (!animacijaZavrsena)
        {
            animacijaZavrsena = true;
            Invoke("UnistiObjekt", vrijemeDoUnistenja);
        }
    }

    void UnistiObjekt()
    {
        Destroy(gameObject);
    }
}