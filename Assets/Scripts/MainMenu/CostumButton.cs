using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Boje")]
    [SerializeField] private Color normalColor = new Color(0.8f, 0.9f, 1f, 0.8f); // Hladna plavo-bijela
    [SerializeField] private Color hoverColor = new Color(0.6f, 0.05f, 0.05f, 1f); // Tamno crvena (Alarm/Emergency)
    [SerializeField] private float colorTransitionSpeed = 8f;

    [Header("Skaliranje na Hover (Opcionalno)")]
    [SerializeField] private bool scaleOnHover = true;
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Klik Događaj")]
    public UnityEvent onClick;

    private Image img;
    private Color targetColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isInteractable = true;

    private void Awake()
    {
        img = GetComponent<Image>();
        img.color = normalColor;
        targetColor = normalColor;

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // Glatki prijelaz boje (Lerp)
        img.color = Color.Lerp(img.color, targetColor, Time.deltaTime * colorTransitionSpeed);

        // Glatko skaliranje
        if (scaleOnHover)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * colorTransitionSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;
        targetColor = hoverColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = normalColor;
        if (scaleOnHover) targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;

        // Resetiramo izgled odmah pri kliku
        targetColor = normalColor;
        targetScale = originalScale;

        onClick?.Invoke();
    }

    public void SetInteractable(bool state)
    {
        isInteractable = state;
    }
}