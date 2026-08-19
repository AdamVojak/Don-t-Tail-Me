using UnityEngine;

public class Hodanje : MonoBehaviour
{
    private bool lijevaNoga = true;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void PlayAlternatingFootstepSound()
    {
        if (lijevaNoga)
        {
            SFX.zvucniEfekti.ZvukKoraka1.Play();
        }
        else
        {
            SFX.zvucniEfekti.ZvukKoraka2.Play();
        }
        lijevaNoga = !lijevaNoga;
    }
}
