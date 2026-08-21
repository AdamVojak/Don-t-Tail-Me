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
    private bool miranda = false;
    private bool UIOpen = false;

    private void Start()
    {
        ovajRenderer = GetComponent<SpriteRenderer>();
        ovajSprite = ovajRenderer.sprite;

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

    private void Update()
    {
        if (miranda)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                UIOpen = !UIOpen;

                UI.SetActive(UIOpen);

                if (UIOpen)
                {
                    hintUI.sprite = ovajSprite;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            other.GetComponent<MirandaController>().isInteracting = true;
            miranda = true;

            if (HintMiranda != null)
            {
                HintMiranda.SetActive(true);
                SpriteRenderer sr = HintMiranda.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = clickF;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda"))
        {
            miranda = false;
            UIOpen = false;
            if (HintMiranda != null) HintMiranda.SetActive(false);
            if (UI != null) UI.SetActive(false);
            other.GetComponent<MirandaController>().isInteracting = false;
        }
    }
}