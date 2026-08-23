using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneli")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("Postavke Učitavanja")]
    [Tooltip("Naziv scene koja se učitava nakon klika na New Game")]
    [SerializeField] private string firstLevelSceneName = "GameScene";

    [Header("Zvukovi Menija (Opcionalno)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip buttonHoverSFX;

    private void Start()
    {
        // Prikazujemo samo glavni panel na početku
        ShowOnlyPanel(mainButtonsPanel);

        // Prikazujemo kursor (za slučaj da je bio zaključan u gameplayu)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #region Navigacija Kroz Panele

    public void OpenSettings()
    {
        PlayClickSound();
        ShowOnlyPanel(settingsPanel);
    }

    public void OpenCredits()
    {
        PlayClickSound();
        ShowOnlyPanel(creditsPanel);
    }

    public void BackToMainMenu()
    {
        PlayClickSound();
        ShowOnlyPanel(mainButtonsPanel);
    }

    public void OpenQuitConfirm()
    {
        PlayClickSound();
        quitConfirmPanel.SetActive(true);
    }

    public void CloseQuitConfirm()
    {
        PlayClickSound();
        quitConfirmPanel.SetActive(false);
    }

    private void ShowOnlyPanel(GameObject activePanel)
    {
        if (mainButtonsPanel) mainButtonsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (quitConfirmPanel) quitConfirmPanel.SetActive(false);

        if (activePanel != null)
        {
            activePanel.SetActive(true);
        }
    }

    #endregion

    #region Glavne Funkcije

    public void NewGame()
    {
        PlayClickSound();
        // Ovdje kasnije možemo dodati fade-out efekt prije učitavanja
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Gasim igru...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    #endregion

    #region Audio Pomoćne Funkcije

    public void PlayHoverSound()
    {
        if (audioSource && buttonHoverSFX)
            audioSource.PlayOneShot(buttonHoverSFX);
    }

    private void PlayClickSound()
    {
        if (audioSource && buttonClickSFX)
            audioSource.PlayOneShot(buttonClickSFX);
    }

    #endregion
}