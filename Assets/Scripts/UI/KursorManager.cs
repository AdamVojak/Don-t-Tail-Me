using UnityEngine;
using UnityEngine.Rendering;

public class CursorManager : MonoBehaviour
{
    [Header("Kursor objekti (UI elementi)")]
    [SerializeField] private GameObject sashaCursor;
    [SerializeField] private GameObject mirandaCursor;
    [SerializeField] private GameObject giovanniCursor;
    [SerializeField] private GameObject deathCursor; // NOVO: Kursor za Death Screen

    public bool isDeathScreenActive = false;

    [Header("Giovanni Kursor Postavke")]
    [SerializeField] private Sprite giovanniNormalSprite;
    [SerializeField] private Sprite giovanniHandSprite;
    [SerializeField] private Sprite giovanniCrowbarSprite; // NOVO: Sprite pajsera
    [SerializeField] private Sprite giovanniNoCrowbarSprite; // NOVO: Sprite prekriženog pajsera

    private GiovanniInventory giovanniInventory;
    private UnityEngine.UI.Image giovanniCursorImage;
    public bool IsTargetingItem { get; private set; } = false;

    [Header("Giovanni Interakcija")]
    [SerializeField] private float maxRaycastDistance = 100f; // Domet kamere
    [Tooltip("Koliko Giovanni mora biti blizu predmetu da bi se kursor promijenio i omogućio interakciju")]
    [SerializeField] private float maxInteractionDistance = 4.5f; // NOVO: Fizički domet (u metrima)
    [SerializeField] private LayerMask interactableLayer;

    [Header("Giovanni Kursor Postavke (Ključ i Gumb)")]
    [SerializeField] private Sprite giovanniKeySprite;       // Kursor kada ima ključ
    [SerializeField] private Sprite giovanniNoKeySprite;     // Kursor prekriženog ključa
    [SerializeField] private Sprite giovanniPressSprite;     // Kursor za pritiskanje gumba

    [Header("Audio Postavke")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip errorSound;

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager;

    private Camera mainCamera;

    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager referenca nije postavljena u CursorManageru!");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;

        FindGiovanniInventory();

        if (giovanniCursor != null)
        {
            giovanniCursorImage = giovanniCursor.GetComponent<UnityEngine.UI.Image>();

            if (giovanniCursorImage == null)
            {
                giovanniCursorImage = giovanniCursor.GetComponentInChildren<UnityEngine.UI.Image>();
            }

            if (giovanniCursorImage == null)
            {
                Debug.LogError("UnityEngine.UI.Image komponenta nije pronađena na giovanniCursor objektu niti u njegovoj djeci!");
            }
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (gameManager == null) return;

        UpdateActiveCursor();
        FollowMousePosition();

        if (gameManager.currChar == GameManager.ActiveCharacter.Giovanni)
        {
            HandleGiovanniInteraction();
        }
    }

    private void UpdateActiveCursor()
    {
        if (isDeathScreenActive)
        {
            if (sashaCursor != null) sashaCursor.SetActive(false);
            if (mirandaCursor != null) mirandaCursor.SetActive(false);
            if (giovanniCursor != null) giovanniCursor.SetActive(false);
            if (deathCursor != null) deathCursor.SetActive(true);
            return;
        }

        if (deathCursor != null) deathCursor.SetActive(false);

        GameManager.ActiveCharacter activeChar = gameManager.currChar;

        if (sashaCursor != null) sashaCursor.SetActive(activeChar == GameManager.ActiveCharacter.Sasha);
        if (mirandaCursor != null) mirandaCursor.SetActive(activeChar == GameManager.ActiveCharacter.Miranda);
        if (giovanniCursor != null) giovanniCursor.SetActive(activeChar == GameManager.ActiveCharacter.Giovanni);
    }

    private void FollowMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;

        if (isDeathScreenActive && deathCursor != null && deathCursor.activeSelf) deathCursor.transform.position = mousePos;
        else if (sashaCursor != null && sashaCursor.activeSelf) sashaCursor.transform.position = mousePos;
        else if (mirandaCursor != null && mirandaCursor.activeSelf) mirandaCursor.transform.position = mousePos;
        else if (giovanniCursor != null && giovanniCursor.activeSelf) giovanniCursor.transform.position = mousePos;
    }

