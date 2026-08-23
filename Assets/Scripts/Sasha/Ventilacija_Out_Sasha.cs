using System.Collections;
using UnityEngine;

public class Ventilacija_Out_Sasha : MonoBehaviour
{
    [Header("Postavke ispadanja")]
    public Transform tockaIzlaska;
    public Transform tockaPadanja;
    public float trajanjeLeta = 0.5f; // Koliko dugo traje let do poda

    [Header("Prefabi predmeta (Za Sashu)")]
    public GameObject prefabGun;      // ID 1
    public GameObject prefabMinigun;  // ID 2
    public GameObject prefabPajser;   // ID 5

    //private bool imaItemNaCekanju = false;
    private int cekajuciItemTip;
    public bool obrniSmjerLeta = false;
    private GameObject trenutniStvoreniItem;

    // Cijev je slobodna ako GameManager kaže da nitko ne čeka i ako na podu nema itema
    public bool MozePrimiti()
    {
        return VentNetworkManager.Instance != null && !VentNetworkManager.Instance.sashaCekaItem && trenutniStvoreniItem == null;
    }

    public void SpremiItemZaSashu(int tip)
    {
        // Spremamo u VentNetworkManager kako bi BILO KOJA soba znala za item!
        if (VentNetworkManager.Instance != null)
        {
            VentNetworkManager.Instance.sashaCekaItem = true;
            VentNetworkManager.Instance.sashaPendingItemID = tip;
        }
        Debug.Log("Item ID: " + tip + " čeka Sashu u globalnoj cijevi.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kada Sasha priđe BILO KOJOJ izlaznoj ventilaciji u sceni
        if (VentNetworkManager.Instance != null && VentNetworkManager.Instance.sashaCekaItem && other.CompareTag("Sasha"))
        {
            cekajuciItemTip = VentNetworkManager.Instance.sashaPendingItemID;
            VentNetworkManager.Instance.sashaCekaItem = false; // Cijev se prazni
            VentNetworkManager.Instance.sashaPendingItemID = -1;

            IzbaciItem();
        }
    }

    private void IzbaciItem()
    {
        GameObject odabraniPrefab = null;
        if (cekajuciItemTip == 1) odabraniPrefab = prefabGun;
        else if (cekajuciItemTip == 2) odabraniPrefab = prefabMinigun;
        else if (cekajuciItemTip == 5) odabraniPrefab = prefabPajser;

        if (odabraniPrefab != null)
        {
            // Stvaramo prefab na točki izlaska
            trenutniStvoreniItem = Instantiate(odabraniPrefab, tockaIzlaska.position, odabraniPrefab.transform.rotation);

            // Pokrećemo animaciju leta i rotacije
            StartCoroutine(AnimacijaLeta(trenutniStvoreniItem));
        }
        else
        {
            Debug.LogWarning("Prefab za ID " + cekajuciItemTip + " nije dodijeljen!");
        }
    }

    private IEnumerator AnimacijaLeta(GameObject item)
    {
        // Ako je smjer kriv, skripta automatski mijenja mjesta startu i cilju
        Vector3 startPos = obrniSmjerLeta ? tockaPadanja.position : tockaIzlaska.position;
        Vector3 endPos = obrniSmjerLeta ? tockaIzlaska.position : tockaPadanja.position;

        // Fiksiramo Z os kako predmet ne bi propao iza 2D pozadine
        endPos.z = startPos.z;

        // Postavljamo predmet na točan start
        item.transform.position = startPos;

        float protekloVrijeme = 0f;

        while (protekloVrijeme < trajanjeLeta)
        {
            if (item == null) yield break;

            float postotak = protekloVrijeme / trajanjeLeta;
            float smoothPostotak = Mathf.SmoothStep(0f, 1f, postotak);

            item.transform.position = Vector3.Lerp(startPos, endPos, smoothPostotak);

            protekloVrijeme += Time.deltaTime;
            yield return null;
        }

        if (item != null)
        {
            item.transform.position = endPos;
            Debug.Log("Item je iskliznuo iz ventilacije kod Sashe.");
        }
    }
}