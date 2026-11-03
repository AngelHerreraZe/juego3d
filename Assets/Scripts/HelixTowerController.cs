using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de rotación de la torre central y plataformas helicoidales (Helix Jump).
/// Compatible al 100% con el nuevo Input System de Unity 6 (com.unity.inputsystem) y móviles:
/// Permite girar la torre sobre el eje Y mediante arrastre táctil (touch swipe),
/// arrastre con ratón (drag) o teclas de flechas/teclado (A/D, Q/E, Flechas).
/// Basado en el tutorial de referencia: https://www.youtube.com/watch?v=QYUgb5pHi9c
/// </summary>
public class HelixTowerController : MonoBehaviour
{
    [Header("Sensibilidad de Rotación")]
    [Tooltip("Velocidad de giro mediante arrastre táctil o ratón")]
    [SerializeField] private float rotationSpeedTouch = 0.35f;

    [Tooltip("Velocidad de giro mediante teclado")]
    [SerializeField] private float rotationSpeedKey = 140.0f;

    [Tooltip("Suavizado de rotación")]
    [SerializeField] private float rotationDamping = 10.0f;

    private float targetRotationY;
    private float currentRotationY;

    void Start()
    {
        targetRotationY = transform.eulerAngles.y;
        currentRotationY = targetRotationY;
    }

    void Update()
    {
        float dragDeltaX = 0f;

        // 1. Entrada de arrastre con Ratón (Nuevo Input System)
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            dragDeltaX = Mouse.current.delta.x.ReadValue();
        }
        // 2. Entrada de arrastre con Pantalla Táctil (Móviles)
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            dragDeltaX = Touchscreen.current.primaryTouch.delta.x.ReadValue();
        }

        // Aplicar giro por arrastre
        if (Mathf.Abs(dragDeltaX) > 0.01f)
        {
            targetRotationY -= dragDeltaX * rotationSpeedTouch;
        }

        // 3. Entrada de teclado (A/D, Flechas Izq/Der, Q/E)
        float keyInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed)
            {
                keyInput += 1f;
            }
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed || Keyboard.current.eKey.isPressed)
            {
                keyInput -= 1f;
            }
        }
        else
        {
            // Fallback legacy seguro
            try
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) keyInput += 1f;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) keyInput -= 1f;
            }
            catch (System.InvalidOperationException) { }
        }

        if (Mathf.Abs(keyInput) > 0.01f)
        {
            targetRotationY += keyInput * rotationSpeedKey * Time.deltaTime;
        }

        // 4. Interpolación suave del ángulo en el eje Y
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
