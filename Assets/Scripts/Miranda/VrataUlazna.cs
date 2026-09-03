using UnityEngine;

public class UlaznaVrata : MonoBehaviour
{
    [Header("Postavke Kretanja")]
    [SerializeField] private float brzinaKretanja = 3f;
    public bool zapocniOtvorena = true; // Započinju u zraku da Miranda može ući

    [Header("Audio")]
    [SerializeField] private DoorAudio doorAudio;

    private Vector3 zatvorenaPozicija;
    private Vector3 otvorenaPozicija;
    private Vector3 trenutnaCiljnaPozicija;

    private bool vrataSeMicu = false;
    public bool suVrataOtvorena { get; private set; }

    void Start()
    {
        if (doorAudio == null) doorAudio = GetComponent<DoorAudio>();

        zatvorenaPozicija = transform.position;

        // Računamo visinu da znamo koliko visoko u zrak trebaju ići
        float visinaVrata = IzracunajVisinu();
        otvorenaPozicija = zatvorenaPozicija + Vector3.up * visinaVrata;

        if (zapocniOtvorena)
        {
            transform.position = otvorenaPozicija;
            suVrataOtvorena = true;
            trenutnaCiljnaPozicija = otvorenaPozicija;
        }
        else
        {
            suVrataOtvorena = false;
            trenutnaCiljnaPozicija = zatvorenaPozicija;
        }
    }

    void Update()
    {
        if (vrataSeMicu)
        {
            transform.position = Vector3.MoveTowards(transform.position, trenutnaCiljnaPozicija, brzinaKretanja * Time.deltaTime);

            if (Vector3.Distance(transform.position, trenutnaCiljnaPozicija) < 0.001f)
            {
                transform.position = trenutnaCiljnaPozicija;
                vrataSeMicu = false;

                if (doorAudio != null) doorAudio.StopMoving();
            }
        }
    }

    // Poziva MirandaIntroSequence čim Miranda stigne na Point B
    public void ZatvoriVrata()
    {
        if (suVrataOtvorena && !vrataSeMicu)
        {
            trenutnaCiljnaPozicija = zatvorenaPozicija;
            vrataSeMicu = true;
            suVrataOtvorena = false;

            if (doorAudio != null) doorAudio.StartMoving();
        }
    }

    // Poziva Poluga na kraju levela
    public void OtvoriVrata()
    {
        if (!suVrataOtvorena && !vrataSeMicu)
        {
            trenutnaCiljnaPozicija = otvorenaPozicija;
            vrataSeMicu = true;
            suVrataOtvorena = true;

            if (doorAudio != null) doorAudio.StartMoving();
        }
    }

    public bool DaLiSeMicu()
    {
        return vrataSeMicu;
    }

    private float IzracunajVisinu()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger) return col.bounds.size.y;
        }

        Collider childCollider = GetComponentInChildren<Collider>();
        if (childCollider != null) return childCollider.bounds.size.y;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds.size.y;

        return 3.5f;
    }
}