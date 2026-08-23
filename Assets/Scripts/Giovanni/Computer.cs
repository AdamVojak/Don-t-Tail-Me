using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Computer : MonoBehaviour
{
    [Header("Animacija Tuba")]
    [SerializeField] private Ventilacija_In_Giovanni tube_In;

    [Header("Reference")]
    [SerializeField] private Light screenLight;
    [SerializeField] private GiovanniController player;
    private bool isFlashing = false;

    [Header("Stanje - Ugašen")]
    public bool isReadyToSend = false;
    [SerializeField] private float targetIntensity = 1.75f;
    [SerializeField] private float fadeDuration = 1.5f;
    private float timeAtClosed;

    [Header("Stanje - Upaljen")]
    [SerializeField] private GameObject computerUI;
    private bool isUIOpen = false;

    [Header("UI Slanje")]
    [SerializeField] private Image[] itemImages;
    [SerializeField] private GameObject[] selectionFrames;
    private int selectedIndex = 0;

    [Header("Inventar")]
    [SerializeField] private GiovanniInventory inventory;

    [Header("Poveznica s Ventilacijom")]
    [SerializeField] private Ventilacija_Out_Miranda mirandinaVentilacija;

    [Header("Poveznica sa Sashom (Ako Miranda nije tu)")]
    [SerializeField] private Ventilacija_Out_Sasha sashaVentilacija;

    public bool IsUIOpen => isUIOpen;

    void Start()
    {
        if (screenLight != null)
        {
            screenLight.enabled = false;
        }
        if (computerUI != null) 
        {
            computerUI.SetActive(false);
        }
    }

    private IEnumerator FlashUIRoutine()
    {
        if (isFlashing) yield break;
        isFlashing = true;

        for (int f = 0; f < 4; f++)
        {
            for (int i = 0; i < itemImages.Length; i++)
            {
                itemImages[i].color = Color.black;
                if (selectionFrames.Length > i && selectionFrames[i] != null)
                {
                    Image frameImg = selectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.red;
                    selectionFrames[i].SetActive(true);
                }
            }
            yield return new WaitForSeconds(0.25f);

            for (int i = 0; i < itemImages.Length; i++)
            {
                itemImages[i].color = Color.white;
                if (selectionFrames.Length > i && selectionFrames[i] != null)
                {
                    Image frameImg = selectionFrames[i].GetComponent<Image>();
                    if (frameImg != null) frameImg.color = Color.white;
                }
            }
            yield return new WaitForSeconds(0.25f);
        }

        isFlashing = false;
        UpdateUI();
    }

    public bool IsInteracting()
    {
        return isUIOpen || (Time.time - timeAtClosed < 0.2f);
    }

    void Update()
    {
        if (!isUIOpen) return;

        HandleNavigation();

        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleComputerState(false);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            TrySendSelectedItem();
        }
    }

    private void HandleNavigation()
    {
        int prevIndex = selectedIndex;
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll < 0f || Input.GetKeyDown(KeyCode.E)) selectedIndex++;
        else if (scroll > 0f || Input.GetKeyDown(KeyCode.Q)) selectedIndex--;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, 2);

        if (prevIndex != selectedIndex) UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 3; i++)
        {
            // NOVO MAPIRANJE: 0 = Ruka (ID 3), 1 = Pajser (ID 5), 2 = Minigun (ID 2)
            int checkID = (i == 0) ? 3 : (i == 1 ? 5 : 2);

            if (inventory != null)
            {
                itemImages[i].color = inventory.HasItem(checkID) ? Color.white : Color.black;
            }
            selectionFrames[i].SetActive(i == selectedIndex);
        }
    }

    public void TrySendSelectedItem()
    {
        // NOVO MAPIRANJE: 0 = Ruka (ID 3), 1 = Pajser (ID 5), 2 = Minigun (ID 2)
        int itemIDToSend = (selectedIndex == 0) ? 3 : (selectedIndex == 1 ? 5 : 2);

        // 1. Provjeri ima li Giovanni taj item
        if (!inventory.HasItem(itemIDToSend))
        {
            Debug.Log("Giovanni nema odabrani predmet!");
            return;
        }

        // 2. Provjera tko je u igri preko GameManager-a
        bool mirandaU_Igri = GameManager.Instance != null && GameManager.Instance.mirandaOdabrana;
        bool sashaU_Igri = GameManager.Instance != null && GameManager.Instance.sashaOdabran;

        // --- SCENARIJ A: ŠALJEMO MIRANDI ---
        if (mirandaU_Igri && mirandinaVentilacija != null)
        {
            if (!mirandinaVentilacija.MozePrimiti())
            {
                StartCoroutine(FlashUIRoutine());
                Debug.LogWarning("Mirandina cijev je puna! Čeka se da pokupi item.");
                return;
            }

            if (tube_In != null) tube_In.PokreniAnimacijuSlanja(itemIDToSend);

            mirandinaVentilacija.SpremiItemZaMirandu(itemIDToSend);
            Debug.Log("Giovanni je poslao predmet ID: " + itemIDToSend + " Mirandi.");
        }
        // --- SCENARIJ B: ŠALJEMO SASHI (Fallback) ---
        else if (sashaU_Igri && sashaVentilacija != null)
        {
            if (!sashaVentilacija.MozePrimiti())
            {
                StartCoroutine(FlashUIRoutine());
                Debug.LogWarning("Sashina cijev je puna! Čeka se da pokupi item.");
                return;
            }

            if (tube_In != null) tube_In.PokreniAnimacijuSlanja(itemIDToSend);

            sashaVentilacija.SpremiItemZaSashu(itemIDToSend);
            Debug.Log("Mirande nema u igri. Giovanni šalje predmet ID: " + itemIDToSend + " direktno Sashi!");
        }
        else
        {
            StartCoroutine(FlashUIRoutine());
            Debug.LogError("Nema dostupnih likova za primanje Giovannijevog itema!");
            return;
        }

        inventory.RemoveItem(itemIDToSend);
        UpdateUI();
        ToggleComputerState(false);
    }

    public void ToggleComputerState(bool state)
    {
        isUIOpen = state;
        if (state)
        {
            computerUI.SetActive(true);
            UpdateUI();
            if (player != null) player.SetLock(true);
        }
        else
        {
            timeAtClosed = Time.time;
            StartCoroutine(FadeOutLight());
            if (player != null) player.SetLock(false);
        }
    }

    public void TurnOn()
    {
        if (!isReadyToSend)
        {
            isReadyToSend = true;
            StartCoroutine(FadeInLight());
            Debug.Log("Kompjuter se pali...");
        }
    }

    private IEnumerator FadeInLight()
    {
        if (screenLight == null) yield break;

        screenLight.intensity = 0;
        screenLight.enabled = true;

        float currentTime = 0;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            screenLight.intensity = Mathf.Lerp(0, targetIntensity, currentTime / fadeDuration);
            yield return null;
        }
        screenLight.intensity = targetIntensity;
        if (computerUI != null)
        {
            computerUI.SetActive(true);
            ToggleComputerState(true);
                //neki zvuk paljenja
        }
    }

    private IEnumerator FadeOutLight()
    {
        float currentTime = 0;
        float startIntensity = screenLight.intensity;

        if (computerUI != null) computerUI.SetActive(false);

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            screenLight.intensity = Mathf.Lerp(startIntensity, 0, currentTime / fadeDuration);
            yield return null;
        }

        screenLight.enabled = false;
        isReadyToSend = false;
    }
}