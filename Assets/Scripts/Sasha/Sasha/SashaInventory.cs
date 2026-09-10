using UnityEngine;
using UnityEngine.UI;

public class SashaInventory : MonoBehaviour
{
    [Header("Posjed itema")]
    public bool imaZutiKljuc = false;   // ID 0
    public bool imaGun = false;         // ID 1
    public bool imaMinigun = false;     // ID 2
    public bool imaRuku = false;        // ID 3
    public bool imaPajser = false;      // ID 5
    public bool imaObicanKljuc = false; // ID 6

    private SustavOruzja sustavOruzja;
    private bool isFlashing = false;

    [Header("Prikaz u ruci")]
    public SpriteRenderer itemURuciSpriteRenderer;

    [Header("UI Elementi")]
    public GameObject inventoryUIPanel;
    public Image[] itemImages;
    public GameObject[] selectionFrames;

    // =========================================================
    // NOVO: CENTRALNA BAZA SVIH PREDMETA
    // =========================================================
    [System.Serializable]
    public struct ItemBaza
    {
        public string naziv; // Samo da tebi bude lakše čitati u Inspectoru
        public int itemID;
        public Sprite itemSprite;
    }

    [Header("Baza Svih Predmeta")]
    public ItemBaza[] sviPredmeti;

    // Dinamične varijable koje skripta sama postavlja ovisno o modu
    private int[] currentSlotIDs = new int[3];

    private int selectedIndex = 0;
    private bool isUIOpen = false;

    [Header("Poveznica s Mirandom/Giovannijem")]
    public Ventilacija_Out_Miranda mirandinaVentilacija;
    public Ventilacija_Out_Giovanni giovanniVentilacija;

    [SerializeField] private SashaAudio sashaAudio;

    void Start()
    {
        if (sashaAudio == null) sashaAudio = GetComponent<SashaAudio>();
        if (inventoryUIPanel != null) inventoryUIPanel.SetActive(false);

        imaZutiKljuc = false; imaGun = false; imaMinigun = false;
        imaPajser = false; imaObicanKljuc = false; imaRuku = false;

        // =========================================================
        // SKRIPTA SADA BIRA SAMO ID-ove! (Sličice će naći sama)
        // =========================================================
        if (GameModeConfigurator.Instance != null)
        {
            var mode = GameModeConfigurator.Instance.activeMode;

            if (mode == GameModeConfigurator.GameMode.SashaAndGiovanni)
            {
                currentSlotIDs = new int[] { 5, 6, 1 }; // Pajser, ObicanKljuc, Gun
            }
            else if (mode == GameModeConfigurator.GameMode.SashaAndMiranda)
            {
                currentSlotIDs = new int[] { 0, 6, 3 }; // ZutiKljuc, ObicanKljuc, Ruka
            }
            else
            {
                currentSlotIDs = new int[] { 0, 1, 5 }; // ZutiKljuc, Gun, Pajser (Default)
            }
        }
        else
        {
            currentSlotIDs = new int[] { 0, 1, 5 };
        }
    }

    // Pomoćna metoda koja traži sličicu u bazi prema ID-u
    private Sprite GetSpriteForID(int id)
    {
        foreach (var item in sviPredmeti)
        {
            if (item.itemID == id) return item.itemSprite;
        }
        return null; // Ako ne nađe, vraća prazno
    }

    public void OpenUI()
    {
        StopAllCoroutines();
        isFlashing = false;

        isUIOpen = true;
        inventoryUIPanel.SetActive(true);
        selectedIndex = 0;

        if (selectionFrames != null)
        {
            for (int i = 0; i < selectionFrames.Length; i++)
            {
                if (selectionFrames[i] != null)
                {
                    Image frameImg = selectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.white;
                }
            }
        }

        UpdateUI();
    }

    public void CloseUI()
    {
        isUIOpen = false;
        inventoryUIPanel.SetActive(false);
    }

    public void HandleNavigation()
    {
        if (!isUIOpen) return;

        int previousIndex = selectedIndex;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f) selectedIndex++;
        else if (scroll > 0f) selectedIndex--;

        if (Input.GetKeyDown(KeyCode.E)) selectedIndex++;
        if (Input.GetKeyDown(KeyCode.Q)) selectedIndex--;

