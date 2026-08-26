using UnityEngine;
using UnityEngine.UI;

public class Ventilacija_In_Miranda : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MirandaInventory inventory;
    [SerializeField] private MirandaController playerController;
    [SerializeField] private MirandaAudio mirandaAudio; // NOVO: Audio referenca

    [Header("Primatelji (Ventilacije)")]
    [SerializeField] private Ventilacija_Out_Sasha sashaVentilacija;
    [SerializeField] private Ventilacija_Out_Giovanni giovanniVentilacija;

    [Header("UI Elementi")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Image[] itemImages; // 0=Pajser, 1=Gun, 2=Minigun
    [SerializeField] private Sprite[] itemSprites; // 0=Pajser, 1=Gun, 2=Minigun
    [SerializeField] private GameObject[] itemSelectionFrames;

    [Header("UI Primatelji (Radio Gumbi)")]
    [SerializeField] private GameObject sashaSelectionFrame;
    [SerializeField] private GameObject giovanniSelectionFrame;

    private bool isPlayerNear = false;
    private bool isUIOpen = false;
    private bool isFlashing = false;

    private int selectedItemIndex = 0;
    private int selectedRecipientIndex = 0; // 0 = Sasha, 1 = Giovanni

    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);

        if (mirandaAudio == null && playerController != null)
        {
            mirandaAudio = playerController.GetComponent<MirandaAudio>();
        }

        // Postavljamo sve sličice na početku
        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] != null && itemSprites.Length > i)
            {
                itemImages[i].sprite = itemSprites[i];
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            isPlayerNear = true;
            MirandaController mc = other.GetComponentInParent<MirandaController>();
            if (mc != null) mc.isInteracting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            isPlayerNear = false;
            MirandaController mc = other.GetComponentInParent<MirandaController>();
            if (mc != null) mc.isInteracting = false;
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
        uiPanel.SetActive(true);
        if (playerController != null) playerController.SetLock(true);

        // PALI MUZAK GLAZBU:
        if (mirandaAudio != null) mirandaAudio.EnterInteractiveState();

        selectedItemIndex = 0;
        selectedRecipientIndex = 0;

        if (itemSelectionFrames != null)
        {
            for (int i = 0; i < itemSelectionFrames.Length; i++)
            {
                if (itemSelectionFrames[i] != null)
                {
                    Image frameImg = itemSelectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.white;
                }
            }
        }

        UpdateUI();
    }

    public void CloseUI()
    {
        isUIOpen = false;
        uiPanel.SetActive(false);
        if (playerController != null) playerController.SetLock(false);

        // GASI MUZAK GLAZBU:
        if (mirandaAudio != null) mirandaAudio.ExitInteractiveState();
    }

    private void HandleNavigation()
    {
        int prevItemIndex = selectedItemIndex;
        int prevRecipientIndex = selectedRecipientIndex;

        // Navigacija primatelja (W / S)
        if (Input.GetKeyDown(KeyCode.W)) selectedRecipientIndex = 0;
        else if (Input.GetKeyDown(KeyCode.S)) selectedRecipientIndex = 1;

        // Navigacija predmeta (A / D / Q / E / Scroll)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.D)) selectedItemIndex++;
        else if (scroll > 0f || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.A)) selectedItemIndex--;

        if (selectedItemIndex > 2) selectedItemIndex = 0;
        if (selectedItemIndex < 0) selectedItemIndex = 2;

        // ZVUK KLIKA KADA SE PROMIJENI BILO KOJI ODABIR:
        if (prevItemIndex != selectedItemIndex || prevRecipientIndex != selectedRecipientIndex)
        {
            if (mirandaAudio != null) mirandaAudio.PlayNavClick();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // 1. SVI PROZORČIĆI SU UVIJEK VIDLJIVI, SAMO ZATAMNJENI AKO NEMA PREDMETA:
        for (int i = 0; i < 3; i++)
        {
            int checkID = (i == 0) ? 5 : (i == 1 ? 1 : 2); // 0=Pajser (5), 1=Gun (1), 2=Minigun (2)

            if (itemImages[i] != null)
            {
                itemImages[i].gameObject.SetActive(true); // Uvijek upaljen prozorčić!

                // Ako ima predmet -> Bijela (Normalna) boja, ako nema -> Zatamnjeno/Crno kao kod Sashe
                if (inventory != null && inventory.HasItem(checkID))
                {
                    itemImages[i].color = Color.white;
                }
                else
                {
                    itemImages[i].color = Color.black;
                }
            }

            // Samo se okvir odabira pali/gasi ovisno o tome koji je selektiran
            if (itemSelectionFrames[i] != null)
            {
                itemSelectionFrames[i].SetActive(i == selectedItemIndex);
            }
        }

        // 2. Provjera slobodnih cijevi
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
        int itemIDToSend = (selectedItemIndex == 0) ? 5 : (selectedItemIndex == 1 ? 1 : 2);

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
            // ZVUK POTVRDE I PADANJA PREDMETA U VENTILACIJU:
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
            for (int i = 0; i < itemImages.Length; i++)
            {
                if (itemImages[i] != null) itemImages[i].color = Color.red;
            }
            yield return new WaitForSeconds(0.2f);

            UpdateUI();
            yield return new WaitForSeconds(0.2f);
        }

        isFlashing = false;
        UpdateUI();
    }
}