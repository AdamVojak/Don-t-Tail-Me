using UnityEngine;

public class ClickHint : MonoBehaviour
{
    public GameObject hintsObject;
    private SpriteRenderer hintImg;
    public Sprite hintClick;

    void Start()
    {
        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintsObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            // Uzimamo skriptu direktno s lika koji je dotaknuo trigger
            SashaController sasha = other.GetComponentInParent<SashaController>();

            // Ako je kontroler tu I ako trenutno igraš sa Sashom -> UPALI
            if (sasha != null && sasha.currentState == SashaController.SashaState.Active)
            {
                if (hintsObject != null)
                {
                    hintsObject.SetActive(true);
                    if (hintImg != null) hintImg.sprite = hintClick;
                }
            }
            else // Ako si u triggeru, ali si prebacio na Mirandu -> UGASI
            {
                if (hintsObject != null) hintsObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            if (hintsObject != null) hintsObject.SetActive(false);
        }
    }
}