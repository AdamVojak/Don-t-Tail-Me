using UnityEngine;
using UnityEngine.UI;

public class ClickHintMiranda : MonoBehaviour
{
    [Header("UI Elementi Hinta")]
    public GameObject hintsObject; // GameObject hinta iznad glave
    public Sprite hintClick;       // Sličica tipke (npr. tipka 'F')

    private SpriteRenderer hintImg;
    private Image hintUIImg;

    void Start()
    {
        if (hintsObject != null)
        {
            hintImg = hintsObject.GetComponent<SpriteRenderer>();
            hintUIImg = hintsObject.GetComponent<Image>();
            hintsObject.SetActive(false); // Ugašen na početku
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 1. Sigurno pronalazimo Mirandu bez obzira na kojem je djetetu collider
        MirandaController miranda = other.GetComponentInParent<MirandaController>();

        // 2. Ako je Miranda u triggeru, pod kontrolom igrača i živa -> UPALI HINT
        if (miranda != null && miranda.isControlled && miranda.currentState != MirandaController.MirandaState.Dead)
        {
            if (hintsObject != null)
            {
                if (!hintsObject.activeSelf) hintsObject.SetActive(true);

                // Postavi sličicu hinta (podržava i SpriteRenderer i UI Image)
                if (hintClick != null)
                {
                    if (hintImg != null) hintImg.sprite = hintClick;
                    if (hintUIImg != null) hintUIImg.sprite = hintClick;
                }
            }
        }
        else
        {
            // Ako prebaciš na drugog lika dok stojiš u triggeru -> UGASI HINT
            if (hintsObject != null && hintsObject.activeSelf)
            {
                hintsObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Kada Miranda izađe iz zone ventilacije -> UGASI HINT
        MirandaController miranda = other.GetComponentInParent<MirandaController>();
        if (miranda != null)
        {
            if (hintsObject != null)
            {
                hintsObject.SetActive(false);
            }
        }
    }
}