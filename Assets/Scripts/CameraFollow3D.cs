using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Configuracion Orbital y Altura")]
    [SerializeField] private float orbitDistance = 6.8f;
    [SerializeField] private float heightOffset = 2.4f;
    [SerializeField] private float lookOffsetY = 0.4f;
    [SerializeField] private float smoothTimeOrbit = 0.12f;
    [SerializeField] private float smoothTimeY = 0.14f;

    [Header("Modo")]
    [SerializeField] private bool onlyFollowDownwards = true;

    private float currentAngle;
    private float angleVelocity;
    private float currentY;
    private float velocityY;
    private float currentTargetY;
    private bool isInitialized = false;

    void Start()
    {
        InitializeCamera();
    }

    public void InitializeCamera()
    {
        if (target == null)
        {
            PlayerBallController player = FindAnyObjectByType<PlayerBallController>();
            if (player != null) target = player.transform;
        }

        if (target != null)
        {
            Vector2 flatPos = new Vector2(target.position.x, target.position.z);
            if (flatPos.sqrMagnitude > 0.05f)
            {
                currentAngle = Mathf.Atan2(flatPos.x, flatPos.y) * Mathf.Rad2Deg;
            }
            else
            {
                currentAngle = 0f;
            }

            currentTargetY = target.position.y;
            currentY = currentTargetY + heightOffset;
        }
        else
        {
            currentAngle = 0f;
            currentY = transform.position.y;
            currentTargetY = transform.position.y;
        }

        UpdateCameraPosition(instant: true);
        isInitialized = true;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            PlayerBallController player = FindAnyObjectByType<PlayerBallController>();
            if (player != null) target = player.transform;
            if (target == null) return;
        }

        if (!isInitialized)
        {
            InitializeCamera();
        }

        UpdateCameraPosition(instant: false);
    }

    private void UpdateCameraPosition(bool instant)
    {
        if (target == null) return;

        // 1. Seguimiento orbital 360: la camara gira alrededor del cilindro para estar SIEMPRE detras de la bola
        Vector2 flatPos = new Vector2(target.position.x, target.position.z);
        if (flatPos.sqrMagnitude > 0.05f)
        {
            float targetAngle = Mathf.Atan2(flatPos.x, flatPos.y) * Mathf.Rad2Deg;

            if (instant)
            {
                currentAngle = targetAngle;
                angleVelocity = 0f;
            }
            else
            {
                // Si la bola cambia de lado rapidamente, acelerar la rotacion para evitar oclusion
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
                if (angleDiff > 75f)
                {
                    currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 360f * Time.deltaTime);
                }
                else
                {
                    currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, smoothTimeOrbit);
                }
            }
        }

        // 2. Seguimiento vertical en Y: desciende suavemente acompanando a la bola a traves de los pisos
        float desiredY = target.position.y;
        if (onlyFollowDownwards)
        {
            if (desiredY < currentTargetY)
            {
                currentTargetY = desiredY;
            }
            else if (desiredY > currentTargetY + 3.0f)
            {
                // En caso de reinicio de posicion (ResetBall)
                currentTargetY = desiredY;
            }
            else
            {
                // Relajar hacia arriba lentamente si la bola se queda fija en un nivel superior
                currentTargetY = Mathf.MoveTowards(currentTargetY, desiredY, 1.5f * Time.deltaTime);
            }
        }
        else
        {
            currentTargetY = desiredY;
        }

        float targetCamY = currentTargetY + heightOffset;

        if (instant)
        {
            currentY = targetCamY;
            velocityY = 0f;
        }
        else
        {
            currentY = Mathf.SmoothDamp(currentY, targetCamY, ref velocityY, smoothTimeY);

            float maxLagDistance = 3.5f;
            if (currentY - target.position.y > maxLagDistance + heightOffset)
            {
                currentY = target.position.y + maxLagDistance + heightOffset;
            }
        }

        // 3. Posicionamiento en coordenadas cilindricas alrededor del mastil central
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 camPos = new Vector3(Mathf.Sin(rad) * orbitDistance, currentY, Mathf.Cos(rad) * orbitDistance);
        transform.position = camPos;

        // 4. Orientacion visual cinematica: enfocar hacia la bola y el centro de la torre
        Vector3 lookTarget = new Vector3(target.position.x * 0.35f, target.position.y + lookOffsetY, target.position.z * 0.35f);
        if ((lookTarget - transform.position).sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);
        }
    }

    public void ResetCamera()
    {
        InitializeCamera();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        InitializeCamera();
    }
}
