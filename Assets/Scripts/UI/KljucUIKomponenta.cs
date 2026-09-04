using UnityEngine;
using UnityEngine.UI; // Potrebno za rad s UI Image komponentom

public class KljucUIKomponenta : MonoBehaviour
{
    public enum UIKljuc
    {
        ZutiKljuc,
        LjubicastiKljuc
    }

    [Header("Key Type for this UI Slot")]
    [SerializeField]
    private UIKljuc tipUIkljuca; // Određuje koji ključ ovaj UI slot predstavlja

    [Header("Sprites")]
    [SerializeField]
    private Sprite imaKljuc; // Sprite kada igrač posjeduje ovaj ključ
    [SerializeField]
    private Sprite nemaKljuc; // Sprite kada igrač NE posjeduje ovaj ključ

    private Image slikaKljuca; // Referenca na Image komponentu ovog UI elementa
    private MirandaInventory mirandaInventory; // Referenca na inventar igrača

    void Awake()
    {
        slikaKljuca = GetComponent<Image>();
        if (slikaKljuca == null)
        {
            Debug.LogError("KljucUIKomponenta: Image komponenta nije pronađena na GameObjectu " + gameObject.name);
            enabled = false; // Onemogući skriptu ako nema Image komponente
            return;
        }

        // Pokušaj pronaći MirandaInventory na igraču
        GameObject playerObject = GameObject.FindGameObjectWithTag("Miranda");
        if (playerObject != null)
        {
            mirandaInventory = playerObject.GetComponentInParent<MirandaInventory>();
        }

        if (mirandaInventory == null)
        {
            enabled = false; // Onemogući skriptu ako nema inventara
        }
    }

    void Update()
    {
        if (mirandaInventory == null) return; // Ako inventar nije pronađen, ne radi ništa

        // Ažuriraj sprite ovisno o tome posjeduje li igrač ključ
        if (tipUIkljuca == UIKljuc.ZutiKljuc)
        {
            if (mirandaInventory.ImaZutiKljuc)
            {
                slikaKljuca.sprite = imaKljuc;
            }
            else
            {
                slikaKljuca.sprite = nemaKljuc;
            }
        }
        else if (tipUIkljuca == UIKljuc.LjubicastiKljuc)
        {
            if (mirandaInventory.ImaLjubicastiKljuc)
            {
                slikaKljuca.sprite = imaKljuc;
            }
            else
            {
                slikaKljuca.sprite = nemaKljuc;
            }
        }
    }
}