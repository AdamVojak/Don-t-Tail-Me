using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Kljucanice : MonoBehaviour
{
    public enum PotrebanTipKljuca
    {
        Zuti = 0,        // ID 0 u unificiranom inventaru
        Ljubicasti = 4   // ID 4 u unificiranom inventaru
    }

    [Header("Postavke Ključanice")]
    [SerializeField] private PotrebanTipKljuca potrebniKljuc;

    [Header("Vizuali")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite otkljucanaZuta;
    [SerializeField] private Sprite otkljucanaLjub;

    [Header("Vrata")]
    [SerializeField] private Vrata vrata;

    [Header("Audio i UI Feedback")]
    [SerializeField] private MirandaAudio mirandaAudio;
    [SerializeField] private Image uiImage;
    [SerializeField] private GameObject uiFrame;
    [SerializeField] private GameObject keyUIIcon;

    [HideInInspector] public bool otkljucana = false;
    private bool playerInside = false;

    // Prati ulazak Mirande radi logike pritiska tipke F
    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Miranda")) playerInside = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Miranda")) playerInside = false; }

    [Header("Collider za isključivanje")]
    [SerializeField] private Collider triggerCollider;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Samo ako je igrač u triggeru, vrata nisu otključana i stisne F
        if (playerInside && !otkljucana && Input.GetKeyDown(KeyCode.F))
        {
            MirandaInventory inv = Object.FindFirstObjectByType<MirandaInventory>();

            if (inv != null && inv.HasItem((int)potrebniKljuc))
            {
                // Uspješno otključavanje
                if (mirandaAudio != null) mirandaAudio.PlayKeyUse();
                UnlockDoor(inv);
            }
            else
            {
                // Error logika (Nema ključa)
                if (mirandaAudio != null) mirandaAudio.PlayError();
                StartCoroutine(FlashUIRoutine(uiImage, uiFrame));
            }
        }
    }

    private IEnumerator FlashUIRoutine(Image targetImage, GameObject frame)
    {
        if (targetImage == null || frame == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            targetImage.color = Color.black;
            frame.GetComponent<Image>().color = Color.red;
            yield return new WaitForSeconds(0.2f);

            targetImage.color = Color.white;
            frame.GetComponent<Image>().color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void UnlockDoor(MirandaInventory mirandaInventory)
    {
        otkljucana = true;
        mirandaInventory.RemoveItem((int)potrebniKljuc);

        // 1. ISKLJUČI COLLIDER (Sada je ključanica "mrtva" za fiziku)
        if (triggerCollider != null) triggerCollider.enabled = false;

        // 2. UNIŠTI UI IKONU KLJUČA
        if (keyUIIcon != null) Destroy(keyUIIcon);

        // 3. UGASI HINT I SAKRIJ GA S EKRANA
        ClickHintMiranda hintScript = GetComponent<ClickHintMiranda>();
        if (hintScript != null)
        {
            MirandaController miranda = Object.FindFirstObjectByType<MirandaController>();
            if (miranda != null) miranda.SakrijHint(hintScript);

            hintScript.enabled = false;
        }

        // 3. Promjena spritea
        if (potrebniKljuc == PotrebanTipKljuca.Zuti && otkljucanaZuta != null)
        {
            spriteRenderer.sprite = otkljucanaZuta;
        }
        else if (potrebniKljuc == PotrebanTipKljuca.Ljubicasti && otkljucanaLjub != null)
        {
            spriteRenderer.sprite = otkljucanaLjub;
        }

        // 4. Obavijesti vrata
        if (vrata != null)
        {
            vrata.ProvjeriKljucanice();
        }
    }
}