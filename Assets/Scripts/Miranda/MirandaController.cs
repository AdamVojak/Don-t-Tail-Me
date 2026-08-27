using UnityEngine;

public class MirandaController : MonoBehaviour
{
    public enum MirandaState { Active, Dead }
    public MirandaState currentState = MirandaState.Active;

    [Header("Kretanje (Inercija)")]
    public float normalMoveSpeed = 7f;
    public float sprintSpeed = 14f;
    public float sprintRotationMultiplier = 25f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float jumpForce = 5f;

    public float kutZvuka = 180f;
    private CharacterController controller;
    public bool isControlled = false;

    public float currentZSpeed = 0f;

    [Header("Vizuali (Djeca)")]
    public Transform mirandaTijelo;
    public Transform mirandaGlava;
    public GameObject mirandaRukaObjekt; // NOVO: Povuci objekt 'Miranda_Ruka' ovdje

    [Header("Ruka i Interakcije")]
    public bool isInteracting = false;
    public bool rukaAktivna = false;
    private MirandaInventory inventar;

    [Header("UI Reference")]
    public DeathScreenMiranda deathScreenManager;
    private bool deathScreenTriggered = false;
    public GameObject HintUI;

    public SpriteRenderer clickHint;

    public float rotationMultiplier = 60f;
    private float currentRotation = 0f;

    [Header("Fizika")]
    public float gravity = -15f;
    private float verticalVelocity;
    private bool isFalling = false; // Prati je li Miranda u fazi pada

    [Header("Audio")]
    [SerializeField] private MirandaAudio mirandaAudio;
    private float accumulatedRotation = 0f;

    void Start()
    {
        if (mirandaAudio == null) mirandaAudio = GetComponent<MirandaAudio>();
        controller = GetComponent<CharacterController>();
        inventar = GetComponent<MirandaInventory>();

        if (clickHint != null)
        {
            clickHint.enabled = false;
        }

        if (HintUI != null) HintUI.SetActive(false);

        // --- PROVJERA INVENTARA ODMAH NA STARTU ---
        // Ako nema ruku u inventaru (ili ako inventar još nije spreman), 
        // prisilno ugasi objekt ruke čak i ako je bio upaljen u Unity Editoru!
        bool posjedujeRuku = (inventar != null && inventar.ImaRuku);

        if (mirandaRukaObjekt != null)
        {
            mirandaRukaObjekt.SetActive(false);
            rukaAktivna = false;

            if (!posjedujeRuku)
            {
                Debug.Log("Miranda na startu NEMA ruku -> Ruka je zaključana i ugašena.");
            }
        }
    }

    void PokusajAktiviratiRuku()
    {
        if (inventar == null || !inventar.ImaRuku)
        {
            if (mirandaAudio != null) mirandaAudio.PlayArmError();

            if (mirandaRukaObjekt != null && mirandaRukaObjekt.activeSelf)
            {
                mirandaRukaObjekt.SetActive(false);
                rukaAktivna = false;
            }
            return;
        }

        bool hintOtvoren = (HintUI != null && HintUI.activeSelf);
        if (hintOtvoren) return;

        if (isInteracting) return;

        // 4. AKO IMA RUKU -> Pali / Gasi uz zvuk:
        if (mirandaRukaObjekt != null)
        {
            rukaAktivna = !rukaAktivna;
            mirandaRukaObjekt.SetActive(rukaAktivna);

            // DODAJ OVO (Zvuk aktiviranja ili skrivanja ruke):
            if (mirandaAudio != null)
            {
                if (rukaAktivna) mirandaAudio.PlayArmActivate();
                else mirandaAudio.PlayArmDeactivate();
            }

            Debug.Log($"Robotska ruka: {(rukaAktivna ? "UPALJENA" : "UGAŠENA")}");
        }
    }

