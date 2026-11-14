using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BirdFlight : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float flapSpeed = 5f;

    [Range(0f, 1f)]
    [SerializeField] private float gravityScale = 0.5f;

    private Rigidbody body;
    private Vector2 moveInput;
    private bool flapRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        moveInput = Vector2.zero;

        if (keyboard == null)
            return;

        if (keyboard.wKey.isPressed) moveInput.y += 1f;
        if (keyboard.sKey.isPressed) moveInput.y -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        if (keyboard.spaceKey.wasPressedThisFrame)
            flapRequested = true;
    }


    private void FixedUpdate()
    {
        // Move across the ground plane.
        Vector3 targetVelocity =
            new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;

        Vector3 horizontalVelocity =
            new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);

        Vector3 steeringAcceleration = Vector3.ClampMagnitude(
            (targetVelocity - horizontalVelocity) / Time.fixedDeltaTime,
            acceleration
        );

        body.AddForce(steeringAcceleration, ForceMode.Acceleration);

        // Apply a custom gravity force.
        body.AddForce(
            Physics.gravity * gravityScale,
            ForceMode.Acceleration
        );

        if (flapRequested)
        {
            // Apply an upward force to the bird.
            float upwardChange = flapSpeed - body.linearVelocity.y;

            body.AddForce(
                Vector3.up * upwardChange,
                ForceMode.VelocityChange
            );

            flapRequested = false;
        }
    }
}