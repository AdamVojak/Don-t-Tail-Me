using System.Collections;
using UnityEngine;

public class GunItemPickup : MonoBehaviour
{
    [Header("Postavke migoljenja (Hobotnica)")]
    public float vrijemeRotacije = 0.5f;
    public float trajanjeSkoka = 0.15f;
    public float vrijemeCekanja = 0.3f;
    public bool kreceSe;

    public float minUdaljenost = 0.2f;
    public float maxUdaljenost = 0.6f;

    public bool isPickedUp = false;
    private SashaController sasha;

    private SFX sfx;
    private AudioSource skok;
    private AudioSource doskok;
    private AudioSource pickup;

    void Awake()
    {
        if (sfx == null)
        {
            sfx = FindFirstObjectByType<SFX>();
            if (sfx != null)
            {
                skok = sfx.ZvukGunSkok;
                doskok = sfx.ZvukGunDoskok;
                pickup = sfx.ZvukGunPickup;
            }
        }

        if (sasha == null)
        {
            sasha = FindFirstObjectByType<SashaController>();
            if (sasha == null)
            {
                Debug.LogWarning(gameObject.name + " ne može pronaći SashaController u sceni!");
            }
        }
    }
    void OnEnable()
    {
        if (!isPickedUp)
        {
            StartCoroutine(MigoljenjeRoutine());
        }
    }

    private IEnumerator MigoljenjeRoutine()
    {
        while (!isPickedUp)
        {
            while (sasha == null || !sasha.isControlled)
            {
                kreceSe = false;
                yield return null;
            }

            kreceSe = true;
            float randomKut = Random.Range(0f, 360f);
            float randomUdaljenost = Random.Range(minUdaljenost, maxUdaljenost);

            Quaternion pocetnaRotacija = transform.rotation;
            Quaternion ciljnaRotacija = Quaternion.Euler(0, 0, randomKut);

            float protekloVrijeme = 0f;
            while (protekloVrijeme < vrijemeRotacije)
            {
                transform.rotation = Quaternion.Slerp(pocetnaRotacija, ciljnaRotacija, protekloVrijeme / vrijemeRotacije);
                protekloVrijeme += Time.deltaTime;
                yield return null;
            }
            transform.rotation = ciljnaRotacija;

            if (skok != null) skok.Play();

            Vector3 pocetnaPozicija = transform.position;
            Vector3 ciljnaPozicija = pocetnaPozicija + (transform.up * randomUdaljenost);

            protekloVrijeme = 0f;

            while (protekloVrijeme < trajanjeSkoka)
            {
                if (sasha != null && sasha.isControlled)
                {
                    transform.position = Vector3.Lerp(pocetnaPozicija, ciljnaPozicija, protekloVrijeme / trajanjeSkoka);
                    protekloVrijeme += Time.deltaTime;
                }
                yield return null;
            }

            transform.position = ciljnaPozicija;

            if (doskok != null) doskok.Play();

            yield return new WaitForSeconds(vrijemeCekanja);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPickedUp && other.CompareTag("Sasha"))
        {
            SashaInventory inventar = other.GetComponent<SashaInventory>();

            if (inventar != null)
            {
                kreceSe = false;
                if (pickup != null) pickup.Play();
                inventar.CollectItem(1);
                isPickedUp = true;
                Destroy(gameObject);
            }
        }
    }
}