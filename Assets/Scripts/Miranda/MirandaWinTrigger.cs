using UnityEngine;

public class MirandaWinTrigger : MonoBehaviour
{
    private bool isWon = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Miranda") && !isWon)
        {
            isWon = true;

            MirandaController mc = other.GetComponent<MirandaController>();
            if (mc != null) mc.SetLock(true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinCurrentCharacter();
            }
        }
    }
}