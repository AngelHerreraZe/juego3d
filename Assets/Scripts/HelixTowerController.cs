using UnityEngine;

/// <summary>
/// Controlador de rotación de la torre central y plataformas helicoidales (Helix Jump).
/// Permite girar la torre sobre el eje Y mediante arrastre táctil (touch swipe),
/// arrastre con ratón o teclas de flechas/teclado, permitiendo que la bola encuentre los huecos.
/// Basado en el tutorial de referencia: https://www.youtube.com/watch?v=QYUgb5pHi9c
/// </summary>
public class HelixTowerController : MonoBehaviour
{
    [Header("Sensibilidad de Rotación")]
    [Tooltip("Velocidad de giro mediante arrastre táctil o ratón")]
    [SerializeField] private float rotationSpeedTouch = 0.4f;

    [Tooltip("Velocidad de giro mediante teclado")]
    [SerializeField] private float rotationSpeedKey = 120.0f;

    [Tooltip("Suavizado de rotación")]
    [SerializeField] private float rotationDamping = 8.0f;

    private Vector2 lastMousePosition;
    private float targetRotationY;
    private float currentRotationY;

    void Start()
    {
        targetRotationY = transform.eulerAngles.y;
        currentRotationY = targetRotationY;
    }

    void Update()
    {
        // 1. Entrada táctil / Ratón
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            targetRotationY -= delta.x * rotationSpeedTouch;
            lastMousePosition = Input.mousePosition;
        }

        // 2. Entrada de teclado (Flechas o teclas Q / E)
        float keyInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) keyInput += 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) keyInput -= 1f;

        if (Mathf.Abs(keyInput) > 0.01f)
        {
            targetRotationY += keyInput * rotationSpeedKey * Time.deltaTime;
        }

        // 3. Interpolación suave del ángulo en el eje Y
        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, Time.deltaTime * rotationDamping);
        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }

    /// <summary>
    /// Método público para vincular con botones UI táctiles
    /// </summary>
    public void RotateByButton(float direction)
    {
        targetRotationY += direction * 30f;
    }
}
