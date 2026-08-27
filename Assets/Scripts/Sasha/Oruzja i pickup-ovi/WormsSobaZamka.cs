using UnityEngine;

public class WormsSobaZamka : MonoBehaviour
{
    [Header("Predmet koji pokreće zamku")]
    public GameObject predmetZaPokupiti;

    [Header("Elementi Zamke u Sobi")]
    public GameObject svijetlo;
    public LeverSasha[] lever;
    public GumbSasha[] gumbi;
    public Door vrata;
    private bool trapActivated = false;

    [Header("Audio (2D Kratki Spoj / Power Outage)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerOutageClip;
    [Range(0f, 1f)][SerializeField] private float outageVolume = 1f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f; // 2D zvuk
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        if (!trapActivated && predmetZaPokupiti == null)
        {
            AktivirajZamku();
        }
    }

    void AktivirajZamku()
    {
        trapActivated = true;

        // 1. ZVUK KRATKOG SPOJA / NESTANKA STRUJE
        if (audioSource != null && powerOutageClip != null)
        {
            audioSource.PlayOneShot(powerOutageClip, outageVolume);
        }

        // 2. Uništi svjetlo u sobi
        if (svijetlo != null)
        {
            Destroy(svijetlo.gameObject);
        }

        // 3. Resetiraj gumbe i levere
        foreach (GumbSasha g in gumbi)
        {
            if (g != null) g.aktiviran = false;
        }

        foreach (LeverSasha l in lever)
        {
            if (l != null) l.aktiviran = false;
        }

        // 4. Pokreni vrata
        if (vrata != null)
        {
            vrata.enabled = true;
            vrata.OdrediSmjerIPokreni();
        }

        // 5. PREBACIVANJE NA SUSTAV STRUJE (MIRANDA & ENERGY MANAGER)
        // Rušimo struju na 0 tako da Sasha padne u mrak dok Miranda ne krene trčati na Treadmillu!
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.struja = 0f;
        }

        // Aktiviramo kontroler koji svjetla u Sashinom levelu veže uz količinu struje koju Miranda puni
        if (SashaSvjetlaKontroler.Instance != null)
        {
            SashaSvjetlaKontroler.Instance.AktivirajUpravljanje();
        }

        Debug.Log("Worms Soba: Nestanak struje! Zamka aktivirana i kontrola struje prebačena na Mirandu.");
    }
}