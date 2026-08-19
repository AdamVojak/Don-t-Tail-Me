using UnityEngine;
using UnityEngine.UI;

public class Ventilacija_In_Miranda : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MirandaInventory inventory;
    [SerializeField] private MirandaController playerController;

    [Header("Primatelji (Ventilacije)")]
    // Ovdje ćeš povući skripte ventilacija koje izbacuju iteme kod Sashe i Giovannija
    [SerializeField] private Ventilacija_Out_Sasha sashaVentilacija;
    [SerializeField] private Ventilacija_Out_Giovanni giovanniVentilacija;

    [Header("UI Elementi")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Image[] itemImages; // 0=Pajser, 1=Gun, 2=Minigun
    [SerializeField] private Sprite[] itemSprites; // 0=Pajser, 1=Gun, 2=Minigun
    [SerializeField] private GameObject[] itemSelectionFrames;

    [Header("UI Primatelji (Radio Gumbi)")]
    [SerializeField] private GameObject sashaSelectionFrame; // Okvir koji označava da je odabran Sasha
    [SerializeField] private GameObject giovanniSelectionFrame; // Okvir koji označava da je odabran Giovanni

    private bool isPlayerNear = false;
    private bool isUIOpen = false;
    private bool isFlashing = false;

    private int selectedItemIndex = 0;
    private int selectedRecipientIndex = 0; // 0 = Sasha, 1 = Giovanni

    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        for(int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].sprite = itemSprites[i];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda")) isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda")) isPlayerNear = false;
    }

    void Update()
    {
        // Otvaranje / Zatvaranje na tipku F
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            if (isUIOpen) CloseUI();
            else OpenUI();
        }

        if (!isUIOpen) return;

        HandleNavigation();

        // Slanje na Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            TrySendItem();
        }
    }

    private void OpenUI()
    {
        isUIOpen = true;
        uiPanel.SetActive(true);
        playerController.SetLock(true);

        selectedItemIndex = 0;
        selectedRecipientIndex = 0; // Defaultno je odabran Sasha (W)

        UpdateUI();
    }

    private void CloseUI()
    {
        isUIOpen = false;
        uiPanel.SetActive(false);
        playerController.SetLock(false);
    }

    private void HandleNavigation()
    {
        int prevItemIndex = selectedItemIndex;
        int prevRecipientIndex = selectedRecipientIndex;

        // Navigacija za PRIMATELJA (W / S)
        if (Input.GetKeyDown(KeyCode.W)) selectedRecipientIndex = 0; // Sasha
        else if (Input.GetKeyDown(KeyCode.S)) selectedRecipientIndex = 1; // Giovanni

        // Navigacija za PREDMET (A / D / Q / E / Scroll)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f || Input.GetKeyDown(KeyCode.E)) selectedItemIndex++;
        else if (scroll > 0f || Input.GetKeyDown(KeyCode.Q)) selectedItemIndex--;

        // Cikličko prebacivanje predmeta (0 do 2)
        if (selectedItemIndex > 2) selectedItemIndex = 0;
        if (selectedItemIndex < 0) selectedItemIndex = 2;

        if (prevItemIndex != selectedItemIndex || prevRecipientIndex != selectedRecipientIndex)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 3; i++)
        {
            int checkID = (i == 0) ? 5 : (i == 1 ? 1 : 2);

            if (inventory != null)
            {
                itemImages[i].color = inventory.HasItem(checkID) ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            itemSelectionFrames[i].SetActive(i == selectedItemIndex);
        }

        bool sashaSlobodan = sashaVentilacija != null && sashaVentilacija.MozePrimiti();
        bool giovanniSlobodan = giovanniVentilacija != null && giovanniVentilacija.MozePrimiti;

        if (sashaSelectionFrame != null)
        {
            sashaSelectionFrame.SetActive(selectedRecipientIndex == 0 || !sashaSlobodan);
            Image img = sashaSelectionFrame.GetComponent<Image>();
            if (img != null) img.color = sashaSlobodan ? Color.white : Color.red;
        }

        if (giovanniSelectionFrame != null)
        {
            giovanniSelectionFrame.SetActive(selectedRecipientIndex == 1 || !giovanniSlobodan);
            Image img = giovanniSelectionFrame.GetComponent<Image>();
            if (img != null) img.color = giovanniSlobodan ? Color.white : Color.red;
        }
    }

    private void TrySendItem()
    {
        int itemIDToSend = (selectedItemIndex == 0) ? 5 : (selectedItemIndex == 1 ? 1 : 2);

        if (inventory.HasItem(itemIDToSend))
        {
            if (selectedRecipientIndex == 0 && sashaVentilacija != null)
            {
                if (!sashaVentilacija.MozePrimiti())
                {
                    StartCoroutine(FlashUIRoutine());
                    return;
                }
                sashaVentilacija.SpremiItemZaSashu(itemIDToSend);
                Debug.Log("Miranda je poslala predmet ID: " + itemIDToSend + " Sashi.");
            }
            else if (selectedRecipientIndex == 1 && giovanniVentilacija != null)
            {
                if (!giovanniVentilacija.MozePrimiti)
                {
                    StartCoroutine(FlashUIRoutine());
                    return;
                }
                giovanniVentilacija.SpremiItemZaGiovannia(itemIDToSend);
                Debug.Log("Miranda je poslala predmet ID: " + itemIDToSend + " Giovanniju.");
            }
            else
            {
                Debug.LogWarning("Primatelj nije ispravno povezan u Inspectoru!");
                return;
            }

            inventory.RemoveItem(itemIDToSend);
            CloseUI();
        }
        else
        {
            Debug.Log("Miranda nema odabrani predmet za slanje!");
        }
    }

    private System.Collections.IEnumerator FlashUIRoutine()
    {
        if (isFlashing) yield break;
        isFlashing = true;

        for (int f = 0; f < 4; f++)
        {
            for (int i = 0; i < itemImages.Length; i++)
            {
                itemImages[i].color = Color.black;
                if (itemSelectionFrames.Length > i && itemSelectionFrames[i] != null)
                {
                    Image frameImg = itemSelectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.red;
                    itemSelectionFrames[i].SetActive(true);
                }
            }
            yield return new WaitForSeconds(0.25f);

            for (int i = 0; i < itemImages.Length; i++)
            {
                int checkID = (i == 0) ? 5 : (i == 1 ? 1 : 2);
                itemImages[i].color = inventory.HasItem(checkID) ? Color.white : Color.red;

                if (itemSelectionFrames.Length > i && itemSelectionFrames[i] != null)
                {
                    Image frameImg = itemSelectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.white;
                }
            }
            yield return new WaitForSeconds(0.25f);
        }

        isFlashing = false;
        UpdateUI();
    }
}