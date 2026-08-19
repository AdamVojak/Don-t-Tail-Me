using UnityEngine;

public class Zombie_Hodanje : MonoBehaviour
{
    private bool lijevaNoga = true;
    public SFX zvucniEfekti;
    private AudioSource zvuk1;
    private AudioSource zvuk2;

    private Zombi zombiGlavni;

    void Awake()
    {
        zombiGlavni = GetComponentInParent<Zombi>();

        if (zvucniEfekti == null)
        {
            zvucniEfekti = FindFirstObjectByType<SFX>();
        }

        if (zvucniEfekti != null)
        {
            zvuk1 = zvucniEfekti.Zombie_ZvukKoraka1;
            zvuk2 = zvucniEfekti.Zombie_ZvukKoraka2;
        }
        else
        {
            Debug.LogWarning(gameObject.name + " ne može pronaći SFX u sceni!");
        }
    }

    public void PlayAlternatingFootstepSound()
    {
        if (lijevaNoga)
        {
            zvuk1.Play();
        }
        else
        {
            zvuk2.Play();
        }
        lijevaNoga = !lijevaNoga;

        if (zombiGlavni != null)
        {
            zombiGlavni.NapraviKorak();
        }
    }
}