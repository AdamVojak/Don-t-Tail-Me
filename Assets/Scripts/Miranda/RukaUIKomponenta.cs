using UnityEngine;
using UnityEngine.UI;

public class RukaUIKomponenta : MonoBehaviour
{

    public MirandaController miranda;
    private bool aktivan;

    public Image ruka;

    private void OnEnable()
    {
        if (miranda != null)
        {
            miranda = FindAnyObjectByType<MirandaController>();
        }
    }

    void Update()
    {
        aktivan = miranda.rukaAktivna ? true : false;

        if (aktivan)
        {
            ruka.color = Color.white;
        }
        else
        {
            ruka.color = Color.black;
        }
    }
}
