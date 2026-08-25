using UnityEngine;
using UnityEngine.UI;

public class SashaInventory : MonoBehaviour
{
    [Header("Posjed itema")]
    public bool imaZutiKljuc = false;
    public bool imaGun = false;
    public bool imaMinigun = false;
    public bool imaPajser = false;
    private SustavOruzja sustavOruzja;
    private bool isFlashing = false;

    [Header("Prikaz u ruci")]
    public SpriteRenderer itemURuciSpriteRenderer;

    [Header("UI Elementi")]
    public GameObject inventoryUIPanel;
    public Image[] itemImages;
    public GameObject[] selectionFrames;

    [Header("Sličice (Sprites)")]
    public Sprite[] itemSprites;

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
        imaZutiKljuc = false;
        imaGun = false;
        imaMinigun = false;
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
            // NOVO MAPIRANJE: 0 = Žuti ključ (ID 0), 1 = Gun (ID 1), 2 = Pajser (ID 5)
            int checkID = (i == 0) ? 0 : (i == 1 ? 1 : 5);

            if (itemSprites.Length > i && itemSprites[i] != null)
            {
                itemImages[i].sprite = itemSprites[i];
            }

            // Provjeravamo točan ID umjesto indeksa 'i'
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
        int itemIDToSend = (selectedIndex == 0) ? 0 : (selectedIndex == 1 ? 1 : 5);

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
                itemURuciSpriteRenderer.sprite = itemSprites[selectedIndex];
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
        else if (itemType == 5) { imaPajser = true; Debug.Log("Sasha je pokupio Pajser."); }
        if (isUIOpen) UpdateUI();
    }

    public bool HasItem(int itemType)
    {
        if (itemType == 0) return imaZutiKljuc;
        if (itemType == 1) return imaGun;
        if (itemType == 2) return imaMinigun;
        if (itemType == 5) return imaPajser;
        return false;
    }

    public void RemoveItem(int itemType)
    {
        if (itemType == 0) imaZutiKljuc = false;
        else if (itemType == 1) imaGun = false; 
        else if (itemType == 2) imaMinigun = false;
        else if (itemType == 5) imaPajser = false;
        if (isUIOpen) UpdateUI();
    }
}