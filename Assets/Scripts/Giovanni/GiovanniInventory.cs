using UnityEngine;
using UnityEngine.UI;

public class GiovanniInventory : MonoBehaviour
{
    [Header("Poveznica Mobitel - UI")]
    public GameObject mobitelUI;

    [Header("UI Ikone Predmeta")]
    public Image mobitelIconImage;
    public Image pajserIconImage;

    [Header("Postavke Boja Ikona")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.25f, 0.25f, 0.25f, 0.35f);

    [Header("Posjed predmeta (Giovanni)")]
    [SerializeField] private bool imaRuku = false;
    [SerializeField] private bool imaMobitel = false;
    [SerializeField] private bool imaGun = false;
    [SerializeField] private bool imaMinigun = false;
    [SerializeField] public bool imaPajser = false;
    [SerializeField] public bool imaObicanKljuc = false;

    public const int ID_GUN = 1;
    public const int ID_MINIGUN = 2;
    public const int ID_ARM = 3;
    public const int ID_PAJSER = 5;
    public const int ID_OBICAN_KLJUC = 6;
    public const int ID_MOBITEL = 10;

    public bool ImaRuku => imaRuku;
    public bool ImaMobitel => imaMobitel;
    public bool ImaGun => imaGun;
    public bool ImaMinigun => imaMinigun;
    public bool ImaPajser => imaPajser;
    public bool ImaObicanKljuc => imaObicanKljuc;

    private void Start()
    {
            mobitelUI.SetActive(false);
        UpdateInventoryUI();
    }

    public void CollectItem(int itemID)
    {
        if (itemID == ID_ARM)
        {
            if (!imaRuku)
            {
                imaRuku = true;
                Debug.Log("Giovanni je pokupio Ruku!");
            }
        }
        else if (itemID == ID_MOBITEL)
        {
            if (!imaMobitel)
            {
                imaMobitel = true;
                if (mobitelUI != null) mobitelUI.SetActive(true);
                Debug.Log("Giovanni je pokupio Mobitel!");
            }
        }
        else if (itemID == ID_GUN)
        {
            if (!imaGun)
            {
                imaGun = true;
                Debug.Log("Giovanni je pokupio Gun!");
            }
        }
        else if (itemID == ID_MINIGUN)
        {
            if (!imaMinigun)
            {
                imaMinigun = true;
                Debug.Log("Giovanni je pokupio Minigun (3D model)!");
            }
        }
        else if (itemID == ID_PAJSER)
        {
            if (!imaPajser)
            {
                imaPajser = true;
                Debug.Log("Giovanni je pokupio Pajser!");
            }
        }
        else if (itemID == ID_OBICAN_KLJUC)
        {
            if (!imaObicanKljuc)
            {
                imaObicanKljuc = true;
                Debug.Log("Giovanni je pokupio Običan ključ!");
            }
        }
        else
        {
            Debug.LogWarning("Giovanni ne može pokupiti nepoznat predmet s ID-jem: " + itemID);
        }

        UpdateInventoryUI();
    }

    public bool HasItem(int itemID)
    {
        if (itemID == ID_ARM) return imaRuku;
        if (itemID == ID_GUN) return imaGun;
        if (itemID == ID_MINIGUN) return imaMinigun;
        if (itemID == ID_MOBITEL) return imaMobitel;
        if (itemID == ID_PAJSER) return imaPajser;
        if (itemID == ID_OBICAN_KLJUC) return imaObicanKljuc;
        return false;
    }

    public void RemoveItem(int itemID)
    {
        if (itemID == ID_ARM) imaRuku = false;
        else if (itemID == ID_GUN) imaGun = false;
        else if (itemID == ID_MINIGUN) imaMinigun = false;
        else if (itemID == ID_MOBITEL) imaMobitel = false;
        else if (itemID == ID_PAJSER) imaPajser = false;
        else if (itemID == ID_OBICAN_KLJUC) imaObicanKljuc = false;

        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        if (mobitelIconImage != null)
        {
            mobitelIconImage.color = imaMobitel ? activeColor : inactiveColor;
        }

        if (pajserIconImage != null)
        {
            pajserIconImage.color = imaPajser ? activeColor : inactiveColor;
        }
    }
}