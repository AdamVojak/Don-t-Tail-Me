using UnityEngine;

public class KeyPanelController : MonoBehaviour
{
    [Header("Verzije objekta")]
    public GameObject panelWithoutKey;
    public GameObject panelWithKey;

    [Header("Specifični Collideri (Triggeri)")]
    public Collider insertKeyCollider;
    public Collider takeKeyCollider;
    public Collider pushButtonCollider;

    [Header("Poveznice")]
    public GiovanniElevatorEscape elevatorEscape;
    public CursorManager cursorManager; // NOVO: Referenca na tvoj CursorManager

    [Header("Audio (Opcionalno)")]
    public AudioSource audioSource;
    public AudioClip errorSound;

    private Camera mainCam;
    private bool isShowingNoPowerCursor = false; // Pamti jesmo li već promijenili kursor

    void Start()
    {
        panelWithoutKey.SetActive(true);
        panelWithKey.SetActive(false);
        mainCam = Camera.main;

        if (cursorManager == null) cursorManager = FindFirstObjectByType<CursorManager>();
    }

    void Update()
    {
        // =========================================================================
        // LOGIKA ZA KURSOR: Gleda li Giovanni u gumb za pobjedu?
        // =========================================================================
        bool lookingAtButton = false;

        if (panelWithKey.activeSelf && pushButtonCollider != null && mainCam != null)
        {
            Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            {
                if (hit.collider == pushButtonCollider)
                {
                    lookingAtButton = true;
                }
            }
        }

        // Ako gleda u gumb, a NEMA struje -> Pokaži munju!
        if (lookingAtButton && !ImaDovoljnoStruje())
        {
            if (!isShowingNoPowerCursor && cursorManager != null)
            {
                cursorManager.SetNoPowerCursor(true);
                isShowingNoPowerCursor = true;
            }
        }
        // Ako ne gleda u gumb ILI ima struje -> Vrati normalni kursor!
        else
        {
            if (isShowingNoPowerCursor && cursorManager != null)
            {
                cursorManager.SetNoPowerCursor(false);
                isShowingNoPowerCursor = false;
            }
        }
    }

    public bool ImaDovoljnoStruje()
    {
        if (GameManager.Instance != null && GameManager.Instance.mirandaOdabrana)
        {
            if (EnergyManager.Instance != null)
            {
                return EnergyManager.Instance.struja > 10f;
            }
        }
        return true;
    }

    public void InsertKey()
    {
        panelWithoutKey.SetActive(false);
        panelWithKey.SetActive(true);
    }

    public void TakeKey()
    {
        panelWithKey.SetActive(false);
        panelWithoutKey.SetActive(true);
    }

    public void PressWinButton()
    {
        if (!ImaDovoljnoStruje())
        {
            Debug.Log("<color=red>LIFT ODBIJEN: Nema dovoljno struje!</color>");
            if (audioSource != null && errorSound != null) audioSource.PlayOneShot(errorSound);
            return;
        }

        Debug.Log("<color=green>Gumb je pritisnut! Ima struje, pokrećem lift.</color>");

        // Vraćamo kursor u normalu prije nego što lift krene
        if (isShowingNoPowerCursor && cursorManager != null)
        {
            cursorManager.SetNoPowerCursor(false);
            isShowingNoPowerCursor = false;
        }

        if (elevatorEscape != null)
        {
            elevatorEscape.PokreniBijegLiftom();
        }
    }
}