using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SustavOruzja : MonoBehaviour
{
    public enum ActiveWp { Melee, Gun, Minigun, Pajser }
    public ActiveWp current;
    private ActiveWp lastActiveWeaponForSasha;

    [Header("Weapons")]
    public GameObject melee;
    public GameObject gun;
    public GameObject minigun;

    public GameObject pajser;

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager;

    [Header("Sasha")]
    [SerializeField] private SashaAudio sashaAudio;
    [SerializeField] GameObject JacinaUdarcaUI;
    [SerializeField] GameObject AmmoUI;
    [SerializeField] GameObject MinigunUI;
    [SerializeField] public Melee meleeOruzjeScript;
    [SerializeField] public Pajser pajserOruzjeScript;
    [SerializeField] SashaController sashaControllerRef;
    [SerializeField] SashaInventory sashaInventory;
    [SerializeField] Animator tijeloAnimator;

    [Header("Trake Oružja (Cijeli Paneli)")]
    [Tooltip("Uvuci cijeli UI objekt/traku sa 4 oružja (Normalni mod)")]
    public GameObject trakaCetiriOruzja;

    [Tooltip("Uvuci cijeli UI objekt/traku sa 2 oružja (Sasha + Miranda mod)")]
    public GameObject trakaDvaOruzja;

    [Header("UI Ikone - 4 Oružja (Normalni mod)")]
    public Color fadedColor = new Color(25f, 25f, 25f, 0.05f);
    public Color fade = new Color(5f, 5f, 5f, 0.25f);
    [SerializeField] private Image meleeIcon;
    [SerializeField] private Image gunIcon;
    [SerializeField] private Image minigunIcon;
    [SerializeField] private Image pajserIcon;
    private bool hadGun;
    private bool hadMinigun;
    private bool hadPajser;

    [Header("S/M Mod: Posebni 2-Slot UI")]
    [Tooltip("Uvuci ikonu Melee iz novog 2-slot panela")]
    [SerializeField] private Image smMeleeIcon;
    [Tooltip("Uvuci ikonu Minigun iz novog 2-slot panela")]
    [SerializeField] private Image smMinigunIcon;

    private bool isSashaMirandaMode = false;

    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager referenca nije postavljena u Inspectoru za SustavOruzja! Molimo povucite GameManager objekt u 'Game Manager' polje.");
            enabled = false;
            return;
        }

        if (sashaAudio == null) sashaAudio = GetComponentInParent<SashaAudio>();

        lastActiveWeaponForSasha = ActiveWp.Melee;

        if (gameManager.currChar == GameManager.ActiveCharacter.Sasha)
        {
            Switch(lastActiveWeaponForSasha);
        }
        else
        {
            tijeloAnimator.SetBool("minigun", false);
            tijeloAnimator.SetBool("pajser", false);
            tijeloAnimator.SetBool("udarac", false);
            pajserOruzjeScript.enabled = false;
            meleeOruzjeScript.enabled = false;
            melee.SetActive(false);
            gun.SetActive(false);
            minigun.SetActive(false);
            pajser.SetActive(false);
        }

        if (sashaInventory != null)
        {
            hadGun = sashaInventory.imaGun;
            hadMinigun = sashaInventory.imaMinigun;
            hadPajser = sashaInventory.imaPajser;
        }


        lastActiveWeaponForSasha = ActiveWp.Melee;
        Switch(ActiveWp.Melee);

        if (GameModeConfigurator.Instance != null)
        {
            isSashaMirandaMode = (GameModeConfigurator.Instance.activeMode == GameModeConfigurator.GameMode.SashaAndMiranda);
        }
        else if (gameManager != null)
        {
            isSashaMirandaMode = (gameManager.sashaOdabran && gameManager.mirandaOdabrana && !gameManager.giovanniOdabran);
        }
        if (GameModeConfigurator.Instance != null)
        {
            isSashaMirandaMode = (GameModeConfigurator.Instance.activeMode == GameModeConfigurator.GameMode.SashaAndMiranda);
        }
        else if (gameManager != null)
        {
            isSashaMirandaMode = (gameManager.sashaOdabran && gameManager.mirandaOdabrana && !gameManager.giovanniOdabran);
        }

        // A) Ako je S/M mod -> Gasi traku od 4, pali traku od 2!
        if (isSashaMirandaMode)
        {
            if (trakaCetiriOruzja != null) trakaCetiriOruzja.SetActive(false);
            if (trakaDvaOruzja != null) trakaDvaOruzja.SetActive(true);
        }
        // B) Ako je bilo koji drugi mod -> Pali traku od 4, gasi traku od 2!
        else
        {
            if (trakaCetiriOruzja != null) trakaCetiriOruzja.SetActive(true);
            if (trakaDvaOruzja != null) trakaDvaOruzja.SetActive(false);
        }

        UpdateWeaponIconsUI();
    }

    void Update()
    {
        if (sashaInventory != null)
        {
            if (sashaInventory.imaGun)
            {
                if (!hadGun)
                {
                    hadGun = true;
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    if (pajserOruzjeScript != null) pajserOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Gun);
                    if (sashaAudio != null) sashaAudio.PlayWeaponSwitch(1);
                }
            }
            else
            {
                hadGun = false;
            }

            if (sashaInventory.imaMinigun)
            {
                if (!hadMinigun)
                {
                    hadMinigun = true;
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    if (pajserOruzjeScript != null) pajserOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Minigun);
                    if (sashaAudio != null) sashaAudio.PlayWeaponSwitch(2);
                }
            }
            else
            {
                hadMinigun = false;
            }

            if (sashaInventory.imaPajser)
            {
                if (!hadPajser)
                {
                    hadPajser = true;
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    if (pajserOruzjeScript != null) pajserOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Pajser);
                    if (sashaAudio != null) sashaAudio.PlayWeaponSwitch(3);
                }
            }
            else
            {
                hadPajser = false;
            }
        }

        UpdateWeaponIconsUI();

        if (sashaControllerRef.currentState == SashaController.SashaState.Interactive || sashaControllerRef.currentState == SashaController.SashaState.Pushing)
        {
            if (melee.activeSelf) lastActiveWeaponForSasha = ActiveWp.Melee;
            else if (gun.activeSelf) lastActiveWeaponForSasha = ActiveWp.Gun;
            else if (minigun.activeSelf) lastActiveWeaponForSasha = ActiveWp.Minigun;
            else if (pajser.activeSelf) lastActiveWeaponForSasha = ActiveWp.Pajser;

            if (melee.activeSelf || gun.activeSelf || minigun.activeSelf || pajser.activeSelf)
            {
                tijeloAnimator.SetBool("minigun", false);
                tijeloAnimator.SetBool("pajser", false);
                tijeloAnimator.SetBool("udarac", false);
                pajserOruzjeScript.enabled = false;
                meleeOruzjeScript.enabled = false;
                melee.SetActive(false);
                gun.SetActive(false);
                minigun.SetActive(false);
                pajser.SetActive(false);
            }
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager referenca je izgubljena za SustavOruzja! Onemogućujem skriptu.");
            enabled = false;
            return;
        }

        if (gameManager.currChar != GameManager.ActiveCharacter.Sasha && gameManager.isLoading)
        {
            if (melee.activeSelf) lastActiveWeaponForSasha = ActiveWp.Melee;
            else if (gun.activeSelf) lastActiveWeaponForSasha = ActiveWp.Gun;
            else if (minigun.activeSelf) lastActiveWeaponForSasha = ActiveWp.Minigun;
            else if (pajser.activeSelf) lastActiveWeaponForSasha = ActiveWp.Pajser;

            if (melee.activeSelf || gun.activeSelf || minigun.activeSelf || pajser.activeSelf)
            {
                melee.SetActive(false);
                gun.SetActive(false);
                minigun.SetActive(false);
                pajser.SetActive(false);
                MinigunUI.SetActive(false);
                AmmoUI.SetActive(false);
                JacinaUdarcaUI.SetActive(false);
            }
            return;
        }

        bool imaAktivnoOruzje = melee.activeSelf || gun.activeSelf || minigun.activeSelf || (current == ActiveWp.Pajser && pajserOruzjeScript != null && pajserOruzjeScript.enabled);

        if (!imaAktivnoOruzje)
        {
            if (lastActiveWeaponForSasha == ActiveWp.Gun && sashaInventory != null && !sashaInventory.imaGun)
            {
                lastActiveWeaponForSasha = ActiveWp.Melee;
            }
            else if (lastActiveWeaponForSasha == ActiveWp.Minigun && sashaInventory != null && !sashaInventory.imaMinigun)
            {
                lastActiveWeaponForSasha = ActiveWp.Melee;
            }
            else if (lastActiveWeaponForSasha == ActiveWp.Pajser && sashaInventory != null && !sashaInventory.imaPajser)
            {
                lastActiveWeaponForSasha = ActiveWp.Melee;
            }

            if (lastActiveWeaponForSasha == 0)
            {
                lastActiveWeaponForSasha = ActiveWp.Melee;
            }

            Switch(lastActiveWeaponForSasha);
        }

        // =========================================================================
        // NOVO: KONTROLE TIPKI OVISNO O TOME JESMO LI U S/M MODU
        // =========================================================================
        if (isSashaMirandaMode)
        {
            // --- S/M MOD: SAMO 2 ORUŽJA ---
            if (Input.GetKeyDown("1"))
            {
                if (current != ActiveWp.Melee && sashaAudio != null) sashaAudio.PlayWeaponSwitch(0);
                Switch(ActiveWp.Melee);
            }
            else if (Input.GetKeyDown("2"))
            {
                if (sashaInventory != null && sashaInventory.imaMinigun)
                {
                    if (current != ActiveWp.Minigun && sashaAudio != null) sashaAudio.PlayWeaponSwitch(2);
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Minigun);
                }
                else
                {
                    Debug.Log("Sasha nema Minigun u inventaru, ne može prebaciti oružje!");
                }
            }
            // Tipke 3 i 4 su potpuno ugašene u ovom modu!
        }
        else
        {
            // --- STANDARDNI MOD: SVA 4 ORUŽJA ---
            if (Input.GetKeyDown("1"))
            {
                if (current != ActiveWp.Melee && sashaAudio != null) sashaAudio.PlayWeaponSwitch(0);
                Switch(ActiveWp.Melee);
            }
            else if (Input.GetKeyDown("2"))
            {
                if (sashaInventory != null && sashaInventory.imaGun)
                {
                    if (current != ActiveWp.Gun && sashaAudio != null) sashaAudio.PlayWeaponSwitch(1);
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Gun);
                }
                else
                {
                    Debug.Log("Sasha nema Gun u inventaru, ne može prebaciti oružje!");
                }
            }
            else if (Input.GetKeyDown("3"))
            {
                if (sashaInventory != null && sashaInventory.imaMinigun)
                {
                    if (current != ActiveWp.Minigun && sashaAudio != null) sashaAudio.PlayWeaponSwitch(2);
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Minigun);
                }
                else
                {
                    Debug.Log("Sasha nema Minigun u inventaru, ne može prebaciti oružje!");
                }
            }
            else if (Input.GetKeyDown("4"))
            {
                if (sashaInventory != null && sashaInventory.imaPajser)
                {
                    if (current != ActiveWp.Pajser && sashaAudio != null) sashaAudio.PlayWeaponSwitch(3);
                    if (meleeOruzjeScript != null) meleeOruzjeScript.PrisilnoPrekiniNapad();
                    Switch(ActiveWp.Pajser);
                }
                else
                {
                    Debug.Log("Sasha nema Pajser u inventaru, ne može prebaciti oružje!");
                }
            }
        }

        // Desni klik / Specijalne akcije
        if (current == ActiveWp.Gun && Input.GetMouseButtonDown(1))
        {
            Gun gunScript = gun.GetComponent<Gun>();
        }
        else if (current == ActiveWp.Minigun && Input.GetMouseButtonDown(1))
        {
            Minigun minigunScript = minigun.GetComponent<Minigun>();
        }
        else if (current == ActiveWp.Pajser && Input.GetMouseButtonDown(1))
        {
            Pajser pajserScript = pajser.GetComponent<Pajser>();
        }
    }

    void Switch(ActiveWp newWp)
        {
            current = newWp;

            if (meleeOruzjeScript != null)
            {
                meleeOruzjeScript.PrisilnoPrekiniNapad();
            }

        if (pajserOruzjeScript != null)
        {
            pajserOruzjeScript.PrisilnoPrekiniNapad();
        }

        if (current == ActiveWp.Melee)
            {
            meleeOruzjeScript.enabled = true;
            pajserOruzjeScript.enabled = false;
            tijeloAnimator.SetBool("minigun", false);
            tijeloAnimator.SetBool("pajser", false);
            tijeloAnimator.SetBool("udarac", false);
            if (tijeloAnimator != null) tijeloAnimator.Play("Idle", 0);
            MinigunUI.SetActive(false);
                gun.SetActive(false);
            pajser.SetActive(false);
            AmmoUI.SetActive(false);
                melee.SetActive(true);
                JacinaUdarcaUI.SetActive(true);
                minigun.SetActive(false);
        }
            else if (current == ActiveWp.Gun)
            {
            meleeOruzjeScript.enabled = false;
            pajserOruzjeScript.enabled = false;
            tijeloAnimator.SetBool("minigun", false);
            tijeloAnimator.SetBool("pajser", false);
            tijeloAnimator.SetBool("udarac", false);
            if (tijeloAnimator != null) tijeloAnimator.Play("Idle", 0);
                tijeloAnimator.Play("Idle", 0);
                MinigunUI.SetActive(false);
            pajser.SetActive(false);
            gun.SetActive(true);
                AmmoUI.SetActive(true);
                melee.SetActive(false);
                JacinaUdarcaUI.SetActive(false);
                minigun.SetActive(false);
            }
            else if (current == ActiveWp.Minigun)
            {
            meleeOruzjeScript.enabled = false;
            pajserOruzjeScript.enabled = false;
            tijeloAnimator.SetBool("pajser", false);
            tijeloAnimator.SetBool("udarac", false);
            if (tijeloAnimator != null) tijeloAnimator.Play("Idle", 0);
                tijeloAnimator.SetBool("minigun", true);
            MinigunUI.SetActive(true);
            pajser.SetActive(false);
            gun.SetActive(false);
                AmmoUI.SetActive(false);
                melee.SetActive(false);
                JacinaUdarcaUI.SetActive(false);
                minigun.SetActive(true);
            }
        else if (current == ActiveWp.Pajser)
        {
            meleeOruzjeScript.enabled = false;
            pajserOruzjeScript.enabled = true;
            tijeloAnimator.SetBool("minigun", false);
            if (tijeloAnimator != null) tijeloAnimator.Play("Pajser", 0);
            tijeloAnimator.SetBool("pajser", true);
            MinigunUI.SetActive(false);
            pajser.SetActive(true);
            gun.SetActive(false);
            AmmoUI.SetActive(false);
            melee.SetActive(false);
            JacinaUdarcaUI.SetActive(false);
            minigun.SetActive(false);
        }
        lastActiveWeaponForSasha = newWp;
        }

    void UpdateWeaponIconsUI()
    {
        if (sashaInventory == null) return;

        // =========================================================
        // AKO JE S/M MOD -> OSVJEŽAVAMO SAMO NOVE 2 IKONE!
        // =========================================================
        if (isSashaMirandaMode)
        {
            if (smMeleeIcon != null)
            {
                smMeleeIcon.color = (current == ActiveWp.Melee) ? Color.white : fadedColor;
            }

            if (smMinigunIcon != null)
            {
                if (!sashaInventory.imaMinigun) smMinigunIcon.color = fade;
                else smMinigunIcon.color = (current == ActiveWp.Minigun) ? Color.white : fadedColor;
            }
            return; // Prekidamo ovdje da ne dira stare 4 ikone!
        }

        // =========================================================
        // STANDARDNI MOD: OSVJEŽAVA ORIGINALNE 4 IKONE
        // =========================================================
        if (meleeIcon != null)
        {
            meleeIcon.color = (current == ActiveWp.Melee) ? Color.white : fadedColor;
        }

        if (gunIcon != null)
        {
            if (!sashaInventory.imaGun) gunIcon.color = fade;
            else gunIcon.color = (current == ActiveWp.Gun) ? Color.white : fadedColor;
        }

        if (minigunIcon != null)
        {
            if (!sashaInventory.imaMinigun) minigunIcon.color = fade;
            else minigunIcon.color = (current == ActiveWp.Minigun) ? Color.white : fadedColor;
        }

        if (pajserIcon != null)
        {
            if (!sashaInventory.imaPajser) pajserIcon.enabled = false;
            else
            {
                pajserIcon.enabled = true;
                pajserIcon.color = (current == ActiveWp.Pajser) ? Color.white : fadedColor;
            }
        }
    }
}