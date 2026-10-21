using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Press W, Up Arrow, or Space once to start the flight.</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class AirplaneFlightController : MonoBehaviour
{
    [Header("Flight")]
    [Min(0.1f)] public float forwardSpeed = 5f;
    [Min(0f)] public float takeoffAfterDistance = 2.5f;
    [Min(0.1f)] public float climbRate = 1.4f;
    [Min(0.1f)] public float cruiseAltitude = 1.6f;
    [Min(0.1f)] public float takeoffHeight = 0.65f;

    public bool HasTakenOff { get; private set; }
    public bool HasStarted { get; private set; }
    private Rigidbody body;
    private Camera followCamera;
    private Vector3 cameraOffset;
    private float distanceTravelled;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        followCamera = Camera.main;
        if (followCamera != null) cameraOffset = followCamera.transform.position - transform.position;
    }

    private void Update()
    {
        if (HasStarted || Keyboard.current == null) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            HasStarted = true;
            Debug.Log("Airplane takeoff sequence started. Watch for the Enter Wall.");
        }
    }

    private void FixedUpdate()
    {
        if (!HasStarted || Time.timeScale <= 0f) return;
        float step = forwardSpeed * Time.fixedDeltaTime;
        Vector3 nextPosition = body.position + Vector3.back * step;
        distanceTravelled += step;

        if (distanceTravelled >= takeoffAfterDistance)
        {
            nextPosition.y = Mathf.MoveTowards(nextPosition.y, cruiseAltitude, climbRate * Time.fixedDeltaTime);
            if (nextPosition.y >= takeoffHeight) HasTakenOff = true;
        }
        body.MovePosition(nextPosition);
    }

    private void LateUpdate()
    {
        if (HasStarted && followCamera != null) followCamera.transform.position = transform.position + cameraOffset;
    }
}
