using System.Collections;
using UnityEngine;

public class Ventilacija_In_Giovanni : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Animator fanAnimator; // Animator ventilatora
    [SerializeField] private Transform startPoint; // Dno cijevi (gdje se item pojavi)
    [SerializeField] private Transform endPoint;   // Vrh cijevi (gdje item nestaje)

    [Header("Postavke Animacije")]
    [SerializeField] private float normalFanSpeed = 1f;
    [SerializeField] private float fastFanSpeed = 5f;
    [SerializeField] private float fanTransitionTime = 1f; // Koliko brzo ventilator ubrza

    [Header("Postavke Ciklusa Slanja")]
    [SerializeField] private float vrijemeCekanjaNaDnu = 1f; // Koliko dugo stoji na dnu prije polijetanja

    [Header("Prefabi predmeta (Samo za vizualni efekt)")]
    public GameObject prefabObicanKljuc;     // ID 6
    public GameObject prefabMinigun; // ID 2
    public GameObject prefabArm;     // ID 3
    public GameObject prefabPajser;  // ID 5

    [Header("Postavke Kretanja Itema")]
    [SerializeField] private float initialItemSpeed = 1f;
    [SerializeField] private float itemAcceleration = 15f; // Koliko brzo ubrzava prema gore

    // Ovu metodu će pozvati ComputerTerminal
    public void PokreniAnimacijuSlanja(int itemID)
    {
        StartCoroutine(SendRoutine(itemID));
    }

    private IEnumerator SendRoutine(int itemID)
    {
        // 1. Ubrzaj ventilator
        StartCoroutine(LerpFanSpeed(normalFanSpeed, fastFanSpeed));

        // 2. Odaberi prefab na temelju ID-ja
        GameObject odabraniPrefab = null;
        if (itemID == 6) odabraniPrefab = prefabObicanKljuc;
        else if (itemID == 2) odabraniPrefab = prefabMinigun;
        else if (itemID == 3) odabraniPrefab = prefabArm;

        if (odabraniPrefab != null)
        {
            GameObject item = Instantiate(odabraniPrefab);

            PadObjekta fallScript = item.GetComponent<PadObjekta>();
            if (fallScript != null) fallScript.enabled = false;

            Collider col = item.GetComponent<Collider>();

            if (col != null){
                float pivotToBottom = item.transform.position.y - col.bounds.min.y;

                item.transform.position = new Vector3(startPoint.position.x, startPoint.position.y + pivotToBottom, startPoint.position.z);
            }
            else
            {
            item.transform.position = startPoint.position;
            }

            item.transform.rotation = odabraniPrefab.transform.rotation;

            if (vrijemeCekanjaNaDnu > 0f)
            {
                yield return new WaitForSeconds(vrijemeCekanjaNaDnu);
            }

            float currentSpeed = initialItemSpeed;
            while (item.transform.position.y < endPoint.position.y)
            {
                currentSpeed += itemAcceleration * Time.deltaTime;
                item.transform.Translate(Vector3.up * currentSpeed * Time.deltaTime, Space.World);
                yield return null;
            }

            Destroy(item);
        }
        else
        {
            Debug.LogWarning("Prefab za slanje nije dodijeljen u GiovanniTubeSender za ID: " + itemID);
        }


        StartCoroutine(LerpFanSpeed(fastFanSpeed, normalFanSpeed));
    }

    private IEnumerator LerpFanSpeed(float startSpeed, float targetSpeed)
    {
        float elapsed = 0f;
        while (elapsed < fanTransitionTime)
        {
            elapsed += Time.deltaTime;
                if (fanAnimator != null)
                {
                    fanAnimator.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / fanTransitionTime);
                }
            yield return null;
        }
            if (fanAnimator != null) fanAnimator.speed = targetSpeed;
    }
}