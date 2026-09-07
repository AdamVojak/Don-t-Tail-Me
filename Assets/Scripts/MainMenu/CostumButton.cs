using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CustomImageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Vizualna Stanja i Boje")]
    [SerializeField] private Color normalColor = new Color(0.8f, 0.9f, 1f, 0.8f);   // Početna boja
    [SerializeField] private Color hoverColor = new Color(0.6f, 0.05f, 0.05f, 1f); // Tamno crvena (Hover)
    [SerializeField] private Color pressedColor = new Color(1f, 0.2f, 0.2f, 1f);   // Intenzivni bljesak (Klik)
    [SerializeField] private float colorTransitionSpeed = 12f;

    [Header("Skaliranje (Animacija Veličine)")]
    [SerializeField] private bool enableScaleEffect = true;
    [SerializeField] private float hoverScale = 1.05f;   // Lagano povećanje na hover
    [SerializeField] private float pressedScale = 0.95f; // Lagano utisnuće na klik

    [Header("Vanjski Zvukovi (SFX)")]
    [SerializeField] private AudioSource audioSource;

    [Space(5)]
    [SerializeField] private AudioClip clickSFX;
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1.0f;

    [Header("Klik Događaj")]
    public UnityEvent onClick;

    private Image img;
    private Color targetColor;
    private Vector3 originalScale;
    private Vector3 targetScale;

    private bool isHovered = false;
    private bool isPressed = false;
    private bool isInteractable = true;

    private void Awake()
    {
        img = GetComponent<Image>();
        img.color = normalColor;
        targetColor = normalColor;

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void OnEnable()
    {
        // Resetiramo stanja ako se panel upali/ugasi
        isHovered = false;
        isPressed = false;
        targetColor = normalColor;
        targetScale = originalScale;
        if (img != null) img.color = normalColor;
        transform.localScale = originalScale;
    }

    private void Update()
    {
        // Glatki prijelaz boje
        img.color = Color.Lerp(img.color, targetColor, Time.deltaTime * colorTransitionSpeed);

        // Glatko skaliranje
        if (enableScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * colorTransitionSpeed);
        }
    }

    #region Pointer Događaji

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isHovered = true;
        if (!isPressed)
        {
            targetColor = hoverColor;
            if (enableScaleEffect) targetScale = originalScale * hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isHovered = false;
        isPressed = false;

        // Vraća se u normalno stanje tek kada miš STVARNO ode s gumba
        targetColor = normalColor;
        if (enableScaleEffect) targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = true;
        // Prikazuje boju pritiska i "utiskuje" gumb
        targetColor = pressedColor;
        if (enableScaleEffect) targetScale = originalScale * pressedScale;

        PlaySound(clickSFX, clickVolume);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = false;

        // Ako je miš i dalje iznad gumba, ostajemo u HOVER stanju!
        if (isHovered)
        {
            targetColor = hoverColor;
            if (enableScaleEffect) targetScale = originalScale * hoverScale;
        }
        else
        {
            targetColor = normalColor;
            if (enableScaleEffect) targetScale = originalScale;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;

        // Pokrećemo akciju, a vizualno ostajemo u hoveru ako je miš još uvijek tu
        onClick?.Invoke();
    }

    #endregion

    #region Pomoćne Funkcije

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void SetInteractable(bool state)
    {
        isInteractable = state;
        if (!state)
        {
            targetColor = normalColor;
            targetScale = originalScale;
        }
    }

    #endregion
}