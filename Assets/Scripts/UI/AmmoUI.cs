using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [Header("Spriteovi za municiju")]
    [SerializeField] private Sprite emptyCanteen;
    [SerializeField] private Sprite oneBulletCanteen;
    [SerializeField] private Sprite twoBulletsCanteen;
    [SerializeField] private Sprite halfFullCanteen;
    [SerializeField] private Sprite fullCanteen;

    [Header("Reference")]
    [SerializeField] private Gun playerGun;

    private Image canteenImage;

    void Start()
    {
        canteenImage = GetComponent<Image>();
        if (canteenImage == null)
        {
            Debug.LogError("Nema Image komponente na ovom GameObjectu! Molimo dodajte Image komponentu na UI element.");
            enabled = false;
            return;
        }

        if (playerGun == null)
        {
            Debug.LogError("Referenca na Gun skriptu nije postavljena u Inspectoru! Molimo povucite Gun objekt u 'Player Gun' polje.");
            enabled = false;
            return;
        }

        UpdateAmmoUI();
    }

    void Update()
    {
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (playerGun == null || canteenImage == null)
        {
            return;
        }

        int currentAmmo = playerGun.ammo;

        if (currentAmmo == 0)
        {
            canteenImage.sprite = emptyCanteen;
        }
        else if (currentAmmo == 1)
        {
            canteenImage.sprite = oneBulletCanteen;
        }
        else if (currentAmmo == 2)
        {
            canteenImage.sprite = twoBulletsCanteen;
        }
        else if (currentAmmo >= 3 && currentAmmo <= 4)
        {
            canteenImage.sprite = halfFullCanteen;
        }
        else if (currentAmmo >= 5)
        {
            canteenImage.sprite = fullCanteen;
        }
    }
}