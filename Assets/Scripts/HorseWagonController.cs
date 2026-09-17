using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Handles keyboard/gamepad movement for a horse that pulls a physically connected wagon.</summary>
public sealed class HorseWagonController : MonoBehaviour
{
    private const float InputDeadZone = 0.01f;

    [Header("Movement")]
    [SerializeField] private float maximumSpeed = 5f;
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float braking = 20f;
    [SerializeField] private float turnSpeed = 65f;
    public WagonHealth playerWagonHealth;

    private Rigidbody horseRigidbody;
    private Animator horseAnimator;
    private Vector2 movementInput;

    /// <summary>Top planar speed this wagon can reach, in units/second. Used by AI and damage systems to compare speed.</summary>
    public float MaximumSpeed => maximumSpeed;

    private void Awake()
    {
        horseRigidbody = GetComponent<Rigidbody>();
        horseAnimator = GetComponent<Animator>();
        playerWagonHealth = GetComponent<WagonHealth>();
    }

    private void Update()
    {
        movementInput = ReadMovementInput();
    }

    private void FixedUpdate()
    {
        if (horseRigidbody == null)
        {
            return;
        }

        float throttle = movementInput.y;
        float steering = movementInput.x;
        Vector3 currentPlanarVelocity = new Vector3(horseRigidbody.linearVelocity.x, 0f, horseRigidbody.linearVelocity.z);
        Vector3 targetPlanarVelocity = transform.forward * (throttle * maximumSpeed);
        float speedChange = Mathf.Abs(throttle) > InputDeadZone ? acceleration : braking;
        Vector3 nextPlanarVelocity = Vector3.MoveTowards(
            currentPlanarVelocity,
            targetPlanarVelocity,
            speedChange * Time.fixedDeltaTime);

        horseRigidbody.linearVelocity = new Vector3(
            nextPlanarVelocity.x,
            horseRigidbody.linearVelocity.y,
            nextPlanarVelocity.z);

        if (Mathf.Abs(steering) > InputDeadZone)
        {
            float direction = Mathf.Abs(throttle) > InputDeadZone ? Mathf.Sign(throttle) : 1f;
            float turnAmount = steering * direction * turnSpeed * Time.fixedDeltaTime;
            horseRigidbody.MoveRotation(horseRigidbody.rotation * Quaternion.Euler(0f, turnAmount, 0f));
        }

        UpdateHorseAnimator(throttle, steering);
    }

    private static Vector2 ReadMovementInput()
    {
        Vector2 input = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && input.sqrMagnitude < InputDeadZone * InputDeadZone)
        {
            input = gamepad.leftStick.ReadValue();
        }

        return Vector2.ClampMagnitude(input, 1f);
    }

    private void UpdateHorseAnimator(float throttle, float steering)
    {
        if (horseAnimator == null)
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in horseAnimator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Float && parameter.name == "Vertical")
            {
                horseAnimator.SetFloat(parameter.name, throttle);
            }
            else if (parameter.type == AnimatorControllerParameterType.Float && parameter.name == "Horizontal")
            {
                horseAnimator.SetFloat(parameter.name, steering);
            }
        }
    }
}
