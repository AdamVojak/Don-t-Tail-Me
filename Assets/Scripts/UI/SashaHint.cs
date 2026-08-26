using UnityEngine;
using UnityEngine.UI;

public class SashaHint : MonoBehaviour
{
    [Header("Papir UI")]
    public GameObject UI;
    private Image hintUI;

    [Header("Hint")]
    public SpriteRenderer hintSashaRenderer; // SpriteRenderer iznad glave
    public Sprite clickF;

    private SpriteRenderer ovajRenderer;
    private Sprite ovajSprite;

    private bool UIOpen = false;
    private bool isPlayerInside = false;
    private SashaController sashaRef = null;

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
        // Update radi SAMO ako je igrač fizički u krugu ovog specifičnog papira!
        if (isPlayerInside && sashaRef != null && sashaRef.isControlled)
        {
            // Pritisak tipke F za čitanje papira
            if (Input.GetKeyDown(KeyCode.F))
            {
                UIOpen = !UIOpen;
                if (UI != null) UI.SetActive(UIOpen);

                if (UIOpen && hintUI != null) hintUI.sprite = ovajSprite;

                // Dok čita papir, ugasi hint iznad glave
                if (hintSashaRenderer != null) hintSashaRenderer.enabled = !UIOpen;
            }
        }
        // NEMA VIŠE 'ELSE' BLOKA KOJI GASI DRUGE OBJEKTE!
    }

    private void OnTriggerEnter(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null)
        {
            isPlayerInside = true;
            sashaRef = sasha;

            // PALI HINT SAMO KADA UĐEŠ U OVAJ TRIGGER
            if (hintSashaRenderer != null && !UIOpen)
            {
                if (clickF != null) hintSashaRenderer.sprite = clickF;
                hintSashaRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SashaController sasha = other.GetComponentInParent<SashaController>();
        if (sasha != null)
        {
            isPlayerInside = false;
            sashaRef = null;

            // ZATVORI PAPIR I UGASI HINT SAMO KADA IZAĐEŠ IZ OVOG TRIGGERA
            if (UIOpen)
            {
                UIOpen = false;
                if (UI != null) UI.SetActive(false);
            }

            if (hintSashaRenderer != null)
            {
                hintSashaRenderer.enabled = false;
            }
        }
    }
}