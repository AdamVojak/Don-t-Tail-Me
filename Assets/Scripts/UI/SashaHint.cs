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

    private bool isSashaNear = false;
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

        if (sashaController == null) sashaController = FindFirstObjectByType<SashaController>();
        if (HintSasha != null) HintSasha.SetActive(false);
    }

    private void Update()
    {
        bool isSashaActive = sashaController.currentState == SashaController.SashaState.Active;

        if (isSashaNear && isSashaActive && sashaController.isControlled)
        {
            if (HintSasha != null && !HintSasha.activeSelf)
            {
                HintSasha.SetActive(true);
                SpriteRenderer sr = HintSasha.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = clickF;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                UIOpen = !UIOpen;
                UI.SetActive(UIOpen);
                if (UIOpen) hintUI.sprite = ovajSprite;
            }
        }
        else
        {
            if (HintSasha != null) HintSasha.SetActive(false);

            if (UIOpen)
            {
                UIOpen = false;
                if (UI != null) UI.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sasha")) isSashaNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha")) isSashaNear = false;
    }
}