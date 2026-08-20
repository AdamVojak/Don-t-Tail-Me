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

    [Header("UI Reference")]
    public DeathScreenMiranda deathScreenManager;
    private bool deathScreenTriggered = false;

    public float rotationMultiplier = 60f;
    private float currentRotation = 0f;

    //[Header("Inventar")]
    public enum Inventar { None, KljucZ, KljucLj };
    public Inventar skupljeno;


    [Header("Fizika")]
    public float gravity = -15f;
    private float verticalVelocity;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        //if (!isControlled || currentState == MirandaState.Dead) return;

        float zInput = 0f;
        bool isSprinting = false;
        bool jumpPressed = false;

        // 3. Čitamo prave tipke SAMO ako igrač ima kontrolu i Miranda je živa
        if (isControlled && currentState != MirandaState.Dead)
        {
            zInput = Input.GetAxisRaw("Horizontal");
            isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            jumpPressed = Input.GetKeyDown(KeyCode.W);
        }

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

        if (mirandaTijelo != null)
        {
            currentRotation += currentZSpeed * currentRotMultiplier * Time.deltaTime;
            mirandaTijelo.localRotation = Quaternion.Euler(0, 90, currentRotation);
        }

        if (mirandaGlava != null)
        {
            mirandaGlava.localRotation = Quaternion.Euler(0, 90, 0);
        }

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0)
        {
            verticalVelocity = 0f;
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