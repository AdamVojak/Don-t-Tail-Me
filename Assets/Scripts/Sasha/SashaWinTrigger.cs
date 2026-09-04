using UnityEngine;

public class SashaWinTrigger : MonoBehaviour
{
    private bool isWon = false;

    private void OnTriggerEnter(Collider other)
    {
        // Provjeravamo je li objekt koji je ušao u trigger Sasha
        if (other.CompareTag("Sasha") && !isWon)
        {
            isWon = true;
            Debug.Log("<color=gold>SASHA JE STIGAO DO CILJA! POBJEDA!</color>");

            // 1. INSTANTNI ODZIV: Sasha odmah staje u mjestu!
            SashaController sashaController = other.GetComponent<SashaController>();
            if (sashaController != null)
            {
                // Oduzimamo mu kontrole da ne može trčati dok se zid spušta
                sashaController.SetControlled(false);

                // Zaustavljamo animaciju nogu
                if (sashaController.animacijaNogu != null)
                {
                    sashaController.animacijaNogu.StopPlayback();
                }
                if (sashaController.noge != null)
                {
                    sashaController.noge.SetActive(false);
                }
            }

            // 2. POKREĆEMO POBJEDNIČKU SCENU U GAME MANAGERU!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinCurrentCharacter();
            }
            else
            {
                Debug.LogError("GameManager Instance nije pronađen! Pobjeda se ne može pokrenuti.");
            }
        }
    }
}