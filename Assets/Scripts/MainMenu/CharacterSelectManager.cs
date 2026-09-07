using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class TeamPreset
    {
        public string teamTitle;
        public string teamDescription;
        public bool isSashaActive;
        public bool isMirandaActive;
        public bool isGiovanniActive;
    }

    [System.Serializable]
    public class CharacterCardUI
    {
        public string characterName;
        public Image portraitImage;
        public Image statusIcon; // Kvačica ili Križić
        public CanvasGroup cardCanvasGroup;
    }

    [Header("UI Elementi")]
    [SerializeField] private TextMeshProUGUI textTeamTitle;
    [SerializeField] private TextMeshProUGUI textTeamDescription;

    [Header("Kartice Likova (Redoslijed: Sasha, Miranda, Giovanni)")]
    [SerializeField] private CharacterCardUI cardSasha;
    [SerializeField] private CharacterCardUI cardMiranda;
    [SerializeField] private CharacterCardUI cardGiovanni;

    [Header("Scena za Učitavanje")]
    [SerializeField] private string gameplaySceneName = "DemoLevel";

    [Header("Reference za Blokadu")]
    [SerializeField] private SubmarineDeckNavigator deckNavigator;
    [SerializeField] private int crewDeckIndex = 1;
    [SerializeField] private ExitHatchController exitController;

    [Header("Ikone")]
    [SerializeField] private Sprite checkmarkSprite; // Kvačica
    [SerializeField] private Sprite crossSprite;     // Križić

    [Header("Definirane Postave (4 Opcije)")]
    [SerializeField]
    private TeamPreset[] presets = new TeamPreset[4]
    {
        new TeamPreset { teamTitle = "SASHA & MIRANDA", teamDescription = "Brain and Brawl", isSashaActive = true, isMirandaActive = true, isGiovanniActive = false },
        new TeamPreset { teamTitle = "MIRANDA & GIOVANNI", teamDescription = "Silent, but innocent", isSashaActive = false, isMirandaActive = true, isGiovanniActive = true },
        new TeamPreset { teamTitle = "SASHA & GIOVANNI", teamDescription = "Clown and a Bat", isSashaActive = true, isMirandaActive = false, isGiovanniActive = true },
        new TeamPreset { teamTitle = "ALL INCLUDED", teamDescription = "Real \"Don't Tail Me\"", isSashaActive = true, isMirandaActive = true, isGiovanniActive = true }
    };

    private void Update()
    {
        // 1. Ako je otvoren Exit prozor -> BLOKIRAJ sve ovdje!
        if (exitController != null && exitController.IsDialogOpen)
            return;

        // 2. Ako igrač NIJE na katu odabira likova -> BLOKIRAJ sve ovdje!
        if (deckNavigator != null && deckNavigator.CurrentDeckIndex != crewDeckIndex)
            return;

        // 3. Ako se palube trenutno kreću -> BLOKIRAJ!
        if (deckNavigator != null && deckNavigator.IsSliding)
            return;

        // --- SAMO I ISKLJUČIVO NA KATU 1 DOK NEMA DIJALOGA: ---
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPreset();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPreset();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            StartMission();
        }
    }

    private int currentPresetIndex = 3; // Početno na Punu Posadu (indeks 3)

    private void Start()
    {
        UpdateUI();
    }

    public void NextPreset()
    {
        currentPresetIndex = (currentPresetIndex + 1) % presets.Length;
        UpdateUI();
    }

    public void PreviousPreset()
    {
        currentPresetIndex--;
        if (currentPresetIndex < 0) currentPresetIndex = presets.Length - 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        TeamPreset current = presets[currentPresetIndex];

        if (textTeamTitle) textTeamTitle.text = current.teamTitle;
        if (textTeamDescription) textTeamDescription.text = current.teamDescription;

        UpdateCard(cardSasha, current.isSashaActive);
        UpdateCard(cardMiranda, current.isMirandaActive);
        UpdateCard(cardGiovanni, current.isGiovanniActive);
    }

    private void UpdateCard(CharacterCardUI card, bool isSelected)
    {
        if (card == null) return;

        if (card.statusIcon != null)
            card.statusIcon.sprite = isSelected ? checkmarkSprite : crossSprite;

        if (card.cardCanvasGroup != null)
            card.cardCanvasGroup.alpha = isSelected ? 1f : 0.35f;
    }

    // Poziva se na klik gumba "START"
    public void StartMission()
    {
        TeamPreset current = presets[currentPresetIndex];

        // Spremamo odabir u naš statički most
        CharacterSelectionData.SashaSelected = current.isSashaActive;
        CharacterSelectionData.MirandaSelected = current.isMirandaActive;
        CharacterSelectionData.GiovanniSelected = current.isGiovanniActive;

        Debug.Log($"Pokrećem igru! Sasha: {CharacterSelectionData.SashaSelected}, Miranda: {CharacterSelectionData.MirandaSelected}, Giovanni: {CharacterSelectionData.GiovanniSelected}");

        SceneManager.LoadScene(gameplaySceneName);
    }
}