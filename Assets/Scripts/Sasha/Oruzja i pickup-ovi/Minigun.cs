using UnityEngine;

public class Minigun : MonoBehaviour
{
    [Header("Ammo Status")]
    public bool punAmmo;
    public static int maxAmmo = 70;

    [Header("Tehnikalije")]
    public GameObject bullet;
    public float timeBetween = 0.1f;
    public int ammo;
    private int fullAmmo;
    private float shotTime;

    [Header("Minigun Mehanika")]
    public float windUpDuration = 0.75f;
    private float currentWindUpTime = 0f;
    public float maxAnimSpeed = 2f;

    [Header("Cijevi i Flash (Muzzle)")]
    public Transform shotPlace0;
    public Transform shotPlace1;
    public Transform shotPlace2;
    public Transform shotPlace3;
    public float flashDuration = 0.05f;
    private float flashTimer = 0f;
    private int barrel = 0;

    [Header("Reference")]
    public Animator animacijaGun;
    public GameObject Clip;

    [Header("Audio (2D)")]
    [SerializeField] private AudioSource minigunAudioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip emptyClickClip;
    [SerializeField] private AudioClip pickupClip;

    public float zvukPucanja = 0.5f;

    public float masterZvukova = 0.6f;

    void Start()
    {
        if (minigunAudioSource == null) minigunAudioSource = gameObject.AddComponent<AudioSource>();
        minigunAudioSource.spatialBlend = 0f; // 2D zvuk

        fullAmmo = ammo;
        barrel = 0;

        UgasSveBljeskove();

        if (animacijaGun != null) animacijaGun.speed = 0f;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            currentWindUpTime += Time.deltaTime;
            if (currentWindUpTime > windUpDuration) currentWindUpTime = windUpDuration;
        }
        else
        {
            currentWindUpTime -= Time.deltaTime;
            if (currentWindUpTime < 0) currentWindUpTime = 0;
        }

        punAmmo = (ammo >= maxAmmo);
        if (ammo > maxAmmo) ammo = maxAmmo;

        float spinRatio = currentWindUpTime / windUpDuration;
        if (animacijaGun != null) animacijaGun.speed = spinRatio * maxAnimSpeed;

        if (Input.GetMouseButton(0) && currentWindUpTime >= windUpDuration)
        {
            if (Time.time >= shotTime)
            {
                Pucanje();
                shotTime = Time.time + timeBetween;
            }
        }

        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0 || !Input.GetMouseButton(0))
            {
                UgasSveBljeskove();
            }
        }

        if (Clip != null) Clip.SetActive(ammo > 0);
    }

    public void Pucanje()
    {
        if (ammo <= 0)
        {
            if (minigunAudioSource != null && emptyClickClip != null)
                minigunAudioSource.PlayOneShot(emptyClickClip);
            return;
        }

        ammo--;


        if (minigunAudioSource != null && shootClip != null)
        {
            minigunAudioSource.pitch = Random.Range(0.95f, 1.05f);
            minigunAudioSource.PlayOneShot(shootClip, zvukPucanja);
        }

        Transform currentShotPlace = null;

        switch (barrel)
        {
            case 0: currentShotPlace = shotPlace0; break;
            case 1: currentShotPlace = shotPlace1; break;
            case 2: currentShotPlace = shotPlace2; break;
            case 3: currentShotPlace = shotPlace3; break;
        }

        if (bullet != null && currentShotPlace != null)
        {
            Instantiate(bullet, currentShotPlace.position, transform.rotation);

            UgasSveBljeskove();
            currentShotPlace.gameObject.SetActive(true);
            flashTimer = flashDuration;
        }

        barrel++;
        if (barrel > 3) barrel = 0;
    }

    private void UgasSveBljeskove()
    {
        if (shotPlace0 != null) shotPlace0.gameObject.SetActive(false);
        if (shotPlace1 != null) shotPlace1.gameObject.SetActive(false);
        if (shotPlace2 != null) shotPlace2.gameObject.SetActive(false);
        if (shotPlace3 != null) shotPlace3.gameObject.SetActive(false);
    }

    public void AddAmmo(int pickup)
    {
        ammo += pickup;
        if (ammo > maxAmmo) ammo = maxAmmo;

        if (minigunAudioSource != null && pickupClip != null)
            minigunAudioSource.PlayOneShot(pickupClip);
    }
}