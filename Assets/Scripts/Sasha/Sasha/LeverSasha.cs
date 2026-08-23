using UnityEngine;

public class LeverSasha : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite on;
    public Sprite off;

    public bool aktiviran;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = off;
        aktiviran = false;
    }

    private void Update()
    {
        spriteRenderer.sprite = aktiviran ? on : off;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Melee") || other.CompareTag("Pajser"))
        {
            aktiviran = !aktiviran;
            spriteRenderer.sprite = aktiviran ? on : off;
            Debug.Log("Lever activated!");
        }
    }
}
