using UnityEngine;

public class PadObjekta : MonoBehaviour
{
    public float brzinaPadanja = 8f;
    private bool dotaknuoPod = false;
    private float udaljenostDoDna;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            udaljenostDoDna = col.bounds.extents.y;
        }
    }

    void Update()
    {
        if (dotaknuoPod) return;

        RaycastHit hit;
        float duljinaZrake = udaljenostDoDna + 0.1f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, duljinaZrake))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + udaljenostDoDna, transform.position.z);
            dotaknuoPod = true;

            SFX sfx = FindFirstObjectByType<SFX>();
            if (sfx != null && sfx.ZvukMetalnogPoda != null) sfx.ZvukMetalnogPoda.Play();
        }
        else
        {
            transform.Translate(Vector3.down * brzinaPadanja * Time.deltaTime, Space.World);
        }
    }
}