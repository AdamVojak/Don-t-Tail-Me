using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KljucanicaObican : MonoBehaviour
{
    [Header("Postavke")]
    [SerializeField] private int obicanKeyID = 6;

    [Header("Vizualne Reference")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite otkljucanObican;

    [Header("UI Feedback")]
    [SerializeField] private Image uiImage;
    [SerializeField] private GameObject uiFrame;

    [Header("Audio")]
    [SerializeField] private MirandaAudio mirandaAudio; // Ovdje povuci objekt s MirandaAudio skriptom

    [Header("Poveznica")]
    [SerializeField] private HorizontalnaVrataMiranda vrata;

    private bool playerInside = false;
    private bool otkljucana = false;
    public bool Otkljucana => otkljucana; // Property da vrata mogu provjeriti stanje

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda")) playerInside = false;
    }

    void Update()
    {
        if (playerInside && !otkljucana && Input.GetKeyDown(KeyCode.F))
        {
            MirandaInventory inv = Object.FindFirstObjectByType<MirandaInventory>();

            if (inv != null && inv.HasItem(obicanKeyID))
            {
                // ZVUK
                if (mirandaAudio != null) mirandaAudio.PlayKeyUse();
                UnlockDoor();
            }
            else
            {
                if (mirandaAudio != null) mirandaAudio.PlayError();
                StartCoroutine(FlashUIRoutine());
            }
        }
    }

    private void UnlockDoor()
    {
        otkljucana = true;

        if (spriteRenderer != null && otkljucanObican != null)
        {
            spriteRenderer.sprite = otkljucanObican;
        }

        // Ugasi Hint i renderer
        ClickHintMiranda hintScript = GetComponent<ClickHintMiranda>();
        if (hintScript != null)
        {
            MirandaController miranda = Object.FindFirstObjectByType<MirandaController>();
            if (miranda != null) miranda.SakrijHint(hintScript);
            hintScript.enabled = false;
        }

        // Obavijesti vrata (ovo će odmah pokrenuti otvaranje u HorizontalVrata)
        if (vrata != null) vrata.ProvjeriKljucanicu();

        // Ugasi collider da se ne može ponovno "kliknuti"
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }

    private IEnumerator FlashUIRoutine()
    {
        if (uiImage == null || uiFrame == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            uiImage.color = Color.black;
            uiFrame.GetComponent<Image>().color = Color.red;
            yield return new WaitForSeconds(0.2f);

            uiImage.color = Color.white;
            uiFrame.GetComponent<Image>().color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }
    }
}