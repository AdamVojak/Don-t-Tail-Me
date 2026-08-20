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
            // NOVO: Računamo stvarnu udaljenost između Giovannija i predmeta u koji gledaš
            float distanceToPlayer = Vector3.Distance(giovanniInventory.transform.position, hit.point);

            // Ako je Giovanni predaleko od predmeta (npr. dalje od 4.5m), kursor ostaje normalan i nema interakcije!
            if (distanceToPlayer > maxInteractionDistance)
            {
                ResetGiovanniCursor();
                return;
            }

            CollectibleItem item = hit.collider.GetComponentInParent<CollectibleItem>();
            Computer computer = hit.collider.GetComponentInParent<Computer>();
            DestructibleObject destructible = hit.collider.GetComponentInParent<DestructibleObject>();

            // Logika za uništive objekte (Pajser)
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

                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        if (audioSource != null && errorSound != null)
                        {
                            audioSource.PlayOneShot(errorSound);
                        }
                    }
                }
            }
            // Logika za sakupljanje predmeta (Ruka)
            else if (item != null)
            {
                IsTargetingItem = true;
                SetGiovanniCursorSprite(giovanniHandSprite);

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (giovanniInventory != null)
                    {
                        item.Collect(giovanniInventory);
                    }
                }
            }
            // Logika za računalo
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