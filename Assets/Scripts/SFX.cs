using UnityEngine;

public class SFX : MonoBehaviour
{
    public static SFX zvucniEfekti;

    [Header("Sasha - audio sources")]
    public AudioSource ZvukRepetiranjaOruzja;
    public AudioSource ZvukPucanja;
    public AudioSource ZvukPucanjaMinigun;
    public AudioSource ZvukPraznogKlika;
    public AudioSource ZvukLomaStakla;
    public AudioSource ZvukGunSkok;
    public AudioSource ZvukGunDoskok;
    public AudioSource ZvukGunPickup;
    public AudioSource ZvukKoraka1;
    public AudioSource ZvukKoraka2;
    public AudioSource ZvukUdarca1;
    public AudioSource ZvukUdarca2;
    public AudioSource Zombie_ZvukKoraka1;
    public AudioSource Zombie_ZvukKoraka2;
    public AudioSource ZvukIzmjeneOruzja1;
    public AudioSource ZvukIzmjeneOruzja2;
    public AudioSource ZvukIzmjeneOruzja3;

    public AudioSource ZvukIzmjeneOruzja4;
    public AudioSource ZvukNemaMetaka;
    public AudioSource ZvukPickup;
    public AudioSource ZvukSmrti;
    public AudioSource ZvukZamaha;
    public AudioSource ZvukUdarca;
    public AudioSource ZvukElektrosoka;
    public AudioSource ZvukRibljeGlave;
    public AudioSource ZvukRibljeGlave2;
    public AudioSource ZvukKacige;
    public AudioSource ZvukNeuspjehaUdarcaVent;
    public AudioSource ZvukUspjehaUdarcaVent;

    [Header("Miranda - audio sources")]
    public AudioSource ZvukPadanjaKrozVentilacije;
    public AudioSource ZvukIzlaskaIzVentilacije;
    public AudioSource ZvukMetalnogPoda;
    public AudioSource ZvukOtvaranjaVrata;
    public AudioSource zvukMiniAlert;

    [Header("Giovanni - audio sources")]
    public AudioSource ZvukMobitelaL;
    public AudioSource ZvukMobitelaR;
    public AudioSource ZvukMobitelaFwd;
    public AudioSource ZvukMobitelaNon;

    void Awake()
    {
        if (zvucniEfekti == null)
        {
            zvucniEfekti = this;
        }
        else if (zvucniEfekti != this)
        {
            Destroy(gameObject);
        }
    }

    /*void Start()
    {
        zvucniEfekti = this;
    }

    
    void Update()
    {
        
    }*/
}
