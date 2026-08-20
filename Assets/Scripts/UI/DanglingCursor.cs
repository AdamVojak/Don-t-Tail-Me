using UnityEngine;

public class DanglingCursor : MonoBehaviour
{
    [Header("Postavke Njihanja")]
    [Tooltip("Koliko jako brzina miša naginje kursor")]
    [SerializeField] private float tiltSensitivity = 0.04f;

    [Tooltip("Maksimalni kut nagiba u stupnjevima")]
    [SerializeField] private float maxTiltAngle = 60f;

    [Tooltip("Brzina kojom se njiše i vraća u centar")]
    [SerializeField] private float smoothSpeed = 8f;

    private Vector3 lastMousePosition;
    private float currentZRotation = 0f;

    void OnEnable()
    {
        lastMousePosition = Input.mousePosition;
        currentZRotation = 0f;
        transform.localRotation = Quaternion.identity;
    }

    void Update()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        // Računamo brzinu pomaka miša po X osi
        float mouseDeltaX = (currentMousePosition.x - lastMousePosition.x) / Time.deltaTime;

        // NOVO: Dodan minus (-) kako bi se naginjao u SUPROTNOM smjeru od kretanja (inercija)
        float targetAngle = Mathf.Clamp(-mouseDeltaX * tiltSensitivity, -maxTiltAngle, maxTiltAngle);

        // Glatko naginjemo kursor prema tom kutu (ili natrag na 0 ako miš stane)
        currentZRotation = Mathf.Lerp(currentZRotation, targetAngle, smoothSpeed * Time.deltaTime);

        // Primjenjujemo rotaciju točno oko središta slike (Z os)
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);

        lastMousePosition = currentMousePosition;
    }
}