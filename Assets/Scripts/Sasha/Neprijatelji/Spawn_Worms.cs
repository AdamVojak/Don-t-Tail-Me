using UnityEngine;
using System.Collections;

public class Worms_Spawner : MonoBehaviour
{
    [Header("Postavke Spawnera")]
    public GameObject Prefab;             // Stvarni neprijatelj
    public GameObject spawnVisual;        // Sprite crva koji raste pri spawnanju
    public float scaleUpDuration = 0.5f;  // Koliko traje animacija rasta crva
    public float respawnDelay = 3.0f;     // Ukupno vrijeme do novog spawna (mora biti veće od 0.5s)

    [Header("Vizualizacija Štoperice")]
    public GameObject stopwatchObject;        // Glavni kontejner štoperice (pozadina, kazaljka...)
    public Transform stopwatchHand;          // Transform kazaljke
    public SpriteRenderer stopwatchBackground; // Pozadina štoperice

    [Header("Okidači i Prepreke")]
    [SerializeField] private SpawnTrigger triggerScript;
    public float snapRadius = 0.45f;         // Unutar ovog radijusa prepreka se snap-a na centar rupe
    public GuideArrowSasha activeArrow;

    private GameObject current;
    private bool isBlocked = false;
    private bool isActivated = false;
    private bool isPermanentlyBlocked = false;

    private GameObject nearbyObstacle;       // Prepreka koja je trenutno u dometu spawnera

    void Start()
    {
        if (spawnVisual != null)
        {
            spawnVisual.SetActive(false);
        }

        if (stopwatchObject != null)
        {
            stopwatchObject.SetActive(false);
        }

        if (triggerScript == null)
        {
            triggerScript = GetComponentInChildren<SpawnTrigger>();
        }
    }

