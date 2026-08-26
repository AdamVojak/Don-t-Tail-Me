using UnityEngine;
using UnityEngine.UI;

public class RukaUIKomponenta : MonoBehaviour
{

    public MirandaController miranda;
    private bool aktivan;

    public Image ruka;
    private SpriteRenderer sprite;

    private void OnEnable()
    {
        if (miranda != null)
        {
            miranda = FindAnyObjectByType<MirandaController>();
        }
        if (ruka != null)
        {
            sprite = ruka.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        aktivan = miranda.rukaAktivna ? true : false;

        if (aktivan)
        {
            sprite.color = Color.white;
        }
        else
        {
            sprite.color = Color.black;
        }
    }
}
