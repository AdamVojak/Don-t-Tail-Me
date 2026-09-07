using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SashaController : MonoBehaviour
{
    public enum SashaState { Active, Interactive, Pushing, Dead, Shocked }

    [Header("Postavke Ranjenosti")]
    public float stetaCooldown = 1.0f;
    private float zadnjeVrijemeStete = -1f;

    [Header("UI Srce Audio")]
    [SerializeField] private KucanjeSrca uiHeartAudio;

    [Header("Postavke Roja Crva")]
    public float wormBiteDelay = 1.0f;       // Koliko dugo crvi moraju biti na igraču za ugriz (1 sekunda)
    public int minWormsForDamage = 2;        // Minimalan broj crva potreban za nanošenje štete (2 crva)
    private List<Collider> touchingWorms = new List<Collider>(); // Lista svih crva koji trenutno dodiruju Sashu
    private Coroutine wormBiteCoroutine = null; // Referenca na timer ugriza

    [Header("Zajedničko Guranje (Entity Lock)")]
    private bool isActivelyMovingBox = false; // Prati kreće li se kutija fizički
    public float pushSpeed = 2.5f;          // Brzina guranja/povlačenja
    public float pushRotationSpeed = 120f;  // Brzina rotiranja entiteta (stupnjevi u sekundi)
    public float attachDistance = 1.15f;    // Točna distanca između Sashe i centra kutije pri guranju
    private Vector3 relativeOffset = Vector3.zero; // Relativni vektor od kutije do Sashe

    [Header("Stanje Igrača")]
    public SashaState currentState = SashaState.Active;
    public GameObject mrtavSpritePrefab;

    [SerializeField] private float multiplierSpeed = 2f;

    private float originalZ; // Pamti samo Z-koordinatu igrača prije početka guranja

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

    [Header("Hint Sustav")]
    public SpriteRenderer hintRenderer; // Ovdje u Inspectoru povuci onaj SpriteRenderer iznad glave
    private object trenutniVlasnikHinta = null;

    public Sprite clickF;

    public GameObject svijetlo;

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

    [Header("Audio")]
    [SerializeField] private SashaAudio sashaAudio;

    private Vector3 lockedPushDir = Vector3.zero;
    private Rigidbody lockedBox = null;
    private GameObject currentObstacleInRange = null;

    void Start()
    {
        if (sashaAudio == null) sashaAudio = GetComponent<SashaAudio>();
        if (uiHeartAudio == null) uiHeartAudio = FindFirstObjectByType<KucanjeSrca>();
        if (uiHeartAudio != null) uiHeartAudio.UpdateHeartRateByHealth(zivot, 3);
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

        if (hintRenderer != null) hintRenderer.enabled = false;

        svijetlo.SetActive(true);
    }

    void Update()
    {
        if (!isControlled || currentState == SashaState.Dead || currentState == SashaState.Shocked)
        {
            animacijaNogu.StopPlayback();
            if (noge != null) noge.SetActive(false);

            if (sashaAudio != null) sashaAudio.StopAllLoops();

            return;
        }

        if (currentObstacleInRange != null)
        {
            if (!currentObstacleInRange.activeInHierarchy || !currentObstacleInRange.CompareTag("Obstacle"))
            {
                currentObstacleInRange = null;
                SakrijHint(this);
            }
        }

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
                        sashaAudio.ExitInteractiveState();
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

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z + tijelo.transform.position.z));
        mouseWorldPosition.z = 0;

        // 4. GRANANJE KRETANJA: Guranje VS Normalno kretanje
        if (currentState == SashaState.Pushing && lockedBox != null)
        {
            // --- NOVO: AUDIO LOGIKA ZA FIZIČKO GURANJE ---
            // Kutija se kreće ako igrač stišće W/S (vertical) ili A/D (horizontal za rotaciju)
            bool isMovingNow = (vertical != 0 || horizontal != 0);

            if (isMovingNow && !isActivelyMovingBox)
            {
                // Tek je počeo gurati
                isActivelyMovingBox = true;
                if (sashaAudio != null) sashaAudio.StartPushing();
            }
            else if (!isMovingNow && isActivelyMovingBox)
            {
                // Stao je u mjestu
                isActivelyMovingBox = false;
                if (sashaAudio != null) sashaAudio.StopPushing();
            }

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
            // --- SIGURNOSNA PROVJERA (Ako smo u stanju guranja, ali je kutija uništena/nestala) ---
            if (currentState == SashaState.Pushing)
            {
                StopPushing(); // Automatski prekini guranje i teleportiraj Sashu natrag
            }

            // --- NORMALNO KRETANJE (Kada ne guramo) ---
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                activeMoveSpeed *= multiplierSpeed;
                if (animacijaNogu != null) animacijaNogu.speed = multiplierSpeed;
            }
            else
            {
                if (animacijaNogu != null) animacijaNogu.speed = 1f;
            }

            // Pomakni kontroler normalnom brzinom
            controller.Move(moveDirection * activeMoveSpeed * Time.deltaTime);

            // --- NOVO: ROTACIJA NOGU PREMA SMJERU KRETANJA (W,A,S,D) ---
            if (moveDirection != Vector3.zero && noge != null)
            {
                // Noge se rotiraju točno u onom smjeru u kojem igrač hoda (uključujući dijagonale)
                noge.transform.rotation = Quaternion.LookRotation(Vector3.forward, moveDirection);
            }

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
            if (!touchingWorms.Contains(other))
            {
                touchingWorms.Add(other);
            }

            if (touchingWorms.Count >= minWormsForDamage && wormBiteCoroutine == null)
            {
                wormBiteCoroutine = StartCoroutine(WormBiteRoutine());
            }
        }

        if (other.CompareTag("Fist"))
        {
            if (sashaAudio != null) sashaAudio.PlayFistHit();
            TakeDamage(1, 1);
        }

        if (other.CompareTag("Obstacle") && currentState == SashaState.Active)
        {
            currentObstacleInRange = other.gameObject;
            PrikaziHint(this, clickF);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle") && currentState == SashaState.Active)
        {
            PrikaziHint(this, clickF);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Worm"))
        {
            if (touchingWorms.Contains(other))
            {
                touchingWorms.Remove(other);
            }

            if (touchingWorms.Count < minWormsForDamage && wormBiteCoroutine != null)
            {
                StopCoroutine(wormBiteCoroutine);
                wormBiteCoroutine = null;
                Debug.Log("Sasha: Roj se smanjio. Timer ugriza zaustavljen.");
            }
        }

        if (other.CompareTag("Obstacle"))
        {
            currentObstacleInRange = null;
            SakrijHint(this);
            StopPushing();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
      
    }

    public void TakeDamage(int damageAmount, int cause, bool ignoreCooldown = false)
    {
        if (currentState == SashaState.Dead) return;

        // Ako NE ignoriramo cooldown, provjeri vrijeme
        if (!ignoreCooldown && Time.time < zadnjeVrijemeStete + stetaCooldown)
        {
            return;
        }

        // Resetiramo timer kako idući obični udarac ne bi bio trenutan
        zadnjeVrijemeStete = Time.time;

        zivot -= damageAmount;

        if (sashaAudio != null) sashaAudio.PlayHurtDelayed();

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
        if (sashaAudio != null) sashaAudio.StopAllLoops();
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
        if (sashaAudio != null) sashaAudio.PlayHeal();
        int dodatak = paket;
        zivot += dodatak;
        if (uiHeartAudio != null) uiHeartAudio.UpdateHeartRateByHealth(zivot, 3);
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

            // 1. TELEPORTACIJA: Zadržavamo Sashinu originalnu Z poziciju da ne propadne u pozadinu
            Vector3 ciljnaPozicija = trenutnaVentilacija.interactionAreaCenter.position;
            ciljnaPozicija.z = transform.position.z;
            transform.position = ciljnaPozicija;

            controller.enabled = true;

            // 2. DINAMIČKA ROTACIJA: Sasha pita ventilaciju pod kojim kutem treba stati
            tijelo.transform.rotation = Quaternion.Euler(0, 0, trenutnaVentilacija.kutGledanjaSashe);

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
            }

            if (sashaInventory != null) sashaInventory.CloseUI();
        }
    }


    public void PrisilnoPrekiniInterakciju()
    {
        if (currentState == SashaState.Interactive)
        {
            if (sashaAudio != null) sashaAudio.ExitInteractiveState();
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
                    // --- NOVO: Spremi originalnu Z-koordinatu prije nego što započne guranje ---
                    originalZ = transform.position.z;

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

            if (isActivelyMovingBox)
            {
                isActivelyMovingBox = false;
                if (sashaAudio != null) sashaAudio.StopPushing();
            }

            if (animacijaTijela != null) animacijaTijela.SetBool("guranje", false);

            // --- NOVO: Vrati Sashu na njezinu originalnu Z-koordinatu kako nikada ne bi potonula ---
            if (controller != null)
            {
                controller.enabled = false; // Isključujemo CharacterController radi sigurnog snap-anja Z osi
            }

            Vector3 currentPos = transform.position;
            currentPos.z = originalZ; // Vraćamo samo Z os na njezinu originalnu, sigurnu vrijednost!
            transform.position = currentPos;

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
            Debug.Log("Sasha: Otključan entitet guranja (Z-os osigurana).");
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

    private IEnumerator WormBiteRoutine()
    {
        Debug.Log("Sasha: Barem 2 crva su na igraču! Započinjem odbrojavanje za ugriz...");

        while (touchingWorms.Count >= minWormsForDamage)
        {
            // Čistimo listu od uništenih (ubijenih) crva u slučaju da ih je igrač ubio dok su na njemu
            touchingWorms.RemoveAll(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);

            // Provjera ako je nakon čišćenja broj crva pao ispod granice
            if (touchingWorms.Count < minWormsForDamage)
            {
                break;
            }

            // Čekamo 1 sekundu (vrijeme potrebno za ugriz)
            yield return new WaitForSeconds(wormBiteDelay);

            // Još jedna provjera nakon čekanja (u slučaju da je igrač ubio crva u zadnjoj milisekundi)
            touchingWorms.RemoveAll(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);

            if (touchingWorms.Count >= minWormsForDamage)
            {
                if (sashaAudio != null) sashaAudio.PlayWormBite();
                TakeDamage(1, 0); // Nanosi točno 1 štetu (uzrok 0, npr. crv)

                // Cooldown između dva ugriza roja (da ne nanose štetu svaki frame)
                yield return new WaitForSeconds(1.5f);
            }
        }

        wormBiteCoroutine = null;
        Debug.Log("Sasha: Timer ugriza završen.");
    }

    public void PrikaziHint(object trazitelj, Sprite slicica)
    {
        if (!isControlled || currentState == SashaState.Dead) return;

        trenutniVlasnikHinta = trazitelj;
        if (hintRenderer != null)
        {
            hintRenderer.sprite = slicica;
            hintRenderer.enabled = true;
        }
    }
    public void SakrijHint(object trazitelj)
    {
        if (trenutniVlasnikHinta == null || trenutniVlasnikHinta == trazitelj || trenutniVlasnikHinta.Equals(null))
        {
            trenutniVlasnikHinta = null;
            if (hintRenderer != null)
            {
                hintRenderer.enabled = false;
            }
        }
    }
    public void PrisilnoUgasiSveHintove()
    {
        trenutniVlasnikHinta = null;
        if (hintRenderer != null)
        {
            hintRenderer.enabled = false;
        }
    }

    // =========================================================
    // EFEKT STRUJNOG UDARA (Poziva ga LaserBarijera)
    // =========================================================
    public void PrimijeniStrujniUdar(float snagaOdbacivanja, float trajanje, Sprite sokiraniSprite)
    {
        if (currentState == SashaState.Dead || currentState == SashaState.Shocked) return;
        StartCoroutine(StrujniUdarRoutine(snagaOdbacivanja, trajanje, sokiraniSprite));
    }

    private System.Collections.IEnumerator StrujniUdarRoutine(float snagaOdbacivanja, float trajanje, Sprite sokiraniSprite)
    {
        currentState = SashaState.Shocked;
        isControlled = false;

        if (noge != null) noge.SetActive(false);
        animacijaNogu.StopPlayback();
        if (animacijaTijela != null) animacijaTijela.enabled = false;

        // =========================================================================
        // 1. PRIVREMENO GASIMO SUSTAV ORUŽJA (Da ne može pucati u šoku)
        // =========================================================
        bool oruzjeBiloAktivno = false;
        if (SustavOruzja != null)
        {
            oruzjeBiloAktivno = SustavOruzja.enabled;
            SustavOruzja.enabled = false; // Isključujemo skriptu za oružje
        }

        // =========================================================================
        // 2. AUTOMATSKI PRONALAZIMO I SKRIVAMO ORUŽJE U RUCI (Gun, Minigun, Melee)
        // =========================================================================
        List<SpriteRenderer> sakrivenaOruzja = new List<SpriteRenderer>();
        SpriteRenderer[] sviSpriteovi = GetComponentsInChildren<SpriteRenderer>(false);

        foreach (SpriteRenderer sr in sviSpriteovi)
        {
            // Sakrivamo sve spriteove koji NISU tijelo, glava ili hint
            if (sr != tijeloSprite && sr != glavaSprite && sr != hintRenderer)
            {
                // =========================================================
                // NOVO: IGNORIRAMO BILO ŠTO ŠTO IMA VEZE S GUIDING ARROW!
                // =========================================================
                if (sr.name.ToLower().Contains("arrow") || sr.GetComponent("GuidingArrow") != null)
                {
                    continue; // Preskoči strelicu, nju ne diraj!
                }

                sakrivenaOruzja.Add(sr);
                sr.enabled = false;
            }
        }

        // 3. Postavljamo šokirani sprite (kostur) i gasimo glavu
        Sprite origTijelo = tijeloSprite.sprite;
        if (sokiraniSprite != null)
        {
            tijeloSprite.sprite = sokiraniSprite;
            if (glava != null) glava.SetActive(false);
        }

        // 4. Glatko odbacivanje unazad po Y osi (Knockback)
        float knockbackDuration = 0.2f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(0, -snagaOdbacivanja, 0);

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;
            Vector3 frameMove = Vector3.Lerp(startPos, targetPos, t) - transform.position;
            if (controller != null) controller.Move(frameMove);
            yield return null;
        }

        // 5. Brzo treperenje boja (Plava <-> Crna)
        int brojTreptaja = 8;
        float vrijemeTreptaja = (trajanje - knockbackDuration) / (brojTreptaja * 2);

        for (int i = 0; i < brojTreptaja; i++)
        {
            tijeloSprite.material.color = Color.blue;
            if (glavaSprite != null) glavaSprite.material.color = Color.blue;
            yield return new WaitForSeconds(vrijemeTreptaja);

            tijeloSprite.material.color = Color.black;
            if (glavaSprite != null) glavaSprite.material.color = Color.black;
            yield return new WaitForSeconds(vrijemeTreptaja);
        }

        // 6. Vraćanje tijela, glave i animacija u normalu
        tijeloSprite.material.color = Color.white;
        if (glavaSprite != null) glavaSprite.material.color = Color.white;

        tijeloSprite.sprite = origTijelo;
        if (glava != null) glava.SetActive(true);
        if (animacijaTijela != null) animacijaTijela.enabled = true;

        // =========================================================================
        // 7. VRAĆAMO ORUŽJE U RUKU I PALIMO SUSTAV ORUŽJA NATRAG
        // =========================================================================
        foreach (SpriteRenderer sr in sakrivenaOruzja)
        {
            if (sr != null) sr.enabled = true; // Vrati oružje vidljivim!
        }

        if (SustavOruzja != null)
        {
            SustavOruzja.enabled = oruzjeBiloAktivno; // Opet može pucati!
        }

        currentState = SashaState.Active;
        isControlled = true;
    }

    void OnDisable()
    {
        StopAllCoroutines();

        if (sashaAudio != null) sashaAudio.StopAllLoops();
    }
}