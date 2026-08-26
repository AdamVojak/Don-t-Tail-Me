using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Postavke Spawnera")]
    public GameObject Prefab;
    public float respawnDelay = 2.0f;
    [SerializeField] private int numberOfSpawns = 3;

    private GameObject current;
    private bool isRespawning = false;
    [HideInInspector] public bool isActive = false;

    private int sashaCount = 0;
    private bool needsInstantSpawn = false; // Oznaka za instantno stvaranje pri paljenju

    private SashaController sashaRef;


    private bool IsSashaActiveAndAlive()
    {
        if (sashaRef == null) sashaRef = FindFirstObjectByType<SashaController>();

        return sashaRef != null && sashaRef.isControlled && sashaRef.currentState != SashaController.SashaState.Dead;
    }

    // Metoda za paljenje/gašenje spawnera
    public void SetActiveState(bool state)
    {
        bool prevState = isActive;
        isActive = state;

        // Ako se spawner TKOJEST UPALIO i nema trenutnog neprijatelja -> označi da prvi ide ODMAH
        if (!prevState && isActive && current == null)
        {
            needsInstantSpawn = true;
        }
    }

    void Update()
    {
        if (!IsSashaActiveAndAlive()) return;

        if (isActive && current == null && !isRespawning)
        {
            if (numberOfSpawns > 0)
            {
                // Spawna se samo ako Sasha NIJE u triggeru
                if (sashaCount == 0)
                {
                    if (needsInstantSpawn)
                    {
                        // Prvi zombi nakon paljenja nastaje ODMAH!
                        needsInstantSpawn = false;
                        Spawn();
                    }
                    else
                    {
                        // Svaki sljedeći zombi čeka respawnDelay
                        StartCoroutine(RespawnTimer());
                    }
                }
            }
            else
            {
                this.enabled = false;
            }
        }
    }

    void Spawn()
    {
        if (numberOfSpawns > 0 && sashaCount == 0)
        {
            numberOfSpawns--;
            current = Instantiate(Prefab, transform.position, transform.rotation);
        }
    }

    IEnumerator RespawnTimer()
    {
        isRespawning = true;

        yield return new WaitForSeconds(respawnDelay);

        // Čeka sve dok je Sasha unutar triggera ILI dok Sasha uopće nije aktivan
        while (sashaCount > 0 || !IsSashaActiveAndAlive())
        {
            yield return null;
        }

        if (isActive && current == null && IsSashaActiveAndAlive())
        {
            Spawn();
        }

        isRespawning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            sashaCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            sashaCount = Mathf.Max(0, sashaCount - 1);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isRespawning = false;
    }
}