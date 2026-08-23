using UnityEngine;

public class ClickHintMiranda : MonoBehaviour
{
    public GameObject hintsObject;
    private SpriteRenderer hintImg;
    public Sprite hintClick;

    private MirandaController mirandaController;
    private bool isMirandaNear = false;

    private void Start()
    {
        mirandaController = FindFirstObjectByType<MirandaController>();

        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintsObject.SetActive(false);
        }
    }

    private void Update()
    {
        // OSIGURAČ 1: Ako nismo našli Sashu u Startu (zbog loadinga), tražimo je ponovno!
        if (mirandaController == null)
        {
            mirandaController = FindFirstObjectByType<MirandaController>();
            if (mirandaController == null) return; // Ako je i dalje nema, prekidamo Update da izbjegnemo error
        }

        // OSIGURAČ 2: Ako nismo dodali hint objekt u Inspectoru
        if (hintsObject == null || hintImg == null) return;

        // Glavna logika
        if (isMirandaNear && mirandaController.currentState == MirandaController.MirandaState.Active)
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
        if (other.CompareTag("Miranda")) isMirandaNear = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Miranda")) isMirandaNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Miranda")) isMirandaNear = false;
    }
}