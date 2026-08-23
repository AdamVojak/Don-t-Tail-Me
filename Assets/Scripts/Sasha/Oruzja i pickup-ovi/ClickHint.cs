using UnityEngine;

public class ClickHint : MonoBehaviour
{
    public GameObject hintsObject;
    private SpriteRenderer hintImg;
    public Sprite hintClick;

    private SashaController sashaController;


    private void Start()
    {
        if (sashaController == null)
        {
            sashaController = FindFirstObjectByType<SashaController>();
        }

        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintsObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha") || sashaController.currentState == SashaController.SashaState.Active)
        {
            hintsObject.SetActive(true);
                hintImg.sprite = hintClick;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            hintsObject.SetActive(false);
        }
    }
}
