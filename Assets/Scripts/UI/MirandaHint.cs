using UnityEngine;
using UnityEngine.UI;

public class MirandaHint : MonoBehaviour
{
    [Header("Papir UI")]
    public GameObject UI;
    private Image hintUI;

    [Header("Hint")]
    public SpriteRenderer hintMirandaRenderer;
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
        if (isPlayerInside && mirandaRef != null && mirandaRef.isControlled)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                UIOpen = !UIOpen;
                if (UI != null) UI.SetActive(UIOpen);

                if (UIOpen && hintUI != null) hintUI.sprite = ovajSprite;

                if (hintMirandaRenderer != null) hintMirandaRenderer.enabled = !UIOpen;
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
            miranda.isInteracting = true;

            if (hintMirandaRenderer != null && !UIOpen)
            {
                if (clickF != null) hintMirandaRenderer.sprite = clickF;
                hintMirandaRenderer.enabled = true;
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

            if (UIOpen)
            {
                UIOpen = false;
                if (UI != null) UI.SetActive(false);
            }

            if (hintMirandaRenderer != null)
            {
                hintMirandaRenderer.enabled = false;
            }
        }
    }
}