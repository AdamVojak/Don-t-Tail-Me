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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        inventar = GetComponent<MirandaInventory>();

        if (HintUI != null) HintUI.SetActive(false);

        // Na početku igre ruka je skrivena (ugašena)
        if (mirandaRukaObjekt != null)
        {
            mirandaRukaObjekt.SetActive(false);
            rukaAktivna = false;
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

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            if (jumpPressed)
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = new Vector3(0, verticalVelocity, currentZSpeed);
        CollisionFlags flags = controller.Move(move * Time.deltaTime);

        // 2. Rotacija tijela i glave
        if (mirandaTijelo != null)
        {
            currentRotation -= currentZSpeed * currentRotMultiplier * Time.deltaTime;
            mirandaTijelo.localRotation = Quaternion.Euler(0, -90, currentRotation);
        }

        if (mirandaGlava != null)
        {
            mirandaGlava.localRotation = Quaternion.Euler(0, -90, 0);
        }

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0)
        {
            verticalVelocity = 0f;
        }
    }

    void PokusajAktiviratiRuku()
    {
        // 1. Provjeri ima li uopće ruku u inventaru
        if (inventar == null || !inventar.ImaRuku)
        {
            Debug.Log("Miranda nema robotsku ruku u inventaru!");
            return;
        }

        // 2. Provjeri je li otvoren HintUI (papir na zidu)
        bool hintOtvoren = (HintUI != null && HintUI.activeSelf);
        if (hintOtvoren)
        {
            // Igrač gleda papir, nemoj dirati ruku
            return;
        }

        // 3. Provjeri je li Miranda u triggeru ventilacije ili nekog drugog objekta
        if (isInteracting)
        {
            // Druga interakcija ima prioritet
            return;
        }

        // AKO JE SVE ČISTO -> Pali / gasi ruku!
        if (mirandaRukaObjekt != null)
        {
            rukaAktivna = !rukaAktivna;
            mirandaRukaObjekt.SetActive(rukaAktivna);
            Debug.Log($"Robotska ruka: {(rukaAktivna ? "UPALJENA" : "UGAŠENA")}");
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