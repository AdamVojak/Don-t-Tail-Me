using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;

public class TranzicijaSoba : MonoBehaviour
{
    [Header("Soba A (npr. Lijeva/Donja)")]
    public GameObject roomA;
    public CinemachineCamera cameraA;
    public Transform spawnPointA;

    [Header("Soba B (npr. Desna/Gornja)")]
    public GameObject roomB;
    public CinemachineCamera cameraB;
    public Transform spawnPointB;

    [Header("UI Postavke")]
    public Image fadeImage;
    public float fadeSpeed = 3f;
    public float pauzaUMraku = 0.5f; // KOLIKO DUGO EKRAN OSTAJE CRN

    private GameManager gameManager;
    private static bool isTransitioning = false;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;

        SashaController sashaGlavni = other.GetComponentInParent<SashaController>();

        if (sashaGlavni != null)
        {
            if (gameManager == null) return;

            if (gameManager.sashaCam == cameraA)
            {
                StartCoroutine(Tranzicija(roomB, cameraB, spawnPointB, roomA, sashaGlavni.gameObject));
            }
            else if (gameManager.sashaCam == cameraB)
            {
                StartCoroutine(Tranzicija(roomA, cameraA, spawnPointA, roomB, sashaGlavni.gameObject));
            }
        }
    }

    private IEnumerator Tranzicija(GameObject novaSoba, CinemachineCamera novaKamera, Transform noviSpawn, GameObject staraSoba, GameObject cijeliSasha)
    {
        isTransitioning = true;

        // 1. FADE OUT (Zacrnjivanje)
        if (fadeImage != null)
        {
            while (fadeImage.color.a < 1f)
            {
                Color c = fadeImage.color;
                c.a += Time.deltaTime * fadeSpeed;
                fadeImage.color = c;
                yield return null;
            }
        }

        // --- SADA JE EKRAN 100% CRN ---

        // 2. Upali novu sobu
        if (novaSoba != null) novaSoba.SetActive(true);

        // 3. Teleportiraj lika
        CharacterController cc = cijeliSasha.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        cijeliSasha.transform.position = noviSpawn.position;
        cijeliSasha.transform.rotation = noviSpawn.rotation;

        if (cc != null) cc.enabled = true;

        // 4. Prebaci kamere
        if (gameManager.sashaCam != null) gameManager.sashaCam.Priority = 0;
        gameManager.sashaCam = novaKamera;
        if (gameManager.currChar == GameManager.ActiveCharacter.Sasha)
        {
            gameManager.sashaCam.Priority = 10;
        }

        // 5. Ugasi staru sobu
        if (staraSoba != null) staraSoba.SetActive(false);

        // 6. PAUZA U MRAKU (Čekamo da se kamera i fizika potpuno smire)
        yield return new WaitForSeconds(pauzaUMraku);

        // --- VRAĆAMO SLIKU ---

        // 7. FADE IN (Osvjetljavanje)
        if (fadeImage != null)
        {
            while (fadeImage.color.a > 0f)
            {
                Color c = fadeImage.color;
                c.a -= Time.deltaTime * fadeSpeed;
                fadeImage.color = c;
                yield return null;
            }
        }

        isTransitioning = false;
    }
}