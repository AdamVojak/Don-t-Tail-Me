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

    [Header("Zajedničko Guranje (Entity Lock)")]
    public float pushSpeed = 2.5f;          // Brzina guranja/povlačenja
    public float pushRotationSpeed = 120f;  // Brzina rotiranja entiteta (stupnjevi u sekundi)
    public float attachDistance = 1.15f;    // Točna distanca između Sashe i centra kutije pri guranju
    private Vector3 relativeOffset = Vector3.zero; // Relativni vektor od kutije do Sashe

    [Header("Stanje Igrača")]
    public SashaState currentState = SashaState.Active;
    public GameObject mrtavSpritePrefab;

    private Vector3 prePushPosition;       // Pamti poziciju igrača prije početka guranja
    private Quaternion prePushRotation;   // Pamti rotaciju igrača prije početka guranja

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
    public SpriteRenderer hintsSprite;

    public Sprite clickF;

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

        // 1. INPUT ZA INTERAKCIJU (Tipka F) - Uvijek dostupna
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Ako smo blizu kutije ILI već guramo kutiju, tipka F kontrolira guranje
            if (currentObstacleInRange != null || currentState == SashaState.Pushing)
            {
                TogglePushing();
            }
            // Inače, tipka F služi za ventilaciju
            else
            {
                PokusajInterakciju();
            }
        }

        // 2. INTERAKTIVNO STANJE (Ventilacija i Inventar)
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

        // 3. PRIKUPLJANJE INPUTA ZA KRETANJE
        activeMoveSpeed = moveSpeed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z + tijelo.transform.position.z));
        mouseWorldPosition.z = 0;

        // 4. GRANANJE KRETANJA: Guranje VS Normalno kretanje
        if (currentState == SashaState.Pushing && lockedBox != null)
        {
            // A/D (Lijevo/Desno) kontroliraju zajedničku rotaciju oko centra kutije
            if (horizontal != 0)
            {
                float rotAngle = -horizontal * pushRotationSpeed * Time.deltaTime;

                // Rotiramo relativni offset oko kutije
                relativeOffset = Quaternion.Euler(0, 0, rotAngle) * relativeOffset;

                // Rotiramo kutiju na njezinoj osi
                lockedBox.transform.Rotate(Vector3.forward, rotAngle);

                // Rotiramo Sashu tako da uvijek gleda prema kutiji
                tijelo.transform.rotation = Quaternion.LookRotation(Vector3.forward, -relativeOffset.normalized);
            }

            // W/S (Gore/Dolje) guraju ili vuku kutiju duž osi gledanja
            Vector3 pushDir = -relativeOffset.normalized; // Smjer prema kutiji

            if (vertical != 0)
            {
                moveDirection = pushDir * vertical;

                // Pokrećemo Sashu s CharacterControllerom
                controller.Move(moveDirection * pushSpeed * Time.deltaTime);

                // Dajemo silu kutiji preko Rigidbodyja da prate jedno drugo
                lockedBox.linearVelocity = pushDir * vertical * pushSpeed;
            }
            else
            {
                moveDirection = Vector3.zero;
                lockedBox.linearVelocity = Vector3.zero;
            }

            // Drži Sashu fiksiranom na idealnoj distanci od kutije kako ne bi došlo do razdvajanja
            controller.enabled = false;
            transform.position = lockedBox.transform.position + relativeOffset.normalized * attachDistance;
            controller.enabled = true;
        }
        else
        {
            // --- NORMALNO KRETANJE (Kada ne guramo) ---
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

            // Pomakni kontroler normalnom brzinom
            controller.Move(moveDirection * activeMoveSpeed * Time.deltaTime);

            // Normalna rotacija tijela prema mišu
            Vector3 lookDirection = mouseWorldPosition - tijelo.transform.position;
            lookDirection.z = 0;

            if (lookDirection != Vector3.zero)
            {
                tijelo.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDirection);
            }
        }

        // 5. ANIMACIJA NOGU (Radi za oba stanja)
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

        if (other.CompareTag("Obstacle") && currentState == SashaState.Active)
        {
            currentObstacleInRange = other.gameObject;
            hints.SetActive(true);
            hintsSprite.sprite = clickF;
        }

        if (other.CompareTag("Obstacle") && currentState != SashaState.Active)
        {
            hints.SetActive(false);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle") && currentState == SashaState.Active)
        {
            hints.SetActive(true);
            hintsSprite.sprite = clickF;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            currentObstacleInRange = null;

            // Pozivamo metodu koja vraća Sashu na njezinu početnu poziciju
            StopPushing();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
      
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
            GameObject mrtviSasha = Instantiate(mrtavSpritePrefab, transform.position, tijelo.transform.rotation);

            Sasha_Death deathScript = mrtviSasha.GetComponent<Sasha_Death>();
            if (deathScript != null)
            {
                deathScript.uzrokSmrti = cause;
            }
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

    public void PrisilnoPrekiniInterakciju()
    {
        if (currentState == SashaState.Interactive)
        {
            currentState = SashaState.Active;

            if (animacijaTijela != null)
            {
                animacijaTijela.SetBool("odabir", false);
                animacijaTijela.SetBool("dodavanje", false);
                animacijaTijela.Play("Idle");
            }

            if (sashaInventory != null)
            {
                sashaInventory.CloseUI();
            }
        }
    }

    private void TogglePushing()
    {
        if (currentState == SashaState.Active)
        {
            if (currentObstacleInRange != null)
            {
                Rigidbody rb = currentObstacleInRange.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    // --- NOVO: Spremi točnu poziciju i rotaciju Sashe prije početka guranja ---
                    prePushPosition = transform.position;
                    if (tijelo != null)
                    {
                        prePushRotation = tijelo.transform.rotation;
                    }

                    lockedBox = rb;
                    currentState = SashaState.Pushing;

                    // 1. Izračunaj smjer guranja (snapanje na najbliži kardinalni smjer)
                    Vector3 dirToBox = (currentObstacleInRange.transform.position - transform.position).normalized;
                    dirToBox.z = 0;

                    if (Mathf.Abs(dirToBox.x) > Mathf.Abs(dirToBox.y))
                    {
                        dirToBox = new Vector3(Mathf.Sign(dirToBox.x), 0, 0);
                    }
                    else
                    {
                        dirToBox = new Vector3(0, Mathf.Sign(dirToBox.y), 0);
                    }

                    // 2. Isključi kontroler nakratko da se izbjegne fizikalni glitch pri teleportu
                    if (controller != null)
                    {
                        controller.enabled = false;
                    }

                    transform.position = lockedBox.transform.position - dirToBox * attachDistance;

                    if (controller != null)
                    {
                        controller.enabled = true;
                    }

                    // 3. Postavi rotaciju i zaključaj relativni offset
                    if (tijelo != null)
                    {
                        tijelo.transform.rotation = Quaternion.LookRotation(Vector3.forward, dirToBox);
                    }

                    relativeOffset = transform.position - lockedBox.transform.position;

                    // --- SIGURNO ISKLJUČIVANJE MEĐUSOBNIH SUDARA (Rješava NullReferenceException) ---
                    Collider boxCol = lockedBox.GetComponent<Collider>();
                    Collider[] sashaColliders = GetComponentsInChildren<Collider>();

                    if (boxCol != null && sashaColliders != null)
                    {
                        foreach (Collider sCol in sashaColliders)
                        {
                            if (sCol != null)
                            {
                                Physics.IgnoreCollision(sCol, boxCol, true);
                            }
                        }
                    }

                    if (animacijaTijela != null) animacijaTijela.SetBool("guranje", true);
                    Debug.Log("Sasha: Zaključan entitet guranja.");
                }
            }
        }
        else if (currentState == SashaState.Pushing)
        {
            StopPushing();
        }
    }

    public void StopPushing()
    {
        if (currentState == SashaState.Pushing)
        {
            currentState = SashaState.Active;
            if (animacijaTijela != null) animacijaTijela.SetBool("guranje", false);

            // --- NOVO: Vrati Sashu na njezinu originalnu poziciju i rotaciju ---
            if (controller != null)
            {
                controller.enabled = false; // Isključujemo CharacterController radi sigurnog teleporta
            }

            transform.position = prePushPosition;

            if (tijelo != null)
            {
                tijelo.transform.rotation = prePushRotation;
            }

            if (controller != null)
            {
                controller.enabled = true; // Ponovno uključujemo kontroler
            }

            if (lockedBox != null)
            {
                // Ponovno uključi međusobne sudare
                Collider boxCol = lockedBox.GetComponent<Collider>();
                Collider[] sashaColliders = GetComponentsInChildren<Collider>();

                if (boxCol != null && sashaColliders != null)
                {
                    foreach (Collider sCol in sashaColliders)
                    {
                        if (sCol != null)
                        {
                            Physics.IgnoreCollision(sCol, boxCol, false);
                        }
                    }
                }

                lockedBox.linearVelocity = Vector3.zero;
                lockedBox = null;
            }
            relativeOffset = Vector3.zero;
            lockedPushDir = Vector3.zero;
            Debug.Log("Sasha: Otključan entitet guranja i uspješno vraćen na početnu poziciju.");
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