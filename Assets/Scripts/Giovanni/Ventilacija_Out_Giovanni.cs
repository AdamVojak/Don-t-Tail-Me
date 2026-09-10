using System.Collections;
using UnityEngine;

public class Ventilacija_Out_Giovanni : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Animator fanAnimator;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Collider trigger;
    //[SerializeField] private Collider dno;
    [SerializeField] private Transform tubeTransform;
    private float ogTubeY;

    [Header("Prefabi predmeta (Interactable)")]
    public GameObject prefabObicanKljuc;
    public GameObject prefabMinigun;
    public GameObject prefabPajser;

    [Header("Postavke Animacije")]
    [SerializeField] private float normalFanSpeed = 1f;
    [SerializeField] private float fastFanSpeed = 5f;
    [SerializeField] private float fanTransitionTime = 1f;

    [Header("Postavke Kretanja Itema")]
    //[SerializeField] private float initialItemSpeed = 15f;
    //[SerializeField] private float itemDeceleration = 10f;

    [Header("Pobjednički Klimaks")]
    public GiovanniWinSequence winSequence;
    public MobitelTracker mobitelTracker;

    private bool imaItemNaCekanju = false;
    private int cekajuciItemTip;
    private bool isBusy = false;
    public bool MozePrimiti => !isBusy && !imaItemNaCekanju;
    public bool ImaAktivnogItema => imaItemNaCekanju || isBusy;

    void Start()
    {
        if (tubeTransform != null)
        {
            ogTubeY = tubeTransform.localPosition.y;
        }
    }

    public void SpremiItemZaGiovannia(int tip)
    {
        imaItemNaCekanju = true;
        cekajuciItemTip = tip;
        Debug.Log("Cijev spremna, čeka se da Giovanni priđe.");
    }

    public void PlayerStayedInTrigger(Collider other)
    {
        if (imaItemNaCekanju && !isBusy && other.CompareTag("Giovanni"))
        {
            if (winSequence == null) winSequence = FindFirstObjectByType<GiovanniWinSequence>();

            // =========================================================================
            // LIGNJA SE PALI ISKLJUČIVO AKO MU POŠALJU GUN (ID 1) - KRIVI PREDMET!
            // =========================================================================
            if (cekajuciItemTip == 1)
            {
                imaItemNaCekanju = false;
                isBusy = true;

                if (winSequence != null)
                {
                    winSequence.PokreniScenuLignje(false); // Lignja ga ubija (Fail)
                }
                else
                {
                    Debug.LogError("GiovanniWinSequence skripta NIJE pronađena na sceni!");
                }
                return;
            }

            // =========================================================================
            // ZA SVE OSTALE PREDMETE (Uključujući Običan Ključ ID 6): NORMALNA DOSTAVA NA POD!
            // =========================================================================
            imaItemNaCekanju = false;
            StartCoroutine(ReceiveRoutine(cekajuciItemTip));
        }
    }

    private IEnumerator ReceiveRoutine(int tip)
    {
        isBusy = true;
        StartCoroutine(LerpFanSpeed(normalFanSpeed, fastFanSpeed));

        GameObject odabraniPrefab = null;
        if (tip == 6) odabraniPrefab = prefabObicanKljuc; // KLJUČ PADA NA POD!
        else if (tip == 2) odabraniPrefab = prefabMinigun;
        else if (tip == 5) odabraniPrefab = prefabPajser;

        if (odabraniPrefab != null)
        {
            GameObject item = Instantiate(odabraniPrefab);

            PadObjekta fallScript = item.GetComponent<PadObjekta>();
            if (fallScript != null) fallScript.enabled = false;

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Vector3 pocetnaPozicija = startPoint.position;
            Vector3 ciljnaPozicija = endPoint.position;
            Collider col = item.GetComponent<Collider>();
            if (col != null)
            {
                float pivotToBottom = item.transform.position.y - col.bounds.min.y;
                ciljnaPozicija.y += pivotToBottom;
            }

            item.transform.position = pocetnaPozicija;

            float trajanjePuta = 1.0f;
            float protekloVrijeme = 0f;

            while (protekloVrijeme < trajanjePuta)
            {
                if (item == null) break;

                float postotak = protekloVrijeme / trajanjePuta;
                float smoothPostotak = Mathf.SmoothStep(0f, 1f, postotak);

                item.transform.position = Vector3.Lerp(pocetnaPozicija, ciljnaPozicija, smoothPostotak);

                protekloVrijeme += Time.deltaTime;
                yield return null;
            }

            if (item != null)
            {
                item.transform.position = ciljnaPozicija;
                if (rb != null) rb.isKinematic = false;
                if (fallScript != null) fallScript.enabled = true;
            }

            StartCoroutine(LerpFanSpeed(fastFanSpeed, normalFanSpeed));

            if (tubeTransform != null)
            {
                yield return StartCoroutine(MoveTube(ogTubeY + 0.0072f, 1f));
            }

            yield return new WaitUntil(() => item == null);

            if (tubeTransform != null)
            {
                StartCoroutine(MoveTube(ogTubeY, 1f));
            }
        }
        else
        {
            yield return StartCoroutine(LerpFanSpeed(fastFanSpeed, normalFanSpeed));
        }

        isBusy = false;
    }

    private IEnumerator LerpFanSpeed(float startSpeed, float targetSpeed)
    {
        float elapsed = 0f;
        while (elapsed < fanTransitionTime)
        {
            elapsed += Time.deltaTime;
            fanAnimator.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / fanTransitionTime);
            yield return null;
        }
        if (fanAnimator != null) fanAnimator.speed = targetSpeed;
    }

    private IEnumerator MoveTube(float targetY, float duration)
    {
        float elapsed = 0f;
        float startY = tubeTransform.localPosition.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY, targetY, elapsed / duration);
            tubeTransform.localPosition = new Vector3(tubeTransform.localPosition.x, newY, tubeTransform.localPosition.z);
            yield return null;
        }

        tubeTransform.localPosition = new Vector3(tubeTransform.localPosition.x, targetY, tubeTransform.localPosition.z);
    }
}