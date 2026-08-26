using UnityEngine;
using UnityEngine.UI; // Potrebno za pristup UI elementima
using System.Collections; // Potrebno za Coroutine

public class SrceUI : MonoBehaviour
{
    public Animator srceAnimator; // Referenca na Animator komponentu srca
    public SashaController playerScript; // Referenca na Player skriptu

    private int trenutniZivot; // Trenutni broj života

    private KucanjeSrca zvukovi;

    void Start()
    {
        // Provjeri jesu li reference postavljene
        if (srceAnimator == null)
        {
            srceAnimator = GetComponent<Animator>();
            if (srceAnimator == null)
            {
                Debug.LogError("Animator komponenta nije pronađena na objektu srca!");
                enabled = false; // Onemogući skriptu ako nema Animatora
                return;
            }
        }

        if (playerScript == null)
        {
            // Pokušaj pronaći Player skriptu ako nije ručno postavljena
            //playerScript = FindObjectOfType<SashaController>();
            if (playerScript == null)
            {
                Debug.LogError("SashaController skripta nije pronađena u sceni! Molimo postavite referencu.");
                enabled = false;
                return;
            }
        }

        // Inicijaliziraj broj života i ažuriraj animaciju
        trenutniZivot = (int)playerScript.zivot;
        UpdateHeartAnimation(trenutniZivot);
    }

    void Update()
    {
        // Provjeravaj promjene u broju života iz Player skripte
        if (trenutniZivot != playerScript.zivot)
        {
            trenutniZivot = (int)playerScript.zivot;
            UpdateHeartAnimation(trenutniZivot);
        }
    }

    void UpdateHeartAnimation(int lives)
    {
        if (srceAnimator == null) return;

        // Postavi "Lives" parametar u Animatoru
        srceAnimator.SetInteger("zivoti", lives);

        // Ako nema života, pauziraj animaciju
        if (lives <= 0)
        {
            srceAnimator.speed = 0; // Pauziraj Animator
            zvukovi.PlayFlatline();
        }
    }
}
