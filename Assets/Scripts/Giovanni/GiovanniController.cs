using Unity.Cinemachine;
using UnityEngine;
using static MirandaController;

public class GiovanniController : MonoBehaviour
{
    public enum GiovanniState { Active, Dead }

    [Header("Stanje Igrača")]
    public GiovanniState currentState = GiovanniState.Active;

    [Header("UI Reference")]
    public GameObject deathUI;
    public DeathScreenGiovanni deathScreen;
    private bool deathScreenTriggered = false;

    [Header("Kretanje (Tank Controls)")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 150f;
    public bool isControlled = false;

    [Header("Svijetiljka")]
    public Transform flashlightHolder;
    public Light flashlightLight;
    public float flashlightRotationSpeed = 15f;
    private KeyCode flashlightToggleKey = KeyCode.F;
    public GameObject flashlightBeamObject;

    [Header("Fizika")]
    public float gravity = -9.81f;
    private float verticalVelocity;
    private CharacterController controller;

    [Header("Stats/States")]
    private GiovanniStats stats;
    private TijeloGiovanni tijeloGiovanniRef;
    public bool uhvacen;

    [Header("Kamera")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private CinemachineCamera virtualCamera;

    [Header("Audio")]
    [SerializeField] private GiovanniAudio giovanniAudio;

    [Header("Postavke Koraka")]
    [SerializeField] private float walkStepInterval = 0.55f;
    [SerializeField] private float sprintStepInterval = 0.35f;
    private float stepTimer = 0f;
    private int currentFoot = 0;

    private Camera mainCamera;

    void Awake()
    {
        if (giovanniAudio == null) giovanniAudio = GetComponent<GiovanniAudio>();
        controller = GetComponent<CharacterController>();
        stats = GetComponent<GiovanniStats>();
        mainCamera = Camera.main;

        if (cameraRoot == null) cameraRoot = transform.Find("CameraPoint");
        if (virtualCamera != null && cameraRoot != null)
        {
            virtualCamera.Follow = cameraRoot;
            virtualCamera.LookAt = cameraPoint;
        }
        deathUI.SetActive(false);
    }

    private void Start()
    {
        if (tijeloGiovanniRef == null)
        {
            tijeloGiovanniRef = FindFirstObjectByType<TijeloGiovanni>();
        }

        if (isControlled && currentState != GiovanniState.Dead)
        {
            if (giovanniAudio != null) giovanniAudio.StartBrownNoise();
        }

        flashlightLight.enabled = false;
        flashlightBeamObject.SetActive(flashlightLight.enabled);
    }

    void Update()
    {
        uhvacen = tijeloGiovanniRef.uhvacen;

        if (currentState != GiovanniState.Dead)
        {
            if (uhvacen)
            {
                Die(1);
            }
        }

        bool uTranziciji = (GameManager.Instance != null && GameManager.Instance.isLoading);

        // Zvuk kontroliramo odvojeno, bez prekidanja Update metode!
        if (currentState == GiovanniState.Dead || !isControlled || uTranziciji)
        {
            if (giovanniAudio != null) giovanniAudio.StopBrownNoise();
        }
        else
        {
            if (giovanniAudio != null) giovanniAudio.StartBrownNoise();
        }

        // FIZIKA I KRETANJE SE SADA UVIJEK IZVRŠAVAJU!
        HandleMovement();

        // Svjetiljku možemo paliti samo ako imamo kontrolu i nismo u tranziciji
        if (isControlled && !uTranziciji && Input.GetKeyDown(flashlightToggleKey))
        {
            if (flashlightLight != null)
            {
                flashlightLight.enabled = !flashlightLight.enabled;

                if (giovanniAudio != null) giovanniAudio.PlayFlashlightClick();

                if (flashlightBeamObject != null)
                    flashlightBeamObject.SetActive(flashlightLight.enabled);
            }
        }
    }

    void LateUpdate()
    {
        if (!isControlled || flashlightHolder == null) return;

        HandleFlashlightRotation();

        if (flashlightLight.enabled && flashlightBeamObject != null)
        {
            SyncFlashlightBeam();
        }
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;
        bool wantsToSprint = false;

        bool uTranziciji = (GameManager.Instance != null && GameManager.Instance.isLoading);

        // Čitamo tipke SAMO ako imamo kontrolu i NISMO u tranziciji
        if (isControlled && !uTranziciji)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            wantsToSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        bool isMoving = Mathf.Abs(verticalInput) > 0.01f;
        bool isSprinting = wantsToSprint && stats.CanSprint();

        bool isFlashlightOn = flashlightLight != null && flashlightLight.enabled;

        stats.UpdateStats(isMoving, isSprinting, isFlashlightOn);

        float activeMoveSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        float rotation = horizontalInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);

        Vector3 moveDirection = transform.forward * verticalInput * activeMoveSpeed;

        if (!controller.isGrounded)
            verticalVelocity += gravity * Time.deltaTime;
        else
            verticalVelocity = -2f;

        moveDirection.y = verticalVelocity;

        controller.Move(moveDirection * Time.deltaTime);

        // --- PROCEDURALNI KORACI (LIJEVA / DESNA NOGA) ---
        if (isMoving && controller.isGrounded)
        {
            float currentInterval = isSprinting ? sprintStepInterval : walkStepInterval;
            stepTimer += Time.deltaTime;

            if (stepTimer >= currentInterval)
            {
                stepTimer = 0f;

                if (giovanniAudio != null)
                {
                    giovanniAudio.PlayFootstep(currentFoot);
                }

                // Izmjena noge: ako je bila 0 prebaci na 1, ako je bila 1 prebaci na 0
                currentFoot = (currentFoot == 0) ? 1 : 0;
            }
        }
        else
        {
            // Čim stane, resetiraj tajmer i kreni opet od prve noge
            stepTimer = 0f;
            currentFoot = 0;
        }
    }

    void HandleFlashlightRotation()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;


        // Maska koja ignorira Player (6) I SVE što je na Ignore Raycast layeru (2)
        int layerMask = ~((1 << 6) | (1 << 2));

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 direction = (targetPoint - flashlightHolder.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Povećaj brzinu na 60f da bi praćenje bilo instantno
            flashlightHolder.rotation = Quaternion.Slerp(
                flashlightHolder.rotation,
                targetRotation,
                Time.deltaTime * 100f
            );
        }
    }

