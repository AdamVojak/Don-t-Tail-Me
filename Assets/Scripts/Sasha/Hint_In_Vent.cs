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
    private bool isSashaNear = false; // Pamtimo je li Sasha fizički u zoni

    private void Start()
    {
        if (sashaController == null) sashaController = FindFirstObjectByType<SashaController>();
        if (ventInRef == null) ventInRef = FindFirstObjectByType<Ventilacija_In_Sasha>();

        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintsObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (ventInRef != null) otvoren = ventInRef.jeOtvorena;

        if (isSashaNear && sashaController.currentState == SashaController.SashaState.Active && sashaController.isControlled)
        {
            hintsObject.SetActive(true);

            if (!otvoren) hintImg.sprite = hintClick;
            else hintImg.sprite = hintF;
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