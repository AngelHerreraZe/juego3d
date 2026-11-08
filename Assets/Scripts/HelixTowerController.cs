using UnityEngine;
using UnityEngine.InputSystem;

public class HelixTowerController : MonoBehaviour
{
    [SerializeField] private float rotationSpeedTouch = 0.35f;
    [SerializeField] private float rotationSpeedKey = 140.0f;
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
        float speedMultiplier = GameManager.singleton != null ? GameManager.singleton.difficultyMultiplier : 1.0f;
        float dragDeltaX = 0f;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            dragDeltaX = Mouse.current.delta.x.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            dragDeltaX = Touchscreen.current.primaryTouch.delta.x.ReadValue();
        }

        if (Mathf.Abs(dragDeltaX) > 0.01f)
        {
            targetRotationY -= dragDeltaX * rotationSpeedTouch * speedMultiplier;
        }

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
            try
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) keyInput += 1f;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) keyInput -= 1f;
            }
            catch (System.InvalidOperationException) { }
        }

        if (Mathf.Abs(keyInput) > 0.01f)
        {
            targetRotationY += keyInput * rotationSpeedKey * speedMultiplier * Time.deltaTime;
        }

        currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, Time.deltaTime * rotationDamping);
        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }

    public void RotateByButton(float direction)
    {
        float speedMultiplier = GameManager.singleton != null ? GameManager.singleton.difficultyMultiplier : 1.0f;
        targetRotationY += direction * 30f * speedMultiplier;
    }
}
