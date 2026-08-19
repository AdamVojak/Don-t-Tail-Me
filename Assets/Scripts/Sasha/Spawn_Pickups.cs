using UnityEngine;
using System.Collections;

public class SpawnerPickups : MonoBehaviour
{
    [Header("Postavke Spawnera")]
    public GameObject Prefab;
    public float respawnDelay = 2.0f;
    [SerializeField] private int numberOfSpawns = 3;

    [Header("Postavke Blicanja Svjetla")]
    public Color agresivnaCrvena = new Color(1f, 0f, 0f); // Jarko crvena
    public float brzinaBlicanja = 0.15f; // Koliko brzo izmjenjuje boje (u sekundama)

    private GameObject current;
    private bool isRespawning = false;
    private bool isSashaInZone = false; // Pratimo je li Sasha u trigger prostoru

    // Reference za svjetlo
    private Light spawnerLight;
    private Color originalnaBoja;
    private Coroutine blinkCoroutine;

    void Start()
    {
        // Automatski tražimo dijete s imenom "Light" ili "light"
        Transform lightChild = transform.Find("Light");

        if (lightChild != null)
        {
            spawnerLight = lightChild.GetComponent<Light>();
        }
        else
        {
            // Ako nije nađeno po imenu, tražimo općenito bilo koju Light komponentu u djeci
            spawnerLight = GetComponentInChildren<Light>();
        }

        // Ako smo našli svjetlo, spremamo njegovu početnu boju
        if (spawnerLight != null)
        {
            originalnaBoja = spawnerLight.color;
        }
    }

    void OnEnable()
    {
        isRespawning = false;

        // Ako nemamo trenutni pickup, imamo još spawnova i Sasha NIJE u zoni, stvori ga odmah
        if (current == null && numberOfSpawns > 0 && !isSashaInZone)
        {
            Spawn();
        }
    }

    void OnDisable()
    {
        // Zaustavi blicanje i vrati boju ako se soba ugasi
        ZaustaviBlicanje();

        if (current != null)
        {
            Destroy(current);
            current = null;
            numberOfSpawns++;
        }

        isRespawning = false;
        isSashaInZone = false;
    }

    void Update()
    {
        if (current == null && !isRespawning)
        {
            if (numberOfSpawns > 0)
            {
                StartCoroutine(RespawnTimer());
            }
            else
            {
                this.enabled = false;
            }
        }
    }

    void Spawn()
    {
        if (numberOfSpawns > 0 && !isSashaInZone)
        {
            numberOfSpawns--;
            current = Instantiate(Prefab, transform.position, transform.rotation);
            current.transform.SetParent(this.transform);
        }
    }

    IEnumerator RespawnTimer()
    {
        isRespawning = true;

        // 1. Čekamo zadano vrijeme za respawn
        yield return new WaitForSeconds(respawnDelay);

        // 2. PAUZIRANJE I BLICANJE: Ako je Sasha u zoni, pokreni blicanje i čekaj
        if (isSashaInZone)
        {
            PokreniBlicanje();

            // Čekamo sve dok je Sasha unutra
            while (isSashaInZone)
            {
                yield return null;
            }

            // Sasha je izašla -> zaustavi blicanje
            ZaustaviBlicanje();
        }

        // 3. Stvaramo novi objekt
        Spawn();

        isRespawning = false;
    }

    // ==========================================
    // LOGIKA ZA BLICANJE SVJETLA
    // ==========================================

    void PokreniBlicanje()
    {
        if (spawnerLight != null && blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkLightCoroutine());
        }
    }

    void ZaustaviBlicanje()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // Obavezno vraćamo svjetlo na njegovu originalnu boju
        if (spawnerLight != null)
        {
            spawnerLight.color = originalnaBoja;
        }
    }

    IEnumerator BlinkLightCoroutine()
    {
        while (true)
        {
            spawnerLight.color = agresivnaCrvena;
            yield return new WaitForSeconds(brzinaBlicanja);

            spawnerLight.color = originalnaBoja;
            yield return new WaitForSeconds(brzinaBlicanja);
        }
    }

    // ==========================================
    // DETEKCIJA SASHE UNUTAR TRIGGER PROSTORA
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            isSashaInZone = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            isSashaInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            isSashaInZone = false;
        }
    }
}