    void Update()
    {
        float zInput = 0f;
        bool isSprinting = false;
        bool jumpPressed = false;

        // Čitamo kontrole samo ako je Miranda živa i pod kontrolom
        if (isControlled && currentState != MirandaState.Dead)
        {
            zInput = Input.GetAxisRaw("Horizontal");
            isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            jumpPressed = Input.GetKeyDown(KeyCode.W);

            // --- NOVO: LOGIKA ZA PALJENJE / GAŠENJE RUKE (Tipka F) ---
            if (Input.GetKeyDown(KeyCode.F))
            {
                PokusajAktiviratiRuku();
            }
        }

        // 1. Kretanje i fizika
        float currentMaxSpeed = isSprinting ? sprintSpeed : normalMoveSpeed;
        float currentRotMultiplier = isSprinting ? sprintRotationMultiplier : rotationMultiplier;

        float targetSpeed = zInput * currentMaxSpeed;

        if (zInput != 0)
        {
            currentZSpeed = Mathf.MoveTowards(currentZSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentZSpeed = Mathf.MoveTowards(currentZSpeed, 0f, deceleration * Time.deltaTime);
        }

        // --- NOVI SUSTAV ZA SKOK I OPĆI UDARAC O POD ---
        if (controller.isGrounded)
        {
            // 1. UDARAC O POD: Ako je padala i upravo dotaknula tlo (bilo sa skoka ili s ruba)
            if (isFalling)
            {
                isFalling = false;
                if (mirandaAudio != null) mirandaAudio.PlayLand();
            }

            // 2. SKOK
            if (jumpPressed)
            {
                verticalVelocity = jumpForce;
                isFalling = false; // 100% sigurnost da se zvuk slijetanja ne može okinuti pri skoku!

                if (mirandaAudio != null) mirandaAudio.PlayJump();
            }
            else
            {
                verticalVelocity = -2f; // Drži je priljubljenom uz tlo
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;

            // Čim krene padati prema dolje određenom brzinom, aktivira se priprema za slijetanje
            if (verticalVelocity < -2.5f)
            {
                isFalling = true;
            }
        }

        Vector3 move = new Vector3(0, verticalVelocity, currentZSpeed);
        CollisionFlags flags = controller.Move(move * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0)
        {
            verticalVelocity = 0f;
        }

        // --- 2. ROTACIJA TIJELA (WHOOSH NA 90 STUPNJEVA) ---
        if (mirandaTijelo != null)
        {
            float rotDelta = currentZSpeed * currentRotMultiplier * Time.deltaTime;
            currentRotation -= rotDelta;
            mirandaTijelo.localRotation = Quaternion.Euler(0, -90, currentRotation);

            // LOGIKA ZA WHOOSH:
            if (Mathf.Abs(currentZSpeed) > 0.2f)
            {
                accumulatedRotation += Mathf.Abs(rotDelta);

                if (accumulatedRotation >= kutZvuka)
                {
                    accumulatedRotation = 0f; // Resetiraj brojač
                    if (mirandaAudio != null) mirandaAudio.PlayWheelWhoosh(); // Pusti Whoosh
                }
            }
            else
            {
                // Čim stane u mjestu, resetiraj brojač da idući pokret krene ispočetka!
                accumulatedRotation = 0f;
            }
        }
    }

    public void SetLock(bool locked)
    {
        isControlled = !locked;
        if (controller != null && controller.enabled && gameObject.activeInHierarchy)
        {
            controller.Move(Vector3.zero);
        }
    }

    public void Die()
    {
        if (currentState == MirandaState.Dead) return;

        currentState = MirandaState.Dead;
        isControlled = false;

        // Ugasi ruku ako Miranda umre
        if (mirandaRukaObjekt != null)
        {
            mirandaRukaObjekt.SetActive(false);
            rukaAktivna = false;
        }

        if (controller != null)
        {
            controller.Move(Vector3.zero);
        }

        if (!deathScreenTriggered && deathScreenManager != null)
        {
            deathScreenTriggered = true;
            deathScreenManager.ShowDeathScreen();
        }

        Debug.Log("Miranda je eliminirana!");
    }
}