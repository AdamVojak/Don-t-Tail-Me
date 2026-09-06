using System.Collections;
using UnityEngine;

public class AnglerFishController : MonoBehaviour
{
    [Header("Centar Mape i Granice (Dome-ovi)")]
    public Transform mapCenter;
    public float dome1Radius = 60f;   // Dome 1: Sigurna zona
    public float dome2Radius = 90f;   // Dome 2: 1 prelet
    public float dome3Radius = 120f;  // Dome 3: 2 preleta (preko ovoga = Smrt!)

    [Header("Vizualni 3D Model Ribe")]
    [Tooltip("Povuci ovdje 3D model ribe (ili child objekt) koji se pali i gasi")]
    [SerializeField] private GameObject visualModel;

    [Header("Reference Točaka (Vezane za Giovannija)")]
    [SerializeField] private Transform leftStart;
    [SerializeField] private Transform leftEnd;
    [SerializeField] private Transform frontStart;
    [SerializeField] private Transform frontEnd;
    [SerializeField] private Transform jumpscareStart;
    [SerializeField] private Transform cameraRoot;

    [Header("Reference")]
    [SerializeField] private GiovanniController giovanni;
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 180, 0);

    [Header("Postavke Brzine")]
    [SerializeField] private float zoomSpeed = 40f;
    [SerializeField] private float attackSpeed = 20f;

    [Header("Audio Postavke")]
    [Tooltip("Vlastiti AudioSource na ovom objektu")]
    [SerializeField] private AudioSource jumpscareAudioSource;
    [SerializeField] private AudioClip jumpscareClip; // Povuci ovdje zvučni clip krika
    [SerializeField] private GiovanniAudio giovanniAudio; // Zvuk preleta

    [SerializeField] private float glasnocaNapada = 0.85f; // Glasnoća napada (0.0 - 1.0)

    private bool isBusy = false;
    private int highestZoneReached = 0;

    public bool IsOutOfBounds
    {
        get
        {
            if (giovanni == null || mapCenter == null) return false;
            float dist = Vector2.Distance(new Vector2(giovanni.transform.position.x, giovanni.transform.position.z), new Vector2(mapCenter.position.x, mapCenter.position.z));
            return dist > dome1Radius;
        }
    }

    public Transform MapCenterTransform => mapCenter;

    void Start()
    {
        if (giovanni == null) giovanni = FindFirstObjectByType<GiovanniController>();

        if (giovanniAudio == null && giovanni != null)
        {
            giovanniAudio = giovanni.GetComponent<GiovanniAudio>();
        }

        // Uzimamo AudioSource s ovog glavnog objekta (koji je uvijek aktivan)
        if (jumpscareAudioSource == null)
        {
            jumpscareAudioSource = GetComponent<AudioSource>();
        }

        if (visualModel == null && transform.childCount > 0)
        {
            visualModel = transform.GetChild(0).gameObject;
        }

        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }
    }

    void Update()
    {
        if (giovanni == null || giovanni.currentState == GiovanniController.GiovanniState.Dead || isBusy || mapCenter == null) return;

        // Mjerimo udaljenost po XZ ravnini
        Vector2 flatPlayer = new Vector2(giovanni.transform.position.x, giovanni.transform.position.z);
        Vector2 flatCenter = new Vector2(mapCenter.position.x, mapCenter.position.z);
        float distance = Vector2.Distance(flatPlayer, flatCenter);

        // 1. IZA DOME 3 -> SMRTONOSNI NAPAD (Ubojstvo)
        if (distance > dome3Radius)
        {
            StartCoroutine(AttackRoutine());
        }
        // 2. DOME 3 -> SAMO DRUGI PRELET (Sprijeda)
        else if (distance > dome2Radius && highestZoneReached < 2)
        {
            highestZoneReached = 2;
            StartCoroutine(SecondFlybyRoutine());
        }
        // 3. DOME 2 -> SAMO PRVI PRELET (Slijeve strane)
        else if (distance > dome1Radius && highestZoneReached < 1)
        {
            highestZoneReached = 1;
            StartCoroutine(FirstFlybyRoutine());
        }
        // 4. POVRATAK U DOME 1 (SIGURNA ZONA) -> Resetiramo upozorenja!
        else if (distance <= dome1Radius && highestZoneReached > 0)
        {
            highestZoneReached = 0;
            Debug.Log("Giovanni se vratio u sigurnu zonu (Dome 1).");
        }
    }

    // DOME 2: Prvi prelet (samo lijeva strana)
    private IEnumerator FirstFlybyRoutine()
    {
        isBusy = true;
        if (visualModel != null) visualModel.SetActive(true);

        yield return StartCoroutine(ZoomBetweenPoints(leftStart, leftEnd));

        if (visualModel != null) visualModel.SetActive(false);
        isBusy = false;
    }

    // DOME 3: Samo drugi prelet (samo sprijeda)
    private IEnumerator SecondFlybyRoutine()
    {
        isBusy = true;
        if (visualModel != null) visualModel.SetActive(true);

        yield return StartCoroutine(ZoomBetweenPoints(frontStart, frontEnd));

        if (visualModel != null) visualModel.SetActive(false);
        isBusy = false;
    }


    // IZA DOME 3: Jumpscare ubojstvo
    private IEnumerator AttackRoutine()
    {
        isBusy = true;
        if (visualModel != null) visualModel.SetActive(true);

        // 1. SIGURAN ZVUK: Puštamo zvuk preko vlastitog AudioSource-a koji je 100% aktivan!
        if (jumpscareAudioSource != null)
        {
            if (jumpscareClip != null)
            {
                jumpscareAudioSource.volume = glasnocaNapada;
                jumpscareAudioSource.PlayOneShot(jumpscareClip);
            }
            else jumpscareAudioSource.Play();
        }

        float distance = Vector3.Distance(jumpscareStart.position, cameraRoot.position);
        float duration = distance / attackSpeed;
        float elapsed = 0;

        // Smjer leta prema kameri
        Vector3 attackDir = (cameraRoot.position - jumpscareStart.position).normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 targetPos = Vector3.Lerp(jumpscareStart.position, cameraRoot.position, t);
            transform.position = targetPos;

            // 2. NOVO: Matematički fiksirana rotacija s nagibom od -20 stupnjeva (Nema više LookAt bugova!)
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir)
                                     * Quaternion.Euler(forwardOffset)
                                     * Quaternion.Euler(-20f, 0f, 0f);
            }

            yield return null;
        }

        // 3. Ubija Giovannija (ID 2 = Angler Fish)
        giovanni.Die(2);

        if (visualModel != null) visualModel.SetActive(false);
        isBusy = false;
    }

    private IEnumerator ZoomBetweenPoints(Transform startT, Transform endT)
    {
        if (giovanniAudio != null) giovanniAudio.PlayViperFlyby();

        float distance = Vector3.Distance(startT.position, endT.position);
        float duration = distance / zoomSpeed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 targetPos = Vector3.Lerp(startT.position, endT.position, t);
            transform.position = targetPos;

            Vector3 lookTarget = endT.position;
            transform.LookAt(lookTarget);
            transform.Rotate(forwardOffset);

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (mapCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(mapCenter.position, dome1Radius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mapCenter.position, dome2Radius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mapCenter.position, dome3Radius);
        }
    }
}