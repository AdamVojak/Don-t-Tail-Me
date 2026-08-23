using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class HintBreakVent : MonoBehaviour
{
    public GameObject hintsObject;
    private SpriteRenderer hintImg;
    public Sprite hintClick;
    public Sprite hintF;
    public Ventilacija_In_Sasha ventInRef;
    private SashaController sashaController;
    public bool otvoren = false;


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

        if (ventInRef == null)
        {
            ventInRef = FindFirstObjectByType<Ventilacija_In_Sasha>();
        }
    }

    private void Update()
    {
            otvoren = ventInRef.jeOtvorena;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sasha") && sashaController.currentState == SashaController.SashaState.Active)
        {
            hintsObject.SetActive(true);
            if (!otvoren)
            {
                hintImg.sprite = hintClick;
            }
            else
            {
                hintImg.sprite = hintF;
            }
        }

        if (other.CompareTag("Sasha") && sashaController.currentState != SashaController.SashaState.Active)
        {
            hintsObject.SetActive(false);
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
