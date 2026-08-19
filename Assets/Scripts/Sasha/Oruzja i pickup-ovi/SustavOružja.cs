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
    [SerializeField] GameObject JacinaUdarcaUI;
    [SerializeField] GameObject AmmoUI;
    [SerializeField] GameObject MinigunUI;
    [SerializeField] public Melee meleeOruzjeScript;
    [SerializeField] public Pajser pajserOruzjeScript;
    [SerializeField] SashaController sashaControllerRef;
    [SerializeField] SashaInventory sashaInventory;
    [SerializeField] Animator tijeloAnimator;

    [Header("UI Ikone Oružja")]
    public Color fadedColor = new Color(25f, 25f, 25f, 0.05f);
    public Color fade= new Color(5f, 5f, 5f, 0.25f);
    [SerializeField] private Image meleeIcon;
    [SerializeField] private Image gunIcon;
    [SerializeField] private Image minigunIcon;
    [SerializeField] private Image pajserIcon;
    private bool hadGun;
    private bool hadMinigun;
    private bool hadPajser;

    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager referenca nije postavljena u Inspectoru za SustavOruzja! Molimo povucite GameManager objekt u 'Game Manager' polje.");
            enabled = false;
            return;
        }

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
                    SFX.zvucniEfekti.ZvukIzmjeneOruzja2.Play();
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
                    SFX.zvucniEfekti.ZvukIzmjeneOruzja3.Play();
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
                    //SFX.zvucniEfekti.ZvukIzmjeneOruzja4.Play();
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
            if (melee.activeSelf)
            {
                lastActiveWeaponForSasha = ActiveWp.Melee;
            }
            else if (gun.activeSelf)
            {
                lastActiveWeaponForSasha = ActiveWp.Gun;
            }
            else if (minigun.activeSelf)
            {
                lastActiveWeaponForSasha = ActiveWp.Minigun;
            }
            else if (pajser.activeSelf)
            {
                lastActiveWeaponForSasha = ActiveWp.Pajser;
            }

            if (melee.activeSelf || gun.activeSelf || minigun.activeSelf || pajser.activeSelf)
            {
                tijeloAnimator.SetBool("minigun", false);
                tijeloAnimator.SetBool("pajser", false);
                tijeloAnimator.SetBool("udarac", false);
                pajserOruzjeScript.enabled = false;
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

        if (gameManager.currChar != GameManager.ActiveCharacter.Sasha)
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
            }
            return;
        }


        bool imaAktivnoOruzje = melee.activeSelf || gun.activeSelf || minigun.activeSelf || (current == ActiveWp.Pajser && pajserOruzjeScript != null && pajserOruzjeScript.enabled);

        if (!imaAktivnoOruzje)
        {
            // Provjera ako je zapamtio oružje koje Sasha zapravo nema u inventaru:
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

            // Ako ništa nije odabrano, prisilno postavi Melee:
            if (lastActiveWeaponForSasha == 0)
            {
                lastActiveWeaponForSasha = ActiveWp.Melee;
            }

            Switch(lastActiveWeaponForSasha);
        }

        if (Input.GetKeyDown("1"))
        {
            if (current != ActiveWp.Melee) SFX.zvucniEfekti.ZvukIzmjeneOruzja1.Play();
            Switch(ActiveWp.Melee);
        }
        else if (Input.GetKeyDown("2"))
        {
            if (sashaInventory != null && sashaInventory.imaGun)
            {
                if (current != ActiveWp.Gun) SFX.zvucniEfekti.ZvukIzmjeneOruzja2.Play();
                meleeOruzjeScript.PrisilnoPrekiniNapad();
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
                if (current != ActiveWp.Minigun) SFX.zvucniEfekti.ZvukIzmjeneOruzja3.Play();
                meleeOruzjeScript.PrisilnoPrekiniNapad();
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
                //if (current != ActiveWp.Pajser) SFX.zvucniEfekti.ZvukIzmjeneOruzja4.Play();
                meleeOruzjeScript.PrisilnoPrekiniNapad();
                Switch(ActiveWp.Pajser);
            }
            else
            {
                Debug.Log("Sasha nema Pajser u inventaru, ne može prebaciti oružje!");
            }
        }

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

        if (meleeIcon != null)
        {
            if (current == ActiveWp.Melee)
            {
                meleeIcon.color = Color.white;
            }
            else
            {
                meleeIcon.color = fadedColor;
            }
        }

        if (gunIcon != null)
        {
            if (!sashaInventory.imaGun)
            {
                gunIcon.color = fade;
            }
            else
            {
                if (current == ActiveWp.Gun)
                {
                    gunIcon.color = Color.white;
                }
                else
                {
                    gunIcon.color = fadedColor;
                }
            }
        }

        if (minigunIcon != null)
        {
            if (!sashaInventory.imaMinigun)
            {
                minigunIcon.color = fade;
            }
            else
            {
                if (current == ActiveWp.Minigun)
                {
                    minigunIcon.color = Color.white;
                }
                else
                {
                    minigunIcon.color = fadedColor;
                }
            }
        }

        if (pajserIcon != null)
        {
            if (!sashaInventory.imaPajser)
            {
                pajserIcon.enabled = false;
            }
            else
            {
                pajserIcon.enabled = true;
                if (current == ActiveWp.Pajser)
                {
                    pajserIcon.color = Color.white;
                }
                else
                {
                    pajserIcon.color = fadedColor;
                }
            }
        }   

    }
}