using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de movimiento y física para el personaje principal (Bola/Esfera).
/// Compatible al 100% con el nuevo Input System de Unity 6 (com.unity.inputsystem)
/// y adaptado para móviles táctiles y teclado:
/// 1. Desplazamiento observable y continuo en los tres ejes del espacio tridimensional (X, Y, Z).
/// 2. Rebote controlado en el eje Y mediante impulso por script (evita la acumulación infinita de energía del Physic Material explicada en el tutorial QYUgb5pHi9c).
/// 3. Soporte para entrada de teclado (WASD / Flechas / Espacio), pantalla táctil y botones virtuales en dispositivos móviles.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerBallController : MonoBehaviour
{
    [Header("Parámetros de Desplazamiento 3D")]
    [Tooltip("Velocidad de movimiento lateral en el eje X")]
    [SerializeField] private float moveSpeedX = 5.0f;

    [Tooltip("Velocidad de movimiento en profundidad en el eje Z")]
    [SerializeField] private float moveSpeedZ = 5.0f;

    [Tooltip("Fuerza de rebote automático hacia arriba en el eje Y al contactar una plataforma")]
    [SerializeField] private float bounceForceY = 6.5f;

    [Tooltip("Fuerza de salto manual adicional")]
    [SerializeField] private float manualJumpForce = 7.0f;

    [Header("Límites de Movimiento")]
    [SerializeField] private bool clampHorizontalMovement = true;
    [SerializeField] private float maxDistanceFromCenter = 2.5f;

    [Header("Efectos y Retroalimentación")]
    [SerializeField] private GameObject bounceParticlePrefab;

    // Componentes internos
    private Rigidbody rb;
    private SphereCollider sphereCollider;

    // Entradas móviles o externas
    private float inputX;
    private float inputZ;
    private bool requestJump;

    // Propiedades públicas para lectura desde UI y cámara
    public Vector3 CurrentPosition => transform.position;
    public Vector3 CurrentVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
    public float PosX => transform.position.x;
    public float PosY => transform.position.y;
    public float PosZ => transform.position.z;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        // Configuración física óptima para el control de la esfera
        rb.mass = 1.0f;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 0.5f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Congelamos rotaciones físicas si queremos que el control sea directo
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // 1. Captura de entradas con el nuevo Input System (Keyboard)
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                requestJump = true;
            }
        }
        else
        {
            // Fallback seguro a Input legacy (si está habilitado en PlayerSettings)
            try
            {
                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
                if (Input.GetKeyDown(KeyCode.Space)) requestJump = true;
            }
            catch (System.InvalidOperationException)
            {
                // Ignorar excepción si legacy input está deshabilitado
            }
        }

        // Combinar con entradas virtuales móviles si no hay teclado activo
        if (Mathf.Abs(horizontal) > 0.01f) inputX = horizontal;
        if (Mathf.Abs(vertical) > 0.01f) inputZ = vertical;
    }

    void FixedUpdate()
    {
        // 2. Aplicación de velocidad física en los tres ejes (X, Y, Z)
        Vector3 velocity = rb.linearVelocity;

        // Desplazamiento en eje X
        velocity.x = inputX * moveSpeedX;

        // Desplazamiento en eje Z
        velocity.z = inputZ * moveSpeedZ;

        // Salto manual en eje Y si fue solicitado
        if (requestJump)
        {
            velocity.y = manualJumpForce;
            requestJump = false;
        }

        // Asignación de velocidad tridimensional
        rb.linearVelocity = velocity;

        // Delimitación radial para no salir de la torre si está activada
        if (clampHorizontalMovement)
        {
            Vector3 pos = transform.position;
            Vector2 flatPos = new Vector2(pos.x, pos.z);
            if (flatPos.magnitude > maxDistanceFromCenter)
            {
                flatPos = flatPos.normalized * maxDistanceFromCenter;
                transform.position = new Vector3(flatPos.x, pos.y, flatPos.y);
            }
        }

        // Atenuar entradas táctiles gradualmente si no se mantienen pulsadas
        inputX = Mathf.MoveTowards(inputX, 0f, Time.fixedDeltaTime * 6f);
        inputZ = Mathf.MoveTowards(inputZ, 0f, Time.fixedDeltaTime * 6f);
    }

    /// <summary>
    /// Manejo de colisiones: Rebote en eje Y según el tutorial QYUgb5pHi9c
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Evitamos rebotar si la colisión proviene de arriba
        ContactPoint contact = collision.contacts[0];
        if (contact.normal.y > 0.4f)
        {
            // Rebote controlado en Y: reseteamos la velocidad vertical a un impulso constante
            Vector3 v = rb.linearVelocity;
            v.y = bounceForceY;
            rb.linearVelocity = v;

            // Spawn de partícula si existe
            if (bounceParticlePrefab != null)
            {
                Instantiate(bounceParticlePrefab, contact.point, Quaternion.identity);
            }

            // Notificación a HUD o sistema de juego
            if (collision.gameObject.CompareTag("Hazard"))
            {
                GameHUDManager.Instance?.TriggerGameOver();
            }
            else if (collision.gameObject.CompareTag("Goal"))
            {
                GameHUDManager.Instance?.TriggerVictory();
            }
        }
    }

    // Métodos para controles táctiles móviles
    public void SetInputX(float val) => inputX = val;
    public void SetInputZ(float val) => inputZ = val;
    public void DoManualJump() => requestJump = true;
}
