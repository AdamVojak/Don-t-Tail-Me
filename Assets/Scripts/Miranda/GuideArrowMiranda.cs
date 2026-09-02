using UnityEngine;

public class GuideArrowMiranda : MonoBehaviour
{
    [Header("Postavke Rotacije (Oko X-osi)")]
    [Tooltip("Kutni offset oko X-osi (isprobaj 0, 90, 180 ili -90 ovisno o tome kako ti je strelica nacrtana)")]
    [SerializeField] private float offsetKuta = 0f;
    [SerializeField] private float defaultDometNestajanja = 2.0f;

    [Header("Jitter / Pulsiranje prema meti")]
    [SerializeField] private float jitterBrzina = 8f;
    [SerializeField] private float jitterAmplituda = 0.35f;

    private SpriteRenderer childSpriteRenderer;
    private Vector3 pocetnaLokalnaPozicija;

    private Vector3 trenutnaMeta;
    private float trenutniDomet;
    private bool isGuiding = false;

    private void Awake()
    {
        // Automatski pronalazi SpriteRenderer na djetetu
        childSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        pocetnaLokalnaPozicija = transform.localPosition;

        SakrijStrelicu(); // Na početku gasi sliku djeteta
    }

    private void LateUpdate()
    {
        if (!isGuiding || childSpriteRenderer == null || !childSpriteRenderer.enabled) return;

        // 1. Provjera udaljenosti (Y-Z ravnina)
        float udaljenost = Vector2.Distance(
            new Vector2(transform.position.z, transform.position.y),
            new Vector2(trenutnaMeta.z, trenutnaMeta.y)
        );

        if (udaljenost <= trenutniDomet)
        {
            SakrijStrelicu(); // Stigla je do cilja -> gasi se
            return;
        }

        // 2. Bazna pozicija u prostoru
        Vector3 baznaPozicija = (transform.parent != null)
            ? transform.parent.TransformPoint(pocetnaLokalnaPozicija)
            : transform.position;

        // 3. Smjer prema meti u Y-Z ravnini (X ignoriramo)
        Vector3 smjerPremaMeti = (trenutnaMeta - baznaPozicija).normalized;
        smjerPremaMeti.x = 0f;

        // 4. ČISTA ROTACIJA OKO X-OSI:
        // Računa nagib između visine (Y) i dubine (Z)
        float kut = Mathf.Atan2(smjerPremaMeti.y, smjerPremaMeti.z) * Mathf.Rad2Deg;

        // Rotiramo samo X os! Dijete automatski prati nagib i ostaje okrenuto kameri
        transform.rotation = Quaternion.Euler(-kut + offsetKuta, 0f, 0f);

        // 5. Pulsiranje prema meti u Y-Z ravnini
        float pomakPremaNaprijed = (Mathf.Sin(Time.time * jitterBrzina) * 0.5f + 0.5f) * jitterAmplituda;
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

        if (childSpriteRenderer != null) childSpriteRenderer.enabled = true;
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
        if (childSpriteRenderer != null) childSpriteRenderer.enabled = false;
        transform.localPosition = pocetnaLokalnaPozicija;
    }
}