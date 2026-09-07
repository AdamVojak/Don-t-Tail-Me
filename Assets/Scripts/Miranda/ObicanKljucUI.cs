using UnityEngine;
using UnityEngine.UI;

public class ObicanKljucUI : MonoBehaviour
{
    public MirandaInventory mirandaInv;
    private bool imaGa;
    public Image kljuc;

    private void Start()
    {
        if (mirandaInv != null)
        {
            mirandaInv = FindAnyObjectByType<MirandaInventory>();
        }
    }

    private void Update()
    {
        imaGa = mirandaInv.ImaObicanKljuc ? true : false;

        if (imaGa)
        {
            kljuc.color = Color.white;
        }
        else
        {
            kljuc.color = Color.black;
        }
    }
}
