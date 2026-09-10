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
    [SerializeField] private GameObject computerUI; // Glavni roditelj UI-ja
    private bool isUIOpen = false;

    // =========================================================
    // NOVO: CENTRALNA BAZA SVIH PREDMETA
    // =========================================================
    [System.Serializable]
    public struct ItemBaza
    {
        public string naziv;
        public int itemID;
        public Sprite itemSprite;
    }

    [Header("Baza Svih Predmeta")]
    public ItemBaza[] sviPredmeti;

    [Header("UI Paneli (Ovisno o broju itema)")]
    [Tooltip("Panel koji ima 3 slota (Za modove Sva 3 lika i S/G)")]
    [SerializeField] private GameObject panel3Slota;
    [Tooltip("Panel koji ima 2 slota (Za mod G/M)")]
    [SerializeField] private GameObject panel2Slota;

    [Header("UI Elementi (Uvuci iz OBA panela)")]
    [Tooltip("Uvuci svih 5 Image komponenti (3 iz prvog panela, 2 iz drugog)")]
    [SerializeField] private Image[] itemImages;
    [Tooltip("Uvuci svih 5 okvira za selekciju")]
    [SerializeField] private GameObject[] selectionFrames;

    private int selectedIndex = 0;

    // Dinamične varijable koje skripta sama postavlja
    private int[] currentSlotIDs;
    private int brojAktivnihSlotova = 3;
    private int offsetSlika = 0; // 0 za panel s 3 slota, 3 za panel s 2 slota

    [Header("Inventar")]
    [SerializeField] private GiovanniInventory inventory;

    [Header("Poveznica s Ventilacijom")]
    [SerializeField] private Ventilacija_Out_Miranda mirandinaVentilacija;

    [Header("Poveznica sa Sashom (Ako Miranda nije tu)")]
    [SerializeField] private Ventilacija_Out_Sasha sashaVentilacija;

    [Header("Poveznica s Mobitelom")]
    [SerializeField] private MobitelTracker mobitelTracker;

    [Header("Audio")]
    [SerializeField] private ComputerAudio computerAudio;

    public bool IsUIOpen => isUIOpen;

    void Start()
    {
        if (computerAudio == null) computerAudio = GetComponent<ComputerAudio>();

        if (screenLight != null) screenLight.enabled = false;
        if (computerUI != null) computerUI.SetActive(false);

        // =========================================================
        // AUTOMATSKO PODEŠAVANJE INVENTARA PREMA GAME MODU!
        // =========================================================
        if (GameModeConfigurator.Instance != null)
        {
            var mode = GameModeConfigurator.Instance.activeMode;

            if (mode == GameModeConfigurator.GameMode.MirandaAndGiovanni)
            {
                currentSlotIDs = new int[] { 3, 6 }; // Ruka, ObicanKljuc
                brojAktivnihSlotova = 2;
                offsetSlika = 3; // Koristimo zadnje 2 slike u nizu
                if (panel3Slota != null) panel3Slota.SetActive(false);
                if (panel2Slota != null) panel2Slota.SetActive(true);
            }
            else if (mode == GameModeConfigurator.GameMode.SashaAndGiovanni)
            {
                currentSlotIDs = new int[] { 5, 6, 2 }; // Pajser, ObicanKljuc, Minigun
                brojAktivnihSlotova = 3;
                offsetSlika = 0;
                if (panel3Slota != null) panel3Slota.SetActive(true);
                if (panel2Slota != null) panel2Slota.SetActive(false);
            }
            else
            {
                // Default (Sva 3 lika)
                currentSlotIDs = new int[] { 3, 1, 2 }; // Ruka, Gun, Minigun
                brojAktivnihSlotova = 3;
                offsetSlika = 0;
                if (panel3Slota != null) panel3Slota.SetActive(true);
                if (panel2Slota != null) panel2Slota.SetActive(false);
            }
        }
        else
        {
            // Fallback
            currentSlotIDs = new int[] { 3, 1, 2 };
            brojAktivnihSlotova = 3;
            offsetSlika = 0;
            if (panel3Slota != null) panel3Slota.SetActive(true);
            if (panel2Slota != null) panel2Slota.SetActive(false);
        }
    }

    private Sprite GetSpriteForID(int id)
    {
        foreach (var item in sviPredmeti)
        {
            if (item.itemID == id) return item.itemSprite;
        }
        return null;
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

        if (selectedIndex >= brojAktivnihSlotova) selectedIndex = 0;
        if (selectedIndex < 0) selectedIndex = brojAktivnihSlotova - 1;

        if (prevIndex != selectedIndex) UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < brojAktivnihSlotova; i++)
        {
            int checkID = currentSlotIDs[i];
            int uiIndex = i + offsetSlika; // Preskačemo slike iz ugašenog panela!

            if (itemImages[uiIndex] != null)
            {
                itemImages[uiIndex].sprite = GetSpriteForID(checkID);

                if (inventory != null && inventory.HasItem(checkID))
                    itemImages[uiIndex].color = Color.white;
                else
                    itemImages[uiIndex].color = Color.black;
            }

            if (selectionFrames[uiIndex] != null)
            {
                selectionFrames[uiIndex].SetActive(i == selectedIndex);
            }
        }
    }

    public void TrySendSelectedItem()
    {
        int itemIDToSend = currentSlotIDs[selectedIndex];

        if (!inventory.HasItem(itemIDToSend))
        {
            Debug.Log("Giovanni nema odabrani predmet!");
            StartCoroutine(FlashUIRoutine());
            return;
        }

        // Koristimo IsCharacterAvailable da provjerimo tko je STVARNO dostupan (nije mrtav i nije pobijedio)
        bool mirandaDostupna = GameManager.Instance != null &&
                              GameManager.Instance.IsCharacterAvailable(GameManager.ActiveCharacter.Miranda) &&
                              mirandinaVentilacija != null;

        bool sashaDostupan = GameManager.Instance != null &&
                             GameManager.Instance.IsCharacterAvailable(GameManager.ActiveCharacter.Sasha) &&
                             sashaVentilacija != null;

        bool uspjesnoPoslano = false;

        // --- SCENARIJ A: ŠALJEMO MIRANDI (Ako je dostupna) ---
        if (mirandaDostupna)
        {
            if (!mirandinaVentilacija.MozePrimiti())
            {
                Debug.LogWarning("Mirandina cijev je puna! Čeka se da pokupi item.");
            }
            else
            {
                if (tube_In != null) tube_In.PokreniAnimacijuSlanja(itemIDToSend);
                mirandinaVentilacija.SpremiItemZaMirandu(itemIDToSend);
                uspjesnoPoslano = true;
            }
        }
        // --- SCENARIJ B: ŠALJEMO SASHI (Ako Miranda više nije tu) ---
        else if (sashaDostupan)
        {
            if (!sashaVentilacija.MozePrimiti())
            {
                Debug.LogWarning("Sashina cijev je puna! Čeka se da pokupi item.");
            }
            else
            {
                if (tube_In != null) tube_In.PokreniAnimacijuSlanja(itemIDToSend);
                sashaVentilacija.SpremiItemZaSashu(itemIDToSend);
                uspjesnoPoslano = true;
            }
        }
        else
        {
            Debug.LogError("Nema dostupnih likova za primanje Giovannijevog itema!");
        }

        if (uspjesnoPoslano)
        {
            inventory.RemoveItem(itemIDToSend);
            UpdateUI();
            ToggleComputerState(false);
        }
        else
        {
            StartCoroutine(FlashUIRoutine());
        }
    }

    private IEnumerator FlashUIRoutine()
    {
        if (isFlashing) yield break;
        isFlashing = true;

        for (int f = 0; f < 4; f++)
        {
            for (int i = 0; i < brojAktivnihSlotova; i++)
            {
                int uiIndex = i + offsetSlika;
                if (itemImages[uiIndex] != null) itemImages[uiIndex].color = Color.red;
            }
            yield return new WaitForSeconds(0.25f);

            UpdateUI();
            yield return new WaitForSeconds(0.25f);
        }

        isFlashing = false;
        UpdateUI();
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

            if (mobitelTracker != null)
            {
                mobitelTracker.OnComputerUsed();
            }

            Debug.Log("Kompjuter se pali...");
        }
    }

    private IEnumerator FadeInLight()
    {
        if (screenLight == null) yield break;

        if (computerAudio != null) computerAudio.PlayStartupSequence();

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
        }
    }

    private IEnumerator FadeOutLight()
    {
        float currentTime = 0;
        float startIntensity = screenLight.intensity;

        if (computerAudio != null) computerAudio.PlayShutdown();

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