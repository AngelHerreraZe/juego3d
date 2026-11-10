using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerBallController : MonoBehaviour
{
    [SerializeField] private float moveSpeedX = 5.0f;
    [SerializeField] private float moveSpeedZ = 5.0f;
    [SerializeField] private float bounceForceY = 6.5f;
    [SerializeField] private float manualJumpForce = 7.0f;
    [SerializeField] private bool clampHorizontalMovement = true;
    [SerializeField] private float maxDistanceFromCenter = 2.5f;
    [SerializeField] private GameObject bounceParticlePrefab;

    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private float inputX;
    private float inputZ;
    private bool requestJump;
    private Vector3 startPosition;

    public Vector3 CurrentPosition => transform.position;
    public Vector3 CurrentVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
    public float PosX => transform.position.x;
    public float PosY => transform.position.y;
    public float PosZ => transform.position.z;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        rb.mass = 1.0f;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 0.5f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
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
            try
            {
                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
                if (Input.GetKeyDown(KeyCode.Space)) requestJump = true;
            }
            catch (System.InvalidOperationException) { }
        }

        if (Mathf.Abs(horizontal) > 0.01f) inputX = horizontal;
        if (Mathf.Abs(vertical) > 0.01f) inputZ = vertical;
    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = inputX * moveSpeedX;
        velocity.z = inputZ * moveSpeedZ;

        if (requestJump)
        {
            velocity.y = manualJumpForce;
            requestJump = false;
        }

        rb.linearVelocity = velocity;

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

        inputX = Mathf.MoveTowards(inputX, 0f, Time.fixedDeltaTime * 6f);
        inputZ = Mathf.MoveTowards(inputZ, 0f, Time.fixedDeltaTime * 6f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DeathPart deathPart = collision.transform.GetComponent<DeathPart>();
        if (deathPart != null)
        {
            deathPart.HitDeathPart();
            GameHUDManager.Instance?.TriggerGameOver();
            return;
        }

        ContactPoint contact = collision.contacts[0];
        if (contact.normal.y > 0.4f)
        {
            Vector3 v = rb.linearVelocity;
            v.y = bounceForceY;
            rb.linearVelocity = v;

            if (bounceParticlePrefab != null)
            {
                Instantiate(bounceParticlePrefab, contact.point, Quaternion.identity);
            }

            if (collision.gameObject.CompareTag("Hazard"))
            {
                GameHUDManager.Instance?.TriggerGameOver();
            }
            else if (collision.gameObject.CompareTag("Goal"))
            {
                if (GameManager.singleton != null)
                {
                    GameManager.singleton.NextLevel();
                }
                GameHUDManager.Instance?.TriggerVictory();
            }
            else
            {
                if (GameManager.singleton != null)
                {
                    GameManager.singleton.AddScore(1);
                }
                else
                {
                    GameHUDManager.Instance?.AddScore(1);
                }
            }
        }
    }

    public void ResetBall()
    {
        transform.position = startPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void SetInputX(float val) => inputX = val;
    public void SetInputZ(float val) => inputZ = val;
    public void DoManualJump() => requestJump = true;
}
