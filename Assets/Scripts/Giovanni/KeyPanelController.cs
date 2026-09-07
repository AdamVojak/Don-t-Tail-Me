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

    [Header("Poveznica s Liftom")]
    public GiovanniElevatorEscape elevatorEscape; // NOVO: Uvuci skriptu lifta ovdje!

    void Start()
    {
        panelWithoutKey.SetActive(true);
        panelWithKey.SetActive(false);
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
        Debug.Log("Gumb je pritisnut! Pokrećem lift.");

        if (elevatorEscape != null)
        {
            elevatorEscape.PokreniBijegLiftom();
        }
    }
}