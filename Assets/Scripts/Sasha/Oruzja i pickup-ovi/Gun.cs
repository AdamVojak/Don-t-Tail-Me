using System;
using UnityEngine;
using TMPro;

public class Gun : MonoBehaviour
{
    [Header("Tehnikalije")]
    public GameObject projectile;
    public Transform shotPlace;
    public float timeBetween;
    private float shotTime;
    public int ammo;

    [Header("Animacije")]
    public Animator animacijaGun;
    public GameObject gun;


    private int fullAmmo;
    private int dodatak;

    void Start()
    {
        animacijaGun.SetInteger("Shot", ammo);
        animacijaGun.SetBool("Shooting", false);
        animacijaGun.SetBool("NoAmmo", false);
        fullAmmo = ammo;
    }

    public void zvukRepetiranja()
    {
        SFX.zvucniEfekti.ZvukRepetiranjaOruzja.Play();
    }

    public void zvukPucanja()
    {
        SFX.zvucniEfekti.ZvukPucanja.Play();
        Instantiate(projectile, shotPlace.position, transform.rotation);
        ammo--;
    }

    public void nemaMetaka()
    {
        SFX.zvucniEfekti.ZvukNemaMetaka.Play();
        if (ammo != 0)
        {
        SFX.zvucniEfekti.ZvukNemaMetaka.Stop();
        }
    }

    void Update()
    {
        ammo += dodatak;
        if (dodatak != 0) { dodatak = 0; }
        animacijaGun.SetInteger("Shot", ammo);
        animacijaGun.SetBool("Shooting", false);
        Vector2 path = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(path.y, path.x) * Mathf.Rad2Deg;

        if (ammo == 0)
        {
            animacijaGun.SetBool("NoAmmo", true);
        } else animacijaGun.SetBool("NoAmmo", false);

        if (projectile != null)
        {

            if (Input.GetMouseButton(0))
            {
                if (Time.time >= shotTime && ammo != 0)
                {
                    animacijaGun.SetInteger("Shot", ammo);
                    animacijaGun.SetBool("Shooting", true);
                    shotTime = Time.time + timeBetween;
                }
            }
        }
    }

    public void AddAmmo(int pickup)
    {
        dodatak = pickup;
        SFX.zvucniEfekti.ZvukPickup.Play();
    }
}
