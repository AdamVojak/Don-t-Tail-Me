using UnityEngine;
using UnityEngine.UI;

public class DanglingCursor_Menu : MonoBehaviour
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
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // KLJUČNO: Gasimo Raycast Target tako da kursor NIKAD ne blokira gumbe ispod sebe!
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null) graphic.raycastTarget = false;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = false;

        // Sakrij sistemski Windows miš
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnEnable()
    {
        lastMousePosition = Input.mousePosition;
        currentZRotation = 0f;
        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        // 1. Kursor prati točnu poziciju miša
        if (rectTransform != null)
        {
            transform.position = currentMousePosition;
        }

        // 2. Inercija i njihanje po X osi
        float mouseDeltaX = (currentMousePosition.x - lastMousePosition.x) / Mathf.Max(Time.deltaTime, 0.001f);
        float targetAngle = Mathf.Clamp(-mouseDeltaX * tiltSensitivity, -maxTiltAngle, maxTiltAngle);

        currentZRotation = Mathf.Lerp(currentZRotation, targetAngle, smoothSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0, 0, currentZRotation);

        lastMousePosition = currentMousePosition;
    }
}