    void Update()
    {
        if (isPermanentlyBlocked) return;

        if (nearbyObstacle != null)
        {
            float distance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(nearbyObstacle.transform.position.x, nearbyObstacle.transform.position.y)
            );

            if (distance <= snapRadius)
            {
                isPermanentlyBlocked = true;
                isBlocked = true;

                // --- NOVO: Prisili Sashu da pusti kutiju PRIJE nego započne privlačenje ---
                SashaController sasha = FindFirstObjectByType<SashaController>();
                if (sasha != null)
                {
                    sasha.StopPushing();
                }

                StartCoroutine(PermanentBlockRoutine(nearbyObstacle));
                nearbyObstacle = null;
            }
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

            if (activeArrow != null)
            {
                activeArrow.SakrijStrelicu();
                activeArrow = null;
            }

            // Prije stvaranja novog crva, očisti i izblijedi sve stare prepreke u blizini
            CleanUpOldObstacles();

            // --- ANIMACIJA RASTA CRVA ---
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

            // Stvaranje stvarnog neprijatelja
            current = Instantiate(Prefab, transform.position, transform.rotation);

            // Čekamo dok je neprijatelj živ
            while (current != null)
            {
                yield return null;
            }

            // --- LOGIKA ŠTOPERICE ---
            if (stopwatchObject != null && !isBlocked)
            {
                Debug.Log("Worms_Spawner: Aktiviram štopericu na odbrojavanje...");
                activeArrow = PronadiStrelicuNaPrepreci();

                if (activeArrow != null)
                {
                    activeArrow.PostaviCilj(transform.position, snapRadius);
                }

                stopwatchObject.SetActive(true);
                if (stopwatchBackground != null) stopwatchBackground.color = Color.white;

                float elapsed = 0f;
                while (elapsed < respawnDelay)
                {
                    if (isBlocked)
                    {
                        stopwatchObject.SetActive(false);
                        yield break;
                    }

                    elapsed += Time.deltaTime;
                    float percentage = elapsed / respawnDelay;

                    if (stopwatchHand != null)
                    {
                        float currentAngle = Mathf.Lerp(0f, -360f, percentage);
                        stopwatchHand.localRotation = Quaternion.Euler(0, 0, currentAngle);
                    }

                    if (stopwatchBackground != null && (respawnDelay - elapsed) <= 0.5f)
                    {
                        stopwatchBackground.color = Color.red;
                    }

                    yield return null;
                }

                stopwatchObject.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(respawnDelay);
            }
        }
    }

    // Čišćenje starih prepreka oko ove rupe
    private void CleanUpOldObstacles()
    {
        GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in allObstacles)
        {
            if (obs == null || obs == nearbyObstacle) continue;

            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(obs.transform.position.x, obs.transform.position.y)
            );

            // Ako je stara prepreka u radijusu od 5 metara oko ove rupe, pokreni fade-out
            if (dist < 5.0f)
            {
                StartCoroutine(FadeAndDestroyObstacle(obs));
            }
        }
    }

    // Coroutine za onemogućavanje collidera, fade-out materijala i uništavanje
    IEnumerator FadeAndDestroyObstacle(GameObject obstacle)
    {
        Debug.Log("Worms_Spawner: Čistim staru prepreku: " + obstacle.name);

        // 1. Odmah isključi sve collidere da više ne smeta igraču
        Collider[] colliders = obstacle.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // 2. Pronađi Renderer i izblijedi materijal
        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material; // Dohvaća instancu materijala
            Color initialColor = mat.color;

            float elapsed = 0f;
            float fadeDuration = 1.0f; // Trajanje blijedenja (1 sekunda)

            while (elapsed < fadeDuration)
            {
                if (obstacle == null) yield break;

                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                mat.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

                yield return null;
            }
        }

        // 3. Uništi objekt nakon blijedenja
        if (obstacle != null)
        {
            Destroy(obstacle);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            nearbyObstacle = other.gameObject;
            Debug.Log("Worms_Spawner: Prepreka je u dometu spawnera.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") && other.gameObject == nearbyObstacle)
        {
            nearbyObstacle = null;
            Debug.Log("Worms_Spawner: Prepreka je izašla iz dometa spawnera.");
        }
    }

    IEnumerator PermanentBlockRoutine(GameObject obstacle)
    {
        Debug.Log("Worms_Spawner: Pokrećem proces snap-anja prepreke (0.75s)...");

        if (activeArrow != null)
        {
            activeArrow.SakrijStrelicu();
            activeArrow = null;
        }

        if (obstacle != null)
        {
            Collider[] obstacleColliders = obstacle.GetComponentsInChildren<Collider>();
            foreach (Collider col in obstacleColliders)
            {
                col.enabled = false;
            }
        }

        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Prvo zaustavi brzinu dok je još dinamičan
            rb.isKinematic = true;            // Tek onda ga postavi na kinematic
        }

        Vector3 startPos = obstacle.transform.position;
        Vector3 targetPos = transform.position;

        targetPos.y = startPos.y; // Zadrži visinu

        float elapsed = 0f;
        float duration = 0.75f;

        // Glatko privlačenje u centar
        while (elapsed < duration)
        {
            if (obstacle == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            obstacle.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // Kada stigne u centar
        if (obstacle != null)
        {
            obstacle.transform.position = targetPos;
            obstacle.tag = "Untagged";
        }

        // Isključi sve collidere na samom spawneru (rupa je začupljena)
        Collider[] spawnerColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in spawnerColliders)
        {
            col.enabled = false;
        }

        Debug.Log("Worms_Spawner: Rupa je trajno blokirana. Gasim rad spawnera.");

        StopAllCoroutines();
        this.enabled = false;
    }

    private GuideArrowSasha PronadiStrelicuNaPrepreci()
    {
        // 1. Ako je prepreka već u triggeru
        if (nearbyObstacle != null)
        {
            GuideArrowSasha arrow = nearbyObstacle.GetComponentInChildren<GuideArrowSasha>();
            if (arrow != null) return arrow;
        }

        // 2. Ako nije, pretraži sve prepreke u krugu od 8 metara oko rupe
        GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in allObstacles)
        {
            if (obs == null) continue;

            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(obs.transform.position.x, obs.transform.position.y)
            );

            if (dist < 8.0f)
            {
                GuideArrowSasha arrow = obs.GetComponentInChildren<GuideArrowSasha>();
                if (arrow != null) return arrow;
            }
        }

        return null;
    }
}