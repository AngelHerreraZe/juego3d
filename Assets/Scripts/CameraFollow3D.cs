using UnityEngine;

/// <summary>
/// Sistema de seguimiento de cámara adaptado a las restricciones de la actividad 3188:
/// 1. "La cámara debe seguir de manera estable al personaje, aplicando desplazamiento sincronizado únicamente en los ejes X y Z."
/// 2. "El sistema de seguimiento de la cámara debe mantener una posición fija y constante en el eje Z respecto al desplazamiento del personaje principal."
/// 
/// Por tanto:
/// - Eje X: Sigue el desplazamiento del personaje con amortiguamiento suave (SmoothDamp).
/// - Eje Z: Mantiene una distancia fija y constante (fixedOffsetZ) respecto a la posición Z del personaje.
/// - Eje Y: Mantiene una posición vertical fija (fixedHeightY) desacoplada de los rebotes de la bola, garantizando estabilidad visual total sin oscilaciones en móviles.
/// </summary>
public class CameraFollow3D : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform del personaje principal (Bola)")]
    [SerializeField] private Transform target;

    [Header("Restricciones Específicas de la Actividad")]
    [Tooltip("Distancia fija e invariable en el eje Z respecto al personaje")]
    [SerializeField] private float fixedOffsetZ = -6.5f;

    [Tooltip("Desplazamiento horizontal inicial en el eje X")]
    [SerializeField] private float offsetX = 0.0f;

    [Tooltip("Altura fija y constante de la cámara en el eje Y (desacoplada de los rebotes)")]
    [SerializeField] private float fixedHeightY = 3.5f;

    [Header("Estabilidad y Suavizado")]
    [Tooltip("Tiempo de suavizado para el seguimiento lateral en X")]
    [SerializeField] private float smoothTimeX = 0.12f;

    [Tooltip("Ángulo de inclinación (Pitch) hacia el personaje")]
    [SerializeField] private float pitchAngle = 20.0f;

    private float currentVelocityX;

    void Start()
    {
        // Si no se asignó objetivo manualmente, buscar el objeto con tag Player o PlayerBallController
        if (target == null)
        {
            PlayerBallController player = FindAnyObjectByType<PlayerBallController>();
            if (player != null) target = player.transform;
        }

        // Si la altura fija no se definió, tomar la altura inicial de la cámara
        if (fixedHeightY == 0)
        {
            fixedHeightY = transform.position.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Cálculo del seguimiento sincronizado ÚNICAMENTE en los ejes X y Z:
        // En X: Seguimiento dinámico amortiguado para estabilidad visual
        float targetX = target.position.x + offsetX;
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocityX, smoothTimeX);

        // En Z: Posición fija y constante respecto al desplazamiento del personaje
        float newZ = target.position.z + fixedOffsetZ;

        // En Y: Altura fija constante, no sincronizada con el rebote del personaje
        float newY = fixedHeightY;

        // 2. Aplicación de la posición tridimensional con las restricciones requeridas
        transform.position = new Vector3(newX, newY, newZ);

        // 3. Orientación estable: Enfocar hacia la posición proyectada del personaje
        Vector3 lookTarget = new Vector3(target.position.x, target.position.y, target.position.z);
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    /// <summary>
    /// Permite asignar dinámicamente el objetivo
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
