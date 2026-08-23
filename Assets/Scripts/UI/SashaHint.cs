using UnityEngine;
using UnityEngine.UI;

public class SashaHint : MonoBehaviour
{
    public GameObject UI;
    private Image hintUI;
    public GameObject HintSasha;
    public Sprite clickF;
    private SpriteRenderer ovajRenderer;
    private Sprite ovajSprite;

    private bool UIOpen = false;
    private SashaController sashaUKrugu = null; // Sprema Sashu samo dok je u zoni

    void Start()
    {
        ovajRenderer = GetComponent<SpriteRenderer>();
        if (ovajRenderer != null) ovajSprite = ovajRenderer.sprite;

        if (UI != null)
        {
            hintUI = UI.GetComponent<Image>();
            UI.SetActive(false);
        }

        if (HintSasha != null)
        {
            HintSasha.SetActive(false);
        }
    }

    void Update()
    {
        // Radi SAMO ako je Sasha fizički u krugu I ako je trenutno aktivna
        if (sashaUKrugu != null && sashaUKrugu.currentState == SashaController.SashaState.Active)
        {
            if (HintSasha != null && !HintSasha.activeSelf)
            {
                HintSasha.SetActive(true);
                SpriteRenderer sr = HintSasha.GetComponent<SpriteRenderer>();
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
            // Ako Sasha nije u krugu ili si prebacio lika -> ugasi hint i zatvori papir
            if (HintSasha != null && HintSasha.activeSelf) HintSasha.SetActive(false);

            if (UIOpen)
            {
                UIOpen = false;
                if (UI != null) UI.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            // Dajemo Update metodi referencu direktno iz collidera
            sashaUKrugu = other.GetComponentInParent<SashaController>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha") && sashaUKrugu == null)
        {
            sashaUKrugu = other.GetComponentInParent<SashaController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            sashaUKrugu = null; // Brišemo referencu jer je Sasha otišla
        }
    }
}