using UnityEngine;

public class UIBubble : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;

    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rect != null)
        {
            rect.anchoredPosition += Vector2.up * speed * Time.deltaTime;
        }
    }
}