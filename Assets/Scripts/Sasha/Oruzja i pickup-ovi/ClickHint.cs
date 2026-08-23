using UnityEngine;

public class ClickHint : MonoBehaviour
{
    public GameObject hintsObject;
    private SpriteRenderer hintImg;
    public Sprite hintClick;

    private SashaController sashaController;
    private bool isSashaNear = false; // Pamtimo je li Sasha fizički u zoni

    private void Start()
    {
        if (sashaController == null) sashaController = FindFirstObjectByType<SashaController>();

        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintsObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isSashaNear && sashaController.currentState == SashaController.SashaState.Active && sashaController.isControlled)
        {
            hintsObject.SetActive(true);
            hintImg.sprite = hintClick;
        }
        else
        {
            hintsObject.SetActive(false);
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