    private void HandleGiovanniInteraction()
    {
        if (isDeathScreenActive) return;

        if (gameManager.currChar != GameManager.ActiveCharacter.Giovanni)
        {
            ResetGiovanniCursor();
            return;
        }

        if (giovanniCursorImage == null) return;

        FindGiovanniInventory();
        if (giovanniInventory == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance))
        {
            float distanceToPlayer = Vector3.Distance(giovanniInventory.transform.position, hit.point);

            if (distanceToPlayer > maxInteractionDistance)
            {
                ResetGiovanniCursor();
                return;
            }

            CollectibleItem item = hit.collider.GetComponentInParent<CollectibleItem>();
            Computer computer = hit.collider.GetComponentInParent<Computer>();
            DestructibleObject destructible = hit.collider.GetComponentInParent<DestructibleObject>();

            // NOVO: Tražimo KeyPanelController
            KeyPanelController keyPanel = hit.collider.GetComponentInParent<KeyPanelController>();

            // LOGIKA ZA PANEL S KLJUČEM
            if (keyPanel != null)
            {
                IsTargetingItem = true;

                // 1. Slučaj: Gledamo u mjesto za ubacivanje ključa
                if (hit.collider == keyPanel.insertKeyCollider)
                {
                    if (giovanniInventory.imaObicanKljuc) // Pretpostavljam da se bool ovako zove
                    {
                        SetGiovanniCursorSprite(giovanniKeySprite);
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            giovanniInventory.imaObicanKljuc = false; // Oduzimamo ključ iz inventara
                            keyPanel.InsertKey(); // Mijenjamo objekte
                        }
                    }
                    else
                    {
                        SetGiovanniCursorSprite(giovanniNoKeySprite);
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            if (audioSource != null && errorSound != null)
                                audioSource.PlayOneShot(errorSound);
                        }
                    }
                }
                // 2. Slučaj: Gledamo u ključ koji je već ubačen (želimo ga nazad)
                else if (hit.collider == keyPanel.takeKeyCollider)
                {
                    SetGiovanniCursorSprite(giovanniHandSprite); // Ruka jer ga uzimamo
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        giovanniInventory.imaObicanKljuc = true; // Vraćamo ključ u inventar
                        keyPanel.TakeKey(); // Vraćamo stari objekt
                    }
                }
                // 3. Slučaj: Gledamo u gumb za pobjedu
                else if (hit.collider == keyPanel.pushButtonCollider)
                {
                    SetGiovanniCursorSprite(giovanniPressSprite); // Kursor za pritisak (prst)
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        keyPanel.PressWinButton(); // Pozivamo funkciju za pobjedu
                    }
                }
                else
                {
                    // Ako smo pogodili panel, ali nismo pogodili nijedan od ova 3 specifična collidera
                    ResetGiovanniCursor();
                }
            }
            // OSTATAK TVOJE LOGIKE (Pajser, Ruka, Kompjuter)
            else if (destructible != null)
            {
                if (destructible.specificTargetCollider != null && hit.collider != destructible.specificTargetCollider)
                {
                    destructible = null;
                }

                if (destructible != null)
                {
                    IsTargetingItem = true;
                    bool hasCrowbar = (giovanniInventory != null && giovanniInventory.imaPajser);

                    if (hasCrowbar)
                    {
                        SetGiovanniCursorSprite(giovanniCrowbarSprite);
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            destructible.DestroyAndReplace();
                        }
                    }
                    else
                    {
                        SetGiovanniCursorSprite(giovanniNoCrowbarSprite);
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            if (audioSource != null && errorSound != null)
                                audioSource.PlayOneShot(errorSound);
                        }
                    }
                }
                else
                {
                    ResetGiovanniCursor();
                }
            }
            else if (item != null)
            {
                IsTargetingItem = true;
                SetGiovanniCursorSprite(giovanniHandSprite);

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (giovanniInventory != null) item.Collect(giovanniInventory);
                }
            }
            else if (computer != null && hit.collider.isTrigger && !computer.isReadyToSend)
            {
                IsTargetingItem = true;
                SetGiovanniCursorSprite(giovanniHandSprite);

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    computer.TurnOn();
                }
            }
            else
            {
                ResetGiovanniCursor();
            }
        }
        else
        {
            ResetGiovanniCursor();
        }
    }

    private void SetGiovanniCursorSprite(Sprite newSprite)
    {
        if (giovanniCursorImage != null && giovanniCursorImage.sprite != newSprite)
        {
            giovanniCursorImage.sprite = newSprite;
        }
    }

    private void ResetGiovanniCursor()
    {
        IsTargetingItem = false;
        SetGiovanniCursorSprite(giovanniNormalSprite);
    }

    private void FindGiovanniInventory()
    {
        if (giovanniInventory == null)
        {
            giovanniInventory = FindFirstObjectByType<GiovanniInventory>();
        }
    }

    public void ActivateDeathCursor()
    {
        isDeathScreenActive = true;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}