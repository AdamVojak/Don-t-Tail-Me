using UnityEngine;
using System.Collections;

public class Worms_Spawner : MonoBehaviour
{
    [Header("Postavke Spawnera")]
    public GameObject Prefab;
    public GameObject spawnVisual;
    public float scaleUpDuration = 0.5f;
    public float respawnDelay = 2.0f;

    [Header("Okidači i Prepreke")]
    [SerializeField] private SpawnTrigger triggerScript;

    private GameObject current;
    private bool isBlocked = false;
    private bool isActivated = false;
    private bool isPermanentlyBlocked = false;

    void Start()
    {
        if (spawnVisual != null)
        {
            spawnVisual.SetActive(false);
        }

        if (triggerScript == null)
        {
            triggerScript = GetComponentInChildren<SpawnTrigger>();
        }
    }

    public void ActivateSpawner()
    {
        if (!isActivated && !isPermanentlyBlocked)
        {
            isActivated = true;
            StartCoroutine(SpawnerLoop());
        }
    }

    IEnumerator SpawnerLoop()
    {
        while (true)
        {
            while (isBlocked)
            {
                yield return null;
            }

            if (spawnVisual != null)
            {
                Vector3 initialScale = new Vector3(0.00001f, 0.00001f, 0.00001f);
                Vector3 targetScale = Vector3.one;
                spawnVisual.transform.localScale = initialScale;
                spawnVisual.SetActive(true);

                float elapsedTime = 0f;
                while (elapsedTime < scaleUpDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float percentage = elapsedTime / scaleUpDuration;
                    spawnVisual.transform.localScale = Vector3.Lerp(initialScale, targetScale, percentage);
                    yield return null;
                }

                spawnVisual.transform.localScale = targetScale;
                spawnVisual.SetActive(false);
            }

            current = Instantiate(Prefab, transform.position, transform.rotation);

            while (current != null)
            {
                yield return null;
            }

            yield return new WaitForSeconds(respawnDelay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPermanentlyBlocked && other.CompareTag("Obstacle"))
        {
            isPermanentlyBlocked = true;
            isBlocked = true;

            StartCoroutine(PermanentBlockRoutine(other.gameObject));
        }
    }

    IEnumerator PermanentBlockRoutine(GameObject obstacle)
    {
        Debug.Log("Worms_Spawner: Prepreka detektirana. Započinjem proces zatvaranja rupe (0.75s)...");

        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        Vector3 startPos = obstacle.transform.position;
        Vector3 targetPos = transform.position;

        targetPos.y = startPos.y;

        float elapsed = 0f;
        float duration = 0.75f;

        while (elapsed < duration)
        {
            if (obstacle == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            obstacle.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (obstacle != null)
        {
            obstacle.transform.position = targetPos;
            obstacle.tag = "Untagged";
        }

        Debug.Log("Worms_Spawner: Rupa je trajno blokirana. Gasim rad spawnera.");

        StopAllCoroutines();

        this.enabled = false;
    }
}