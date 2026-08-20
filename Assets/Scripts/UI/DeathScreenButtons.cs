using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Dodan IPointerClickHandler za automatsku detekciju klika
public class DeathScreenButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum ButtonType { Restart, Exit }

    [Header("Postavke Gumba")]
    public ButtonType tipGumba;

    [Header("Hover Efekt")]
    public Image slikaGumba;
    public Sprite normalniSprite;
    public Sprite hoverSprite;

    private void Start()
    {
        if (slikaGumba == null) slikaGumba = GetComponent<Image>();
        if (slikaGumba != null && normalniSprite != null) slikaGumba.sprite = normalniSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slikaGumba != null && hoverSprite != null) slikaGumba.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slikaGumba != null && normalniSprite != null) slikaGumba.sprite = normalniSprite;
    }

    // NOVO: Unityjev EventSystem automatski poziva ovo na klik!
    public void OnPointerClick(PointerEventData eventData)
    {
        IzvrsiAkciju();
    }

    public void IzvrsiAkciju()
    {
        if (tipGumba == ButtonType.Restart)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (tipGumba == ButtonType.Exit)
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}