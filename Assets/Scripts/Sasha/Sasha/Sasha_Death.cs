using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Sasha_Death : MonoBehaviour
{
    public Transform glava;
    public Transform kaciga;

    public float brzinaAnimacije = 2f;
    public float udaljenostGlave = 0.5f;
    public float udaljenostKacige = 0.8f;

    private Vector3 pocetnaPozicija;
    private Vector3 ciljGlave;
    private Vector3 ciljKacige;
    private float timer = 0f;

    void Start()
    {
        Vector3 smjer = -transform.up;

        ciljGlave = glava.position + (smjer * udaljenostGlave);
        ciljKacige = kaciga.position + (smjer * udaljenostKacige);

        pocetnaPozicija = transform.position;
    }

    void Update()
    {
        if (timer < 1f)
        {
            timer += Time.deltaTime * brzinaAnimacije;

            glava.position = Vector3.Lerp(glava.position, ciljGlave, timer);
            kaciga.position = Vector3.Lerp(kaciga.position, ciljKacige, timer);
            //InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.C));
        }
    }
}