using UnityEngine;

public class MirandaRoboticArm : MonoBehaviour
{
    [Header("Reference na Inventar")]
    public MirandaInventory inventar;

    [Header("Hijerarhija Ruke")]
    public GameObject mirandaRuka;
    public Transform spojRuke;
    public Transform lakat;

    [Header("Postavke Šake i Hvatanja")]
    public SpriteRenderer sakaRenderer;
    public Sprite otvorenaSakaSprite;
    public Sprite zatvorenaSakaSprite;

    // OVDJE JE PROMJENA: Umjesto odLakta i radijusa, vučemo fizički Collider šake!
    public SphereCollider sakaCollider;
    public LayerMask pickupLayer;

    [Header("Jednostavne Postavke")]
    public float maksimalniDometRuke = 4f;
    public bool savijajUnutra = true;

    private bool rukaAktivna = false;

    void Start()
    {
        if (inventar == null) inventar = GetComponentInParent<MirandaInventory>();

        if (mirandaRuka != null) mirandaRuka.SetActive(false);
        if (sakaRenderer != null) sakaRenderer.sprite = otvorenaSakaSprite;

        // Ako nisi povukao collider, probaj ga naći na OdLakta objektu
        if (sakaCollider == null && lakat != null)
        {
            sakaCollider = lakat.GetComponentInChildren<SphereCollider>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (inventar != null && inventar.ImaRuku)
            {
                rukaAktivna = !rukaAktivna;
                if (mirandaRuka != null) mirandaRuka.SetActive(rukaAktivna);
            }
        }

        if (!rukaAktivna) return;

        PratiMisa();

        if (Input.GetMouseButtonDown(1)) ZatvoriSaku();
        else if (Input.GetMouseButtonUp(1)) OtvoriSaku();
    }

    void PratiMisa()
    {
        if (Camera.main == null) return;

        // 1. Pozicija miša u YZ ravnini (X je dubina)
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.x - spojRuke.position.x);

        Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);
        targetPos.x = spojRuke.position.x; // Drži metu u istoj YZ ravnini u kojoj je i rame

        // 2. Kut od ramena prema mišu u YZ ravnini
        Vector3 smjerMete = targetPos - spojRuke.position;
        float bazniKutRamena = Mathf.Atan2(smjerMete.y, smjerMete.z) * Mathf.Rad2Deg;

        // 3. Računanje udaljenosti za jednostavno savijanje
        float udaljenost = Vector3.Distance(spojRuke.position, targetPos);
        float faktorSavijanja = Mathf.Clamp01(1f - (udaljenost / maksimalniDometRuke));

        float kutLakta = faktorSavijanja * 130f;
        float kutRamenaOffset = faktorSavijanja * 45f;

        float rotacijaRamena = 0f;
        float rotacijaLakta = 0f;

        if (savijajUnutra)
        {
            rotacijaRamena = bazniKutRamena - kutRamenaOffset;
            rotacijaLakta = kutLakta;
        }
        else
        {
            rotacijaRamena = bazniKutRamena + kutRamenaOffset;
            rotacijaLakta = -kutLakta;
        }

        // --- POPRAVAK: ROTIRAMO SAMO OKO X OSI, Y I Z SU NA 0 ---
        // Ovo ne dira tvoje točke niti ih premješta, a Y rotaciju drži na 0
        spojRuke.localRotation = Quaternion.Euler(-rotacijaRamena, 0f, 0f);
        lakat.localRotation = Quaternion.Euler(-rotacijaLakta, 0f, 0f);
    }

    void ZatvoriSaku()
    {
        if (sakaRenderer != null) sakaRenderer.sprite = zatvorenaSakaSprite;
        if (sakaCollider == null) return;

        // OVDJE KORISTIMO KOORDINATE I RADIJUS TVOG COLLIDERA!
        Collider[] pronadjeniPredmeti = Physics.OverlapSphere(sakaCollider.transform.position, sakaCollider.radius, pickupLayer);

        foreach (Collider predmet in pronadjeniPredmeti)
        {
            MirandaPickup item = predmet.GetComponent<MirandaPickup>();

            if (item != null && inventar != null)
            {
                inventar.CollectItem(item.itemTip);
                Destroy(predmet.gameObject);
                Debug.Log("Robotska ruka je uspješno pokupila predmet ID: " + item.itemTip);
            }
        }
    }

    void OtvoriSaku()
    {
        if (sakaRenderer != null) sakaRenderer.sprite = otvorenaSakaSprite;
    }

    private void OnDrawGizmosSelected()
    {
        if (sakaCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sakaCollider.transform.position, sakaCollider.radius);
        }
    }
}