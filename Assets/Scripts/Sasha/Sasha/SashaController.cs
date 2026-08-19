using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class SashaController : MonoBehaviour
{
    public enum SashaState { Active, Interactive, Pushing, Dead }

    [Header("Postavke Ranjenosti")]
    public float stetaCooldown = 1.0f;
    private float zadnjeVrijemeStete = -1f;

    [Header("Stanje Igrača")]
    public SashaState currentState = SashaState.Active;
    public GameObject mrtavSpritePrefab;

    public float moveSpeed = 5f;
    private float activeMoveSpeed;
    private CharacterController controller;
    public int zivot;

    [Header("Kontrola stanja")]
    public bool dozvoljenoKretanje = true;

    [Header("Interakcija/Inventar")]
    public Ventilacija_In_Sasha trenutnaVentilacija;
    public Animator animacijaTijela;
    public SashaInventory sashaInventory;
    public SustavOruzja SustavOruzja;
    public GameObject hints;

    public GameObject svijetlo;

    public GameObject svijetloMaze;

    [Header("TIJELO")]
    public GameObject tijelo;
    public GameObject glava;
    public SpriteRenderer tijeloSprite;
    public SpriteRenderer glavaSprite;

    [Header("NOGE")]
    public Animator animacijaNogu;
    public GameObject noge;

    [Header("GAME MANAGER")]
    public bool isControlled = false;

    [Header("UI Reference")]
    public DeathScreenSasha deathScreenManager;
    private bool deathScreenTriggered = false;

    private int zvukUdarca;
    private Vector3 lockedPushDir = Vector3.zero;
    private Rigidbody lockedBox = null;
    private GameObject currentObstacleInRange = null;

    void Start()
    {
        tijeloSprite.material.color = Color.white;
        glavaSprite.material.color = Color.white;
        zivot = 3;
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController nije pronađen na objektu Sasha! Dodaj CharacterController komponentu.");
            enabled = false;
        }

        if (tijelo == null)
        {
            Debug.LogError("GameObject 'tijelo' nije dodijeljen! Dodijeli glavni dio tijela koji se rotira.");
            enabled = false;
        }
        hints.SetActive(false);
        svijetlo.SetActive(true);
        svijetloMaze.SetActive(false);
    }

    void Update()
    {
        if (!isControlled)
        {
            animacijaNogu.StopPlayback();
            noge.SetActive(false);
            return;
        }

        if (currentState == SashaState.Dead) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            PokusajInterakciju();
        }

        if (currentState == SashaState.Interactive)
        {
            if (noge != null) noge.SetActive(false);
            animacijaNogu.StopPlayback();
            if (sashaInventory != null) sashaInventory.HandleNavigation();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (sashaInventory != null && sashaInventory.inventoryUIPanel.activeSelf)
                {
                    if (sashaInventory.TrySendSelectedItem())
                    {
                        sashaInventory.CloseUI();

                        if (animacijaTijela != null)
                        {
                            animacijaTijela.SetBool("dodavanje", true);
                            animacijaTijela.SetBool("odabir", false);
                        }
                    }
                }
            }

            return;
        }

        activeMoveSpeed = moveSpeed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z + tijelo.transform.position.z));
        mouseWorldPosition.z = 0;


        if (currentObstacleInRange != null)
        {
            Vector3 playerPos = transform.position;
            playerPos.z = 0;

            Vector3 boxPos = currentObstacleInRange.transform.position;
            boxPos.z = 0;

            Vector3 mousePos = mouseWorldPosition;
            mousePos.z = 0;

            Vector3 dirToMouse = (mousePos - playerPos).normalized;
            Vector3 dirToBox = (boxPos - playerPos).normalized;

            float mouseAimAngle = Vector3.Dot(dirToMouse, dirToBox);

            if (mouseAimAngle >= 0.7f)
            {
                if (currentState == SashaState.Active)
                {
                    currentState = SashaState.Pushing;
                    if (animacijaTijela != null) animacijaTijela.SetBool("guranje", true);
                }
            }
            else
            {
                if (currentState == SashaState.Pushing)
                {
                    currentState = SashaState.Active;
                    if (animacijaTijela != null) animacijaTijela.SetBool("guranje", false);

                    if (lockedBox != null)
                    {
                        lockedBox.linearVelocity = Vector3.zero;
                        lockedBox = null;
                    }
                    lockedPushDir = Vector3.zero;
                }
            }
        }

        if (currentState == SashaState.Pushing && lockedBox != null)
        {
            float dotInput = Vector3.Dot(moveDirection, lockedPushDir);

            if (dotInput > 0.1f)
            {
                moveDirection = lockedPushDir; // Zaključaj kretanje samo u smjeru guranja

                float finalPushSpeed = (moveSpeed * 0.8f) / lockedBox.mass;
                lockedBox.linearVelocity = lockedPushDir * finalPushSpeed;
            }
            else if (dotInput < -0.1f)
            {
                // Prisilni otključaj ako idemo unazad
                currentState = SashaState.Active;
                if (animacijaTijela != null) animacijaTijela.SetBool("guranje", false);

                lockedBox.linearVelocity = Vector3.zero;
                lockedBox = null;
                lockedPushDir = Vector3.zero;
            }
            else
            {
                moveDirection = Vector3.zero;
                lockedBox.linearVelocity = Vector3.zero;
            }
        }

        if (horizontal == 0 && vertical == 0 || isControlled == false)
        {
            if (noge != null) noge.SetActive(false);
            animacijaNogu.StopPlayback();
        }
        else
        {
            if (noge != null) noge.SetActive(true);
            animacijaNogu.Play("Idle", 0);
        }

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            activeMoveSpeed *= 1.5f;
            if (animacijaNogu != null) animacijaNogu.speed = 2f;
        }
        else
        {
            if (animacijaNogu != null) animacijaNogu.speed = 1f;
        }

        controller.Move(moveDirection * activeMoveSpeed * Time.deltaTime);

        if (currentState != SashaState.Pushing && lockedBox != null)
        {
            Vector3 lookDirection = mouseWorldPosition - tijelo.transform.position;
            lookDirection.z = 0;

            tijelo.transform.rotation = Quaternion.LookRotation(Vector3.forward, lockedPushDir);
        }
        else if (currentState != SashaState.Pushing)
        {
            Vector3 lookDirection = mouseWorldPosition - tijelo.transform.position;
            lookDirection.z = 0;

            if (lookDirection != Vector3.zero)
            {
                tijelo.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDirection);
            }
        }
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
    }



    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Worm"))
        {
            int i = Random.Range(1, 3);
            if (i == 1)
            {
                TakeDamage(1, 0);
            }
        }

        if (other.CompareTag("Fist"))
        {
            zvukUdarca = Random.Range(1, 3);
            if (zvukUdarca == 1)
            {
                SFX.zvucniEfekti.ZvukUdarca1.Play();
            }
            else
            {
                SFX.zvucniEfekti.ZvukUdarca2.Play();
            }
            TakeDamage(1, 1);
        }

        if (other.CompareTag("Obstacle"))
        {
            currentObstacleInRange = other.gameObject;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            currentObstacleInRange = null;

            if (currentState == SashaState.Pushing)
            {
                currentState = SashaState.Active;
                if (animacijaTijela != null) animacijaTijela.SetBool("guranje", false);
            }

            if (lockedBox != null)
            {
                lockedBox.linearVelocity = Vector3.zero;
                lockedBox = null;
            }
            lockedPushDir = Vector3.zero;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle") && !hit.collider.isTrigger)
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb == null || rb.isKinematic) return;

            if (lockedBox == null && currentState == SashaState.Pushing)
            {
                Vector3 facingDir = tijelo.transform.up;
                Vector3 dirToBox = (hit.collider.transform.position - transform.position).normalized;
                dirToBox.z = 0;

                float lookAngle = Vector3.Dot(facingDir, dirToBox);

                if (lookAngle > 0.7f)
                {
                    lockedBox = rb;

                    Vector3 normal = hit.normal;
                    if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                    {
                        lockedPushDir = new Vector3(-Mathf.Sign(normal.x), 0, 0);
                    }
                    else
                    {
                        lockedPushDir = new Vector3(0, -Mathf.Sign(normal.y), 0);
                    }

                    tijelo.transform.rotation = Quaternion.LookRotation(Vector3.forward, lockedPushDir);
                }
            }
        }
    }

    // Dodali smo 'int cause' kako bi znali tko je zadao udarac
    public void TakeDamage(int damageAmount, int cause)
    {
        if (currentState == SashaState.Dead) return;

        if (Time.time < zadnjeVrijemeStete + stetaCooldown)
        {
            return;
        }

        zadnjeVrijemeStete = Time.time;

        zivot -= damageAmount;

        StartCoroutine(ChangeColorTemporary(Color.red, 0.2f));

        if (zivot <= 0)
        {
            zivot = 0;
            SmrtIgraca(cause);
        }
    }

    void ResetColor() { tijeloSprite.material.color = Color.white; glavaSprite.material.color = Color.white; }

    void SmrtIgraca(int cause)
    {
        currentState = SashaState.Dead;
        isControlled = false;
        SFX.zvucniEfekti.ZvukSmrti.Play();

        if (mrtavSpritePrefab != null)
        {
            Instantiate(mrtavSpritePrefab, transform.position, tijelo.transform.rotation);
        }

        if (!deathScreenTriggered && deathScreenManager != null)
        {
            deathScreenTriggered = true;
            deathScreenManager.ShowDeathScreen(cause);
        }

        Debug.Log("Sasha je uništen!");
        Destroy(gameObject);
    }

    public void Heal(int paket)
    {
        int dodatak = paket;
        zivot += dodatak;
        if (zivot > 3) zivot = 3;
        tijeloSprite.material.color = Color.green;
        glavaSprite.material.color = Color.green;
        StartCoroutine(ChangeColorTemporary(Color.green, 0.3f));

        Debug.Log("Igrač je izliječen, ima " + zivot + " života.");
    }

    private IEnumerator ChangeColorTemporary(Color targetColor, float duration)
    {
        tijeloSprite.material.color = targetColor;
        glavaSprite.material.color = targetColor;
        yield return new WaitForSeconds(duration);
        tijeloSprite.material.color = Color.white;
        glavaSprite.material.color = Color.white;
    }

    private void PokusajInterakciju()
    {
        if (currentState == SashaState.Active && trenutnaVentilacija != null && trenutnaVentilacija.JeOtvorena)
        {
            currentState = SashaState.Interactive;

            controller.enabled = false;

            transform.position = trenutnaVentilacija.interactionAreaCenter.position;

            controller.enabled = true;

            tijelo.transform.rotation = Quaternion.Euler(0, 0, -90f);

            if (noge != null) noge.SetActive(false);
            if (animacijaTijela != null) animacijaTijela.SetBool("odabir", true);

            if (sashaInventory != null) sashaInventory.OpenUI();

        }
        else if (currentState == SashaState.Interactive)
        {
            currentState = SashaState.Active;

            if (animacijaTijela != null)
            {
                animacijaTijela.SetBool("odabir", false);
                animacijaTijela.Play("Idle");
                if (sashaInventory != null) sashaInventory.CloseUI();
            }
        }
    }

    public void ZavrsiDodavanje()
    {
        if (animacijaTijela != null)
        {
            animacijaTijela.SetBool("dodavanje", false);
            animacijaTijela.Play("Idle");
        }

        if (sashaInventory != null && sashaInventory.itemURuciSpriteRenderer != null)
        {
            sashaInventory.itemURuciSpriteRenderer.gameObject.SetActive(false);
        }

        currentState = SashaState.Active;
    }
}