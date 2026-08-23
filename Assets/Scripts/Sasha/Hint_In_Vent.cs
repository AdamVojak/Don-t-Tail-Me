using UnityEngine;

public class HintBreakVent : MonoBehaviour
{
    public GameObject hintsObject;
    private SpriteRenderer hintImg;
    public Sprite hintClick;
    public Sprite hintF;
    public Ventilacija_In_Sasha ventInRef;

    void Start()
    {
        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintsObject.SetActive(false);
        }

        if (ventInRef == null)
        {
            ventInRef = FindFirstObjectByType<Ventilacija_In_Sasha>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha"))
        {
            // Uzimamo Sashu direktno iz objekta koji stoji u triggeru
            SashaController sasha = other.GetComponentInParent<SashaController>();

            // Ako je Sasha tu i aktivna je
            if (sasha != null && sasha.currentState == SashaController.SashaState.Active)
            {
                if (hintsObject != null)
                {
                    hintsObject.SetActive(true);

                    // Provjeravamo je li ventilacija otvorena
                    bool otvoren = (ventInRef != null) && ventInRef.jeOtvorena;

                    if (hintImg != null)
                    {
                        hintImg.sprite = otvoren ? hintF : hintClick;
                    }
                }
            }
            else // Ako si prebacio na drugog lika -> ugasi hint
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