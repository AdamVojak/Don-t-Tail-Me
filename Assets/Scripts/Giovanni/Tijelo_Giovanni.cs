using UnityEngine;

public class TijeloGiovanni : MonoBehaviour
{
    public bool uhvacen = false;
    public bool aktiviranBomb = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Viper"))
        {
            uhvacen = true;
        }
        if (other.CompareTag("Bomb"))
        {
            aktiviranBomb = true;
        }
    }
}
