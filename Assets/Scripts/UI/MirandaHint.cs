using UnityEngine;
using UnityEngine.UI;

public class MirandaHint : MonoBehaviour
{
    public GameObject UI;
    private Image hintUI;
    public GameObject HintMiranda;
    public Sprite clickF;

    private SpriteRenderer ovajRenderer;
    private Sprite ovajSprite;

    private bool UIOpen = false;
    private MirandaController mirandaUKrugu = null; // Sprema referencu samo dok je Miranda u zoni

    void Start()
    {
        ovajRenderer = GetComponent<SpriteRenderer>();
        if (ovajRenderer != null) ovajSprite = ovajRenderer.sprite;

        if (UI != null)
        {
            hintUI = UI.GetComponent<Image>();
            UI.SetActive(false);
        }

        if (HintMiranda != null)
        {
            HintMiranda.SetActive(false);
        }
    }

    void Update()
    {
        // Radi SAMO ako je Miranda fizički u krugu I ako je trenutno kontrolirana (isControlled)
        if (mirandaUKrugu != null && mirandaUKrugu.isControlled)
        {
            if (HintMiranda != null && !HintMiranda.activeSelf)
            {
                HintMiranda.SetActive(true);
                SpriteRenderer sr = HintMiranda.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = clickF;
            }

            // Pritisak tipke F za otvaranje/zatvaranje papira
            if (Input.GetKeyDown(KeyCode.F))
            {
                UIOpen = !UIOpen;
                if (UI != null) UI.SetActive(UIOpen);

                if (UIOpen && hintUI != null)
                {
                    hintUI.sprite = ovajSprite;
                }
            }
        }
        else
        {
            // Ako Miranda nije u krugu ILI si prebacio na drugog lika -> ugasi hint i zatvori papir
            if (HintMiranda != null && HintMiranda.activeSelf) HintMiranda.SetActive(false);

            if (UIOpen)
            {
                UIOpen = false;
                if (UI != null) UI.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            // Uzimamo kontroler direktno iz collidera/roditelja
            mirandaUKrugu = other.GetComponentInParent<MirandaController>();
            if (mirandaUKrugu != null) mirandaUKrugu.isInteracting = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Osigurač ako se soba upali dok je Miranda već unutra
        if (other.CompareTag("Miranda") && mirandaUKrugu == null)
        {
            mirandaUKrugu = other.GetComponentInParent<MirandaController>();
            if (mirandaUKrugu != null) mirandaUKrugu.isInteracting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            if (mirandaUKrugu != null)
            {
                mirandaUKrugu.isInteracting = false;
                mirandaUKrugu = null; // Brišemo referencu jer je izašla
            }
        }
    }
}