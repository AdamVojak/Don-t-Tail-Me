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
    private bool sasha = false;
    private bool UIOpen = false;

    private SashaController sashaController;

    private void Start()
    {
        ovajRenderer = GetComponent<SpriteRenderer>();
        ovajSprite = ovajRenderer.sprite;

        if (UI != null)
        {
            hintUI = UI.GetComponent<Image>();
            UI.SetActive(false);
        }

        if (sashaController == null)
        {
            sashaController = FindFirstObjectByType<SashaController>();
        }

        if (HintSasha != null)
        {
            HintSasha.SetActive(false);
        }
    }

    private void Update()
    {
        if (sasha && sashaController.currentState == SashaController.SashaState.Active)
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
        if (other.CompareTag("Sasha") && sashaController.currentState == SashaController.SashaState.Active)
        {
            sasha = true;

            if (HintSasha != null)
            {
                HintSasha.SetActive(true);
                SpriteRenderer sr = HintSasha.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = clickF;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            sasha = false;
            UIOpen = false;
            if (HintSasha != null) HintSasha.SetActive(false);
            if (UI != null) UI.SetActive(false);
        }
    }
}