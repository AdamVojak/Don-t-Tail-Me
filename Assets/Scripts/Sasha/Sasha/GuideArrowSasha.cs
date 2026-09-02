using UnityEngine;

public class GuideArrowSasha : MonoBehaviour
{
    [Header("Postavke")]
    [Tooltip("Stavi -90 ako ti je sličica crtana da gleda prema GORE, stavi 0 ako gleda uDESNO")]
    [SerializeField] private float offsetKuta = -90f;
    [SerializeField] private float defaultDometNestajanja = 2.5f;

    [Header("Jitter / Pulsiranje prema meti")]
    [SerializeField] private float jitterBrzina = 8f;       // Brzina pulsiranja
    [SerializeField] private float jitterAmplituda = 0.35f;  // Koliko se daleko pruža prema cilju

    private SpriteRenderer spriteRenderer;
    private Vector3 pocetnaLokalnaPozicija;

    private Vector3 trenutnaMeta;
    private float trenutniDomet;
    private bool isGuiding = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pocetnaLokalnaPozicija = transform.localPosition;

        SakrijStrelicu(); // Na početku je ugašena
    }

    private void LateUpdate()
    {
        if (!isGuiding || spriteRenderer == null || !spriteRenderer.enabled) return;

        // 1. Provjera udaljenosti do cilja (X-Y ravnina za Sashu)
        float udaljenost = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(trenutnaMeta.x, trenutnaMeta.y)
        );

        if (udaljenost <= trenutniDomet)
        {
            SakrijStrelicu(); // Stigao do cilja -> gasi se
            return;
        }

        // 2. Bazna pozicija iznad glave u prostoru (neovisna o rotaciji tijela)
        Vector3 baznaPozicija = (transform.parent != null)
            ? transform.parent.TransformPoint(pocetnaLokalnaPozicija)
            : transform.position;

        // 3. Točan smjer prema meti
        Vector3 smjerPremaMeti = (trenutnaMeta - baznaPozicija).normalized;
        smjerPremaMeti.z = 0f;

        // 4. Rotacija oko Z-osi točno prema meti
        float kut = Mathf.Atan2(smjerPremaMeti.y, smjerPremaMeti.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, kut + offsetKuta);

        // 5. PULSIRANJE ISKLJUČIVO PREMA METI:
        // Ide od 0 (baza) do 'jitterAmplituda' (prema meti) i natrag, nikada ne ide unatrag!
        float pomakPremaNaprijed = (Mathf.Sin(Time.time * jitterBrzina) * 0.5f + 0.5f) * jitterAmplituda;

        // Postavljanje pozicije u smjeru cilja
        transform.position = baznaPozicija + (smjerPremaMeti * pomakPremaNaprijed);
    }

    // =========================================================================
    // JAVNE METODE
    // =========================================================================

    public void PostaviCilj(Vector3 metaPozicija, float domet = -1f)
    {
        trenutnaMeta = metaPozicija;
        trenutniDomet = (domet > 0) ? domet : defaultDometNestajanja;
        isGuiding = true;

        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void PostaviCilj(Transform metaTransform, float domet = -1f)
    {
        if (metaTransform != null)
        {
            PostaviCilj(metaTransform.position, domet);
        }
    }

    public void SakrijStrelicu()
    {
        isGuiding = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        transform.localPosition = pocetnaLokalnaPozicija;
    }
}