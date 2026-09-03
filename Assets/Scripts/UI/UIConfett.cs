using UnityEngine;

public class UIConfetti : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float rotationSpeed;
    [HideInInspector] public float lifetime;

    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        // Automatski uništava ovaj konfet nakon 'lifetime' sekundi
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rect != null)
        {
            // Gura konfet prema DOLJE (Vector2.down)
            rect.anchoredPosition += Vector2.down * speed * Time.deltaTime;

            // Rotira konfet oko Z osi
            rect.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}