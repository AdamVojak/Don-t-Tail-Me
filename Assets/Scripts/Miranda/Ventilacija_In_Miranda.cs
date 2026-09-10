using UnityEngine;
using UnityEngine.UI;

public class Ventilacija_In_Miranda : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MirandaInventory inventory;
    [SerializeField] private MirandaController playerController;
    [SerializeField] private MirandaAudio mirandaAudio;

    [Header("Primatelji (Ventilacije)")]
    [SerializeField] private Ventilacija_Out_Sasha sashaVentilacija;
    [SerializeField] private Ventilacija_Out_Giovanni giovanniVentilacija;

    // =========================================================
    // NOVO: CENTRALNA BAZA SVIH PREDMETA (Kao kod Sashe)
    // =========================================================
    [System.Serializable]
    public struct ItemBaza
    {
        public string naziv;
        public int itemID;
        public Sprite itemSprite;
    }

    [Header("Baza Svih Predmeta")]
    public ItemBaza[] sviPredmeti;

    [Header("UI Paneli (Ovisno o broju itema)")]
    [Tooltip("Glavni roditelj cijelog UI-ja")]
    [SerializeField] private GameObject uiPanel;
    [Tooltip("Panel koji ima 3 slota (Za mod Sva 3 lika)")]
    [SerializeField] private GameObject panel3Slota;
    [Tooltip("Panel koji ima 2 slota (Za modove S/M i G/M)")]
    [SerializeField] private GameObject panel2Slota;

    [Header("UI Elementi (Uvuci iz OBA panela)")]
    [Tooltip("Uvuci svih 5 Image komponenti (3 iz prvog panela, 2 iz drugog)")]
    [SerializeField] private Image[] itemImages;
    [Tooltip("Uvuci svih 5 okvira za selekciju")]
    [SerializeField] private GameObject[] itemSelectionFrames;

    [Header("UI Primatelji (Radio Gumbi)")]
    [SerializeField] private GameObject sashaSelectionFrame;
    [SerializeField] private GameObject giovanniSelectionFrame;

    private bool isPlayerNear = false;
    private bool isUIOpen = false;
    private bool isFlashing = false;

    private int selectedItemIndex = 0;
    private int selectedRecipientIndex = 0; // 0 = Sasha, 1 = Giovanni

    // Dinamične varijable koje skripta sama postavlja
    private int[] currentSlotIDs;
    private int brojAktivnihSlotova = 3;
    private int offsetSlika = 0; // 0 za panel s 3 slota, 3 za panel s 2 slota

    void Start()
    {
        // OSIGURAČ: Gasimo apsolutno sve panele na početku igre!
        if (uiPanel != null) uiPanel.SetActive(false);
        if (panel3Slota != null) panel3Slota.SetActive(false);
        if (panel2Slota != null) panel2Slota.SetActive(false);

        if (mirandaAudio == null && playerController != null)
        {
            mirandaAudio = playerController.GetComponent<MirandaAudio>();
        }

        // =========================================================
        // SAMO ODREĐUJEMO POSTAVKE (NEMA PALJENJA U STARTU!)
        // =========================================================
        if (GameModeConfigurator.Instance != null)
        {
            var mode = GameModeConfigurator.Instance.activeMode;

            if (mode == GameModeConfigurator.GameMode.MirandaAndGiovanni)
            {
                currentSlotIDs = new int[] { 5, 6 }; // Pajser, ObicanKljuc
                brojAktivnihSlotova = 2;
                offsetSlika = 3;
            }
            else if (mode == GameModeConfigurator.GameMode.SashaAndMiranda)
            {
                currentSlotIDs = new int[] { 6, 2 }; // ObicanKljuc, Minigun
                brojAktivnihSlotova = 2;
                offsetSlika = 3;
            }
            else
            {
                // Default (Sva 3 lika)
                currentSlotIDs = new int[] { 5, 1, 2 }; // Pajser, Gun, Minigun
                brojAktivnihSlotova = 3;
                offsetSlika = 0;
            }
        }
        else
        {
            // Fallback
            currentSlotIDs = new int[] { 5, 1, 2 };
            brojAktivnihSlotova = 3;
            offsetSlika = 0;
        }
    }

    private Sprite GetSpriteForID(int id)
    {
        foreach (var item in sviPredmeti)
        {
            if (item.itemID == id) return item.itemSprite;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            isPlayerNear = true;
            if (playerController != null) playerController.isInteracting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            isPlayerNear = false;
            if (playerController != null) playerController.isInteracting = false;
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            if (isUIOpen) CloseUI();
            else OpenUI();
        }

        if (!isUIOpen) return;

        HandleNavigation();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TrySendItem();
        }
    }

    public void OpenUI()
    {
        StopAllCoroutines();
        isFlashing = false;

        isUIOpen = true;
        if (uiPanel != null) uiPanel.SetActive(true);

        if (brojAktivnihSlotova == 2)
        {
            if (panel2Slota != null) panel2Slota.SetActive(true);
            if (panel3Slota != null) panel3Slota.SetActive(false);
        }
        else
        {
            if (panel3Slota != null) panel3Slota.SetActive(true);
            if (panel2Slota != null) panel2Slota.SetActive(false);
        }

        if (playerController != null) playerController.SetLock(true);
        if (mirandaAudio != null) mirandaAudio.EnterInteractiveState();

        selectedItemIndex = 0;
        selectedRecipientIndex = 0;

        UpdateUI();
    }

    public void CloseUI()
    {
        isUIOpen = false;

        if (uiPanel != null) uiPanel.SetActive(false);
        if (panel3Slota != null) panel3Slota.SetActive(false);
        if (panel2Slota != null) panel2Slota.SetActive(false);

        if (playerController != null) playerController.SetLock(false);
        if (mirandaAudio != null) mirandaAudio.ExitInteractiveState();
    }

    private void HandleNavigation()
    {
        int prevItemIndex = selectedItemIndex;
        int prevRecipientIndex = selectedRecipientIndex;

        if (Input.GetKeyDown(KeyCode.W)) selectedRecipientIndex = 0;
        else if (Input.GetKeyDown(KeyCode.S)) selectedRecipientIndex = 1;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.D)) selectedItemIndex++;
        else if (scroll > 0f || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.A)) selectedItemIndex--;

        if (selectedItemIndex >= brojAktivnihSlotova) selectedItemIndex = 0;
        if (selectedItemIndex < 0) selectedItemIndex = brojAktivnihSlotova - 1;

        if (prevItemIndex != selectedItemIndex || prevRecipientIndex != selectedRecipientIndex)
        {
            if (mirandaAudio != null) mirandaAudio.PlayNavClick();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // 1. AŽURIRANJE SLIČICA PREDMETA
        for (int i = 0; i < brojAktivnihSlotova; i++)
        {
            int checkID = currentSlotIDs[i];
            int uiIndex = i + offsetSlika; // Preskačemo slike iz ugašenog panela!

            if (itemImages[uiIndex] != null)
            {
                itemImages[uiIndex].sprite = GetSpriteForID(checkID);

                if (inventory != null && inventory.HasItem(checkID))
                    itemImages[uiIndex].color = Color.white;
                else
                    itemImages[uiIndex].color = Color.black;
            }

            if (itemSelectionFrames[uiIndex] != null)
            {
                itemSelectionFrames[uiIndex].SetActive(i == selectedItemIndex);
            }
        }

        // 2. AŽURIRANJE PRIMATELJA (Sasha / Giovanni)
        bool sashaU_Igri = GameManager.Instance != null && GameManager.Instance.sashaOdabran;
        bool giovanniU_Igri = GameManager.Instance != null && GameManager.Instance.giovanniOdabran;

        bool sashaSlobodan = sashaU_Igri && sashaVentilacija != null && sashaVentilacija.MozePrimiti();
        bool giovanniSlobodan = giovanniU_Igri && giovanniVentilacija != null && giovanniVentilacija.MozePrimiti;

        if (sashaSelectionFrame != null)
        {
            sashaSelectionFrame.SetActive(selectedRecipientIndex == 0);
            Image img = sashaSelectionFrame.GetComponent<Image>();
            if (img != null) img.color = sashaSlobodan ? Color.white : Color.red;
        }

        if (giovanniSelectionFrame != null)
        {
            giovanniSelectionFrame.SetActive(selectedRecipientIndex == 1);
            Image img = giovanniSelectionFrame.GetComponent<Image>();
            if (img != null) img.color = giovanniSlobodan ? Color.white : Color.red;
        }
    }

    private void TrySendItem()
    {
        int itemIDToSend = currentSlotIDs[selectedItemIndex];

        if (!inventory.HasItem(itemIDToSend))
        {
            Debug.Log("Miranda nema odabrani predmet za slanje!");
            StartCoroutine(FlashUIRoutine());
            return;
        }

        bool sashaU_Igri = GameManager.Instance != null && GameManager.Instance.sashaOdabran;
        bool giovanniU_Igri = GameManager.Instance != null && GameManager.Instance.giovanniOdabran;

        bool uspjesno = false;

        if (selectedRecipientIndex == 0 && sashaU_Igri && sashaVentilacija != null && sashaVentilacija.MozePrimiti())
        {
            sashaVentilacija.SpremiItemZaSashu(itemIDToSend);
            uspjesno = true;
        }
        else if (selectedRecipientIndex == 1 && giovanniU_Igri && giovanniVentilacija != null && giovanniVentilacija.MozePrimiti)
        {
            giovanniVentilacija.SpremiItemZaGiovannia(itemIDToSend);
            uspjesno = true;
        }

        if (uspjesno)
        {
            if (mirandaAudio != null)
            {
                mirandaAudio.PlayConfirmClick();
                mirandaAudio.PlayItemDrop();
            }

            inventory.RemoveItem(itemIDToSend);
            CloseUI();
        }
        else
        {
            StartCoroutine(FlashUIRoutine());
        }
    }

    private System.Collections.IEnumerator FlashUIRoutine()
    {
        if (isFlashing) yield break;
        isFlashing = true;

        for (int f = 0; f < 3; f++)
        {
            for (int i = 0; i < brojAktivnihSlotova; i++)
            {
                int uiIndex = i + offsetSlika;
                if (itemImages[uiIndex] != null) itemImages[uiIndex].color = Color.red;
            }
            yield return new WaitForSeconds(0.2f);

            UpdateUI();
            yield return new WaitForSeconds(0.2f);
        }

        isFlashing = false;
        UpdateUI();
    }
}