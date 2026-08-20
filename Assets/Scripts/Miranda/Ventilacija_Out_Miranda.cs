using System.Collections;
using UnityEngine;

public class Ventilacija_Out_Miranda : MonoBehaviour
{
    [Header("Postavke ispadanja")]
    public Transform tockaIzlaska;
    public Transform tockaPadanja;

    [Header("Prefabi predmeta")]
    public GameObject prefabKey;
    public GameObject prefabGun;
    public GameObject prefabMinigun;
    public GameObject prefabArm;
    public GameObject prefabPajser;

    [Header("Animacija padanja")]
    public float trajanjePadanja = 0.3f;

    [Header("Sustav primanja item-a")]
    private bool imaItemNaCekanju = false;
    private int cekajuciItemTip;
    private Sprite cekajuciItemSprite;
    private GameObject trenutniStvoreniItem;


    public SFX sfx;
    private AudioSource zvukPadanja;
    private AudioSource zvukIzlaska;
    private AudioSource zvukMetalnogPoda;

    public bool MozePrimiti()
    {
        return !imaItemNaCekanju && trenutniStvoreniItem == null;
    }

    private void Start()
    {
        if (sfx == null)
        {
            sfx = FindFirstObjectByType<SFX>();
        }
        zvukPadanja = sfx.ZvukPadanjaKrozVentilacije;
        zvukIzlaska = sfx.ZvukIzlaskaIzVentilacije;
        zvukMetalnogPoda = sfx.ZvukMetalnogPoda;
    }

    public void SpremiItemZaMirandu(int tip)
    {
        imaItemNaCekanju = true;
        cekajuciItemTip = tip;
        Debug.Log("Ventilacija spremna za izbacivanje itema ID: " + tip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (imaItemNaCekanju && other.CompareTag("Miranda"))
        {
            imaItemNaCekanju = false;
            StartCoroutine(ProcesIzbacivanjaSaZvukom());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (imaItemNaCekanju && other.CompareTag("Miranda"))
        {
            imaItemNaCekanju = false;
            StartCoroutine(ProcesIzbacivanjaSaZvukom());
        }
    }

    private IEnumerator ProcesIzbacivanjaSaZvukom()
    {
        if (zvukPadanja != null)
        {
            zvukPadanja.Play();
            yield return new WaitForSeconds(zvukPadanja.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        GameObject odabraniPrefab = null;
        if (cekajuciItemTip == 0) odabraniPrefab = prefabKey;
        else if (cekajuciItemTip == 1) odabraniPrefab = prefabGun;
        else if (cekajuciItemTip == 2) odabraniPrefab = prefabMinigun;
        else if (cekajuciItemTip == 3) odabraniPrefab = prefabArm;
        else if (cekajuciItemTip == 5) odabraniPrefab = prefabPajser;

        if (odabraniPrefab != null)
        {
            trenutniStvoreniItem = Instantiate(odabraniPrefab);

            trenutniStvoreniItem.transform.position = tockaIzlaska.position;
            trenutniStvoreniItem.transform.rotation = odabraniPrefab.transform.rotation;

            if (zvukIzlaska != null) zvukIzlaska.Play();

            Debug.Log("Item je uspješno stvoren i prisilno postavljen na koordinate: " + trenutniStvoreniItem.transform.position);
        }
    }
}