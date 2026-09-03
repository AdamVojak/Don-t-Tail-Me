using UnityEngine;

public class MirandaWinTrigger : MonoBehaviour
{
    private bool isWon = false;

    private void OnTriggerEnter(Collider other)
    {
        // Kada Miranda dotakne ovaj collider na kraju hodnika:
        if (other.CompareTag("Miranda") && !isWon)
        {
            isWon = true;
            Debug.Log("<color=gold>MIRANDA JE POBJEGLA! POBJEDA!</color>");

            // Pozivamo našu univerzalnu pobjedničku scenu u GameManageru!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinCurrentCharacter();
            }
        }
    }
}