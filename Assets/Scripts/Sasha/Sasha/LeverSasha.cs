using UnityEngine;

public class LeverSasha : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite on;
    public Sprite off;

    public bool aktiviran = false;

    [Header("Audio")]
    [SerializeField] private LeverAudio leverAudio;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (leverAudio == null) leverAudio = GetComponent<LeverAudio>();
    }

    void Start()
    {
        spriteRenderer.sprite = aktiviran ? on : off;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Melee") || other.CompareTag("Pajser"))
        {
            aktiviran = !aktiviran;
            spriteRenderer.sprite = aktiviran ? on : off;

            if (leverAudio != null) leverAudio.PlaySwitch(aktiviran);

            Debug.Log("Lever promijenjen: " + aktiviran);
        }
    }
}