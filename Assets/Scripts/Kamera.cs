using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Kamera : MonoBehaviour
{
    [SerializeField] GameObject objektIgraca;
    [SerializeField] private float brzina;

    void Update()
    {
        transform.position = objektIgraca.transform.position;
    }

    void LateUpdate()
    {
        Vector2 trenutnaPozicija = transform.position;
        Vector2 krajnjaPozicija = objektIgraca.transform.position;
        transform.position = Vector3.Lerp(trenutnaPozicija,
                                   krajnjaPozicija, brzina * Time.deltaTime);
    }

}