    public void SetLock(bool locked)
    {
        isControlled = !locked;
        if (locked && controller != null)
        {
            controller.Move(Vector3.zero);
        }
    }

    void SyncFlashlightBeam()
    {
        float range = flashlightLight.range;
        float angleInRadians = (flashlightLight.spotAngle / 2) * Mathf.Deg2Rad;
        float radius = Mathf.Tan(angleInRadians) * range;

        flashlightBeamObject.transform.localScale = new Vector3(radius * 2, radius * 2, range);

        flashlightBeamObject.transform.localRotation = Quaternion.identity;

        float zOffset = (range / 2f) + 0.1f;
        flashlightBeamObject.transform.localPosition = new Vector3(0, 0, zOffset);
    }

    public void Die(int cause)
    {
        if (currentState == GiovanniState.Dead) return;

        currentState = GiovanniState.Dead;
        flashlightLight.enabled = false;

        if (giovanniAudio != null)
        {
            giovanniAudio.StopBrownNoise(instant: true);

            if (cause == 0)
            {
                giovanniAudio.PlayBombExplosion();
            }
        }

        if (!deathScreenTriggered && deathScreen != null)
        {
            deathUI.SetActive(true);
            deathScreenTriggered = true;
            deathScreen.ShowDeathScreen(cause);
        }
    }

    // POPRAVLJENO: Pomiče točku gledanja 15m u visinu tako da Cinemachine MORA podići pogled!
    public System.Collections.IEnumerator LookUpAndAimLightRoutine(float duration)
    {
        // 1. Palimo i pojačavamo svjetiljku kroz maglu
        if (flashlightLight != null)
        {
            flashlightLight.enabled = true;
            flashlightLight.range = 35f;
            flashlightLight.intensity = 3.5f;
        }
        if (flashlightBeamObject != null) flashlightBeamObject.SetActive(true);

        float elapsed = 0f;

        // Pamtimo početne pozicije i rotacije
        Vector3 startLookPos = cameraPoint != null ? cameraPoint.position : transform.position + transform.forward * 5f;

        // CILJ: Točka gledanja leti 15 metara ravno uvis iznad Giovannija!
        Vector3 targetLookPos = transform.position + Vector3.up * 15f + transform.forward * 1.5f;

        Quaternion startRootRot = cameraRoot != null ? cameraRoot.localRotation : Quaternion.identity;
        Quaternion targetRot = Quaternion.Euler(-75f, 0f, 0f); // 75 stupnjeva prema gore
        Quaternion startFlashlightRot = flashlightHolder != null ? flashlightHolder.localRotation : Quaternion.identity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // A) PODIŽEMO TOČKU U ZRAK: Cinemachine LookAt prati ovu točku i DIŽE POGLED U STROP!
            if (cameraPoint != null)
            {
                cameraPoint.position = Vector3.Lerp(startLookPos, targetLookPos, t);
            }

            // B) Naginjemo i cameraRoot za svaki slučaj ako Cinemachine prati rotaciju
            if (cameraRoot != null)
            {
                cameraRoot.localRotation = Quaternion.Slerp(startRootRot, targetRot, t);
            }

            // C) Baterija prati pogled i svijetli ravno u lignju
            if (flashlightHolder != null)
            {
                flashlightHolder.localRotation = Quaternion.Slerp(startFlashlightRot, targetRot, t);
            }

            yield return null;
        }
    }

    private void OnDisable()
    {
        if (giovanniAudio != null) giovanniAudio.StopBrownNoise();
    }

    void OnEnable()
    {
        if (giovanniAudio == null) giovanniAudio = GetComponent<GiovanniAudio>();

        if (currentState != GiovanniState.Dead && isControlled)
        {
            if (giovanniAudio != null) giovanniAudio.StartBrownNoise();
        }
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        if (giovanniAudio != null)
        {
            if (isControlled && currentState != GiovanniState.Dead)
            {
                giovanniAudio.StartBrownNoise(); // Pali ambijent kad preuzmeš kontrolu
            }
            else
            {
                giovanniAudio.StopBrownNoise(); // Gasi ambijent kad prebaciš na drugog lika
            }
        }
    }
}