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

    public float kutZvuka = 110f;
    private CharacterController controller;
    public bool isControlled = false;

    public float currentZSpeed = 0f;

    [Header("Vizuali (Djeca)")]
    public Transform mirandaTijelo;
    public Transform mirandaGlava;
    public GameObject mirandaRukaObjekt; // NOVO: Povuci objekt 'Miranda_Ruka' ovdje

    [Header("Ruka i Interakcije")]
    [Tooltip("Uključi ovo iz drugih skripti (npr. Ventilacija) kada Miranda stoji u njihovom triggeru")]
    public bool isInteracting = false;
    private bool rukaAktivna = false;
    private MirandaInventory inventar;

    [Header("UI Reference")]
    public DeathScreenMiranda deathScreenManager;
    private bool deathScreenTriggered = false;
    public GameObject HintUI;

    public float rotationMultiplier = 60f;
    private float currentRotation = 0f;

    [Header("Fizika")]
    public float gravity = -15f;
    private float verticalVelocity;

    [Header("Audio")]
    [SerializeField] private MirandaAudio mirandaAudio; // DODAJ OVO

    private float accumulatedRotation = 0f; // Prati rotaciju od 90 stupnjeva
    private bool wasGroundedLastFrame = true; // Prati slijetanje

    void Start()
    {
        if (mirandaAudio == null) mirandaAudio = GetComponent<MirandaAudio>();
        controller = GetComponent<CharacterController>();
        inventar = GetComponent<MirandaInventory>();

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

        // --- FIZIKA I SKOK (100% ČISTI REDOSLIJED) ---
        if (controller.isGrounded)
        {
            // ZVUK SLIJETANJA: Čuje se samo kada dotakne pod iz zraka
            if (!wasGroundedLastFrame && verticalVelocity < -3f)
            {
                if (mirandaAudio != null) mirandaAudio.PlayLand();
            }

            if (jumpPressed)
            {
                verticalVelocity = jumpForce; // Instantna primjena sile skoka!

                // ZVUK SKOKA:
                if (mirandaAudio != null) mirandaAudio.PlayJump();
            }
            else
            {
                verticalVelocity = -2f; // Drži je stabilno priljubljenom uz pod
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Pokretanje kontrolera
        Vector3 move = new Vector3(0, verticalVelocity, currentZSpeed);
        CollisionFlags flags = controller.Move(move * Time.deltaTime);

        // KLJUČNO: Bilježimo je li na podu TEK NAKON što se Move izvršio!
        wasGroundedLastFrame = controller.isGrounded;

        // Resetiranje vertikalne brzine ako udari glavom u strop
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