        if (selectedIndex > 2) selectedIndex = 0;
        if (selectedIndex < 0) selectedIndex = 2;

        if (previousIndex != selectedIndex)
        {
            if (sashaAudio != null) sashaAudio.PlayNavClick();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 3; i++)
        {
            int checkID = currentSlotIDs[i];

            // Skripta sama pronalazi pravu sličicu za ovaj ID!
            Sprite pronadjeniSprite = GetSpriteForID(checkID);
            if (pronadjeniSprite != null && itemImages.Length > i && itemImages[i] != null)
            {
                itemImages[i].sprite = pronadjeniSprite;
            }

            if (HasItem(checkID))
            {
                itemImages[i].color = Color.white;
            }
            else
            {
                itemImages[i].color = Color.black;
            }

            if (selectionFrames.Length > i && selectionFrames[i] != null)
            {
                selectionFrames[i].SetActive(i == selectedIndex);
            }
        }
    }

    public bool TrySendSelectedItem()
    {
        int itemIDToSend = currentSlotIDs[selectedIndex];

        if (!HasItem(itemIDToSend))
        {
            Debug.Log("Sasha nema odabrani predmet!");
            return false;
        }

        SashaController controller = GetComponent<SashaController>();
        if (controller == null || controller.trenutnaVentilacija == null)
        {
            Debug.LogWarning("Sasha nije kraj ventilacije!");
            return false;
        }

        bool uspjesnoPoslano = controller.trenutnaVentilacija.PrimiItemUVentilaciju(itemIDToSend);

        if (uspjesnoPoslano)
        {
            if (sashaAudio != null)
            {
                sashaAudio.PlayConfirmClick();
                sashaAudio.ExitInteractiveState();
            }

            if (itemURuciSpriteRenderer != null)
            {
                // Skripta sama pronalazi sličicu i za ruku!
                itemURuciSpriteRenderer.sprite = GetSpriteForID(itemIDToSend);
                itemURuciSpriteRenderer.gameObject.SetActive(true);
            }

            RemoveItem(itemIDToSend);
            return true;
        }

        StartCoroutine(FlashUIRoutine());
        return false;
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
                if (selectionFrames.Length > i && selectionFrames[i] != null)
                {
                    Image frameImg = selectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.red;
                    selectionFrames[i].SetActive(true);
                }
            }
            yield return new WaitForSeconds(0.25f);

            for (int i = 0; i < itemImages.Length; i++)
            {
                itemImages[i].color = Color.white;
                if (selectionFrames.Length > i && selectionFrames[i] != null)
                {
                    Image frameImg = selectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.white;
                }
            }
            yield return new WaitForSeconds(0.25f);
        }

        isFlashing = false;
        UpdateUI();
    }

    public void CollectItem(int itemType)
    {
        if (itemType == 0) { imaZutiKljuc = true; Debug.Log("Sasha je pokupio Žuti ključ."); }
        else if (itemType == 1) { imaGun = true; Debug.Log("Sasha je pokupio Gun."); }
        else if (itemType == 2) { imaMinigun = true; Debug.Log("Sasha je pokupio Minigun."); }
        else if (itemType == 3) { imaRuku = true; Debug.Log("Sasha je pokupio Ruku."); }
        else if (itemType == 5) { imaPajser = true; Debug.Log("Sasha je pokupio Pajser."); }
        else if (itemType == 6) { imaObicanKljuc = true; Debug.Log("Sasha je pokupio Običan ključ."); }
        if (isUIOpen) UpdateUI();
    }

    public bool HasItem(int itemType)
    {
        if (itemType == 0) return imaZutiKljuc;
        if (itemType == 1) return imaGun;
        if (itemType == 2) return imaMinigun;
        if (itemType == 3) return imaRuku;
        if (itemType == 5) return imaPajser;
        if (itemType == 6) return imaObicanKljuc;
        return false;
    }

    public void RemoveItem(int itemType)
    {
        if (itemType == 0) imaZutiKljuc = false;
        else if (itemType == 1) imaGun = false;
        else if (itemType == 2) imaMinigun = false;
        else if (itemType == 3) imaRuku = false;
        else if (itemType == 5) imaPajser = false;
        else if (itemType == 6) imaObicanKljuc = false;
        if (isUIOpen) UpdateUI();
    }
}