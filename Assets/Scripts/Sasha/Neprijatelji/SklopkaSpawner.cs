using Unity.Properties;
using UnityEngine;

public class SklopkaSpawner : MonoBehaviour
{
    [Header("Računalo")]
    public GameObject racunalo;
    private SpriteRenderer racunaloSprite;
    private Animator racunaloAnimator;
    public Sprite racunaloON;
    public Sprite racunaloOFF;

    public Light svijetloRacunala;

    [Header("Povezani Spawner")]
    public Spawner targetSpawner;
    public GameObject svijetlo;

    [Header("Vizualne Postavke (Sprites)")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteOn;
    public Sprite spriteOff;
    public GameObject lampica;

    [Header("Postavke Triggera")]
    public string meleeTag = "Melee";
    private Color crvena = new Color(255, 182, 182, 255);
    private Color zelena = new Color(182, 255, 182, 255);

    [HideInInspector] public bool isOn = false;

    private int prvi;

    private void Start()
    {
        racunaloAnimator = racunalo.GetComponent<Animator>();
        racunaloSprite = racunalo.GetComponent<SpriteRenderer>();
        prvi = 0;
        svijetloRacunala.color = zelena;
    }

    public void SetState(bool state)
    {
        isOn = state;

        if (isOn)
        {
            if (prvi == 0)
            {
                prvi++;
                lampica.SetActive(true);
            }
            else
            {
                lampica.SetActive(false);
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOn ? spriteOn : spriteOff;
        }

        if (targetSpawner != null)
        {
            targetSpawner.SetActiveState(isOn);
        }

        if (targetSpawner != null)
        {
            targetSpawner.isActive = isOn;
        }

        svijetlo.SetActive(isOn ? true : false);

        if (isOn==true)
            {
                racunaloAnimator.enabled = isOn;
                racunaloAnimator.Play("Idle", 0);
                svijetloRacunala.color = zelena;
            }
            else
            {
                racunaloAnimator.enabled = isOn;
                racunaloSprite.sprite = racunaloOFF;
                svijetloRacunala.enabled = isOn;
                svijetloRacunala.color = crvena;
            }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Reagira na udarac samo ako je skripta omogućena
        if (other.CompareTag(meleeTag) || other.CompareTag("Pajser"))
        {
            // Preklapa stanje (ako je bilo ON -> postaje OFF, prvi udarac gasi zamku!)
            SetState(!isOn);
        }
    }
}