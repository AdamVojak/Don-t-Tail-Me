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

    private Camera mainCamera;

    void Awake()
    {
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
                Die(1); // 1 = Viper riba
            }
        }

        if (currentState == GiovanniState.Dead || !isControlled)
        {
            return;
        }

        HandleMovement();

        if (isControlled && Input.GetKeyDown(flashlightToggleKey))
        {
            if (flashlightLight != null)
            {
                flashlightLight.enabled = !flashlightLight.enabled;

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

        if (isControlled)
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
        if (!deathScreenTriggered && deathScreen!= null)
        {
            deathUI.SetActive(true);
            deathScreenTriggered = true;
            deathScreen.ShowDeathScreen(cause);
        }
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
    }
}