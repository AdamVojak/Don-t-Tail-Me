using UnityEngine;
using UnityEngine.UI;

public class MirandaHint : MonoBehaviour
{
    [Header("Papir UI")]
    public GameObject UI;
    private Image hintUI;

    [Header("Hint")]
    public Sprite clickF;

    private SpriteRenderer ovajRenderer;
    private Sprite ovajSprite;

    private bool UIOpen = false;
    private bool isPlayerInside = false;
    private MirandaController mirandaRef = null;

    void Start()
    {
        ovajRenderer = GetComponent<SpriteRenderer>();
        if (ovajRenderer != null) ovajSprite = ovajRenderer.sprite;

        if (UI != null)
        {
            hintUI = UI.GetComponent<Image>();
            UI.SetActive(false);
        }
    }

    void Update()
    {
        // Radi samo ako je Miranda u krugu i pod kontrolom igrača
        if (isPlayerInside && mirandaRef != null && mirandaRef.isControlled)
        {
            // Pritisak tipke F za čitanje papira
            if (Input.GetKeyDown(KeyCode.F))
            {
                UIOpen = !UIOpen;
                if (UI != null) UI.SetActive(UIOpen);

                if (UIOpen && hintUI != null) hintUI.sprite = ovajSprite;

                // Dok čita papir, ugasi hint iznad glave. Kad zatvori, upali ga opet.
                if (UIOpen)
                {
                    mirandaRef.SakrijHint(this);
                }
                else
                {
                    mirandaRef.PrikaziHint(this, clickF);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MirandaController miranda = other.GetComponentInParent<MirandaController>();
        if (miranda != null)
        {
            isPlayerInside = true;
            mirandaRef = miranda;
            miranda.isInteracting = true; // Zaključava druge akcije dok je kod papira

            // Pali hint samo ako papir nije već otvoren
            if (!UIOpen)
            {
                miranda.PrikaziHint(this, clickF);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MirandaController miranda = other.GetComponentInParent<MirandaController>();
        if (miranda != null)
        {
            isPlayerInside = false;
            mirandaRef = null;
            miranda.isInteracting = false;

            // Zatvori papir ako je ostao otvoren
            if (UIOpen)
            {
                UIOpen = false;
                if (UI != null) UI.SetActive(false);
            }

            // Ugasi hint iznad glave
            miranda.SakrijHint(this);
        }
    }
}