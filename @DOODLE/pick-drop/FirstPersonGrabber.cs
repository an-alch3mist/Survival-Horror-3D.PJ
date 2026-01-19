using UnityEngine;

/// <summary>
/// First-person physics-based object grabbing system with spring mechanics.
/// GLITCH-FREE VERSION with CONTROLLED DROP VELOCITY
/// Compatible with Unity 6000.3 and Unity Starter Assets First Person Controller
/// </summary>
public class FirstPersonGrabber : MonoBehaviour
{
	[Header("Grab Detection")]
	[Tooltip("Maximum distance to grab objects")]
	public float grabDistance = 3.5f;

	[Tooltip("Layer mask for grabbable objects")]
	public LayerMask grabbableLayer = ~0; // Everything by default

	[Header("Hold Settings")]
	[Tooltip("Distance from camera to hold object")]
	public float holdDistance = 2f;

	[Tooltip("Minimum hold distance (prevents clipping into camera)")]
	public float minHoldDistance = 1f;

	[Tooltip("Maximum hold distance")]
	public float maxHoldDistance = 5f;

	[Header("Spring Physics - Anti-Glitch Settings")]
	[Tooltip("Spring force strength (300-500 recommended for Unity 6)")]
	[Range(100f, 2000f)]
	public float springStrength = 400f;

	[Tooltip("Damping factor (15-20 recommended for Unity 6)")]
	[Range(0f, 50f)]
	public float dampingFactor = 15f;

	[Tooltip("Target position smoothing time (lower = snappier, higher = smoother)")]
	[Range(0.01f, 0.5f)]
	public float smoothTime = 0.1f;

	[Tooltip("Maximum velocity of grabbed object")]
	public float maxVelocity = 10f;

	[Header("Drop/Throw Velocity Control")]
	[Tooltip("Enable throwing objects on release")]
	public bool allowThrowing = true;

	[Tooltip("Horizontal throw force multiplier (forward direction)")]
	[Range(0f, 20f)]
	public float horizontalThrowForce = 2f;

	[Tooltip("Vertical throw force multiplier (upward direction)")]
	[Range(0f, 20f)]
	public float verticalThrowForce = 1f;

	[Tooltip("Inherit camera movement velocity when dropping")]
	public bool inheritCameraVelocity = true;

	[Tooltip("Camera velocity multiplier (how much camera movement affects throw)")]
	[Range(0f, 2f)]
	public float cameraVelocityMultiplier = 0.3f;

	[Tooltip("Maximum throw velocity (prevents extreme speeds)")]
	public float maxThrowVelocity = 15f;

	[Header("Rotation Settings")]
	[Tooltip("Enable rotation with mouse scroll")]
	public bool allowRotation = true;

	[Tooltip("Rotation speed")]
	public float rotationSpeed = 100f;

	[Header("Visual Feedback")]
	[Tooltip("Show raycast debug line")]
	public bool showDebugRay = true;

	[Tooltip("Color when object is grabbable")]
	public Color grabbableColor = Color.green;

	[Tooltip("Color when holding object")]
	public Color holdingColor = Color.yellow;

	[Header("References")]
	[Tooltip("Player camera (assign in inspector)")]
	public Camera playerCamera;

	// Private variables
	private Rigidbody grabbedRigidbody;
	private float originalDrag;
	private float originalAngularDrag;
	private Vector3 previousCameraPosition;
	private Vector3 cameraVelocity;

	// ANTI-GLITCH: Smoothed target position variables
	private Vector3 smoothedTargetPosition;
	private Vector3 targetPositionVelocity;

	void Start()
	{
		// Auto-assign camera if not set
		if (playerCamera == null)
		{
			playerCamera = Camera.main;
			if (playerCamera == null)
			{
				Debug.LogError("FirstPersonGrabber: No camera assigned and no MainCamera found!");
			}
		}

		previousCameraPosition = playerCamera.transform.position;
	}

	void Update()
	{
		// Calculate camera velocity for throwing
		cameraVelocity = (playerCamera.transform.position - previousCameraPosition) / Time.deltaTime;
		previousCameraPosition = playerCamera.transform.position;

		// Handle grab input
		if (Input.GetMouseButtonDown(0)) // Left click
		{
			if (grabbedRigidbody == null)
				TryGrabObject();
			else
				ReleaseObject();
		}

		// Handle distance adjustment with scroll wheel (when not rotating)
		if (grabbedRigidbody != null && !allowRotation && Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			AdjustHoldDistance(Input.GetAxis("Mouse ScrollWheel"));
		}

		// Handle rotation
		if (grabbedRigidbody != null && allowRotation && Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			RotateGrabbedObject(Input.GetAxis("Mouse ScrollWheel"));
		}

		// Debug visualization
		if (showDebugRay)
		{
			DrawDebugRay();
		}
	}

	void FixedUpdate()
	{
		// Apply spring forces in FixedUpdate for consistent physics
		if (grabbedRigidbody != null)
		{
			ApplySpringForce();
		}
	}

	/// <summary>
	/// Attempts to grab an object in front of the camera
	/// </summary>
	void TryGrabObject()
	{
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
		{
			Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

			// Check if object is valid for grabbing
			if (rb != null && !rb.isKinematic)
			{
				GrabObject(rb);
			}
		}
	}

	/// <summary>
	/// Grabs the specified rigidbody
	/// </summary>
	void GrabObject(Rigidbody rb)
	{
		grabbedRigidbody = rb;

		// ANTI-GLITCH: Initialize smoothed position to current object position
		smoothedTargetPosition = rb.position;
		targetPositionVelocity = Vector3.zero;

		// Store original physics properties
		originalDrag = rb.linearDamping;
		originalAngularDrag = rb.angularDamping;

		// Increase drag for more stable holding
		rb.linearDamping = dampingFactor;
		rb.angularDamping = dampingFactor * 0.5f;

		// IMPORTANT: Enable interpolation on the rigidbody for smooth movement
		if (rb.interpolation == RigidbodyInterpolation.None)
		{
			rb.interpolation = RigidbodyInterpolation.Interpolate;
		}

		// Set initial hold distance based on grab distance
		Vector3 grabPoint = rb.position;
		Vector3 cameraPos = playerCamera.transform.position;
		float distance = Vector3.Distance(grabPoint, cameraPos);
		holdDistance = Mathf.Clamp(distance, minHoldDistance, maxHoldDistance);
	}

	/// <summary>
	/// Applies spring force to move grabbed object toward target position
	/// ANTI-GLITCH VERSION: Uses smoothed target position to prevent jittering
	/// </summary>
	void ApplySpringForce()
	{
		if (grabbedRigidbody == null) return;

		// Calculate raw target position in front of camera
		Vector3 rawTargetPosition = playerCamera.transform.position +
								   playerCamera.transform.forward * holdDistance;

		// ANTI-GLITCH: Smooth the target position to prevent jitter
		// This prevents the object from jerking when camera moves quickly
		smoothedTargetPosition = Vector3.SmoothDamp(
			smoothedTargetPosition,
			rawTargetPosition,
			ref targetPositionVelocity,
			smoothTime
		);

		// === SPRING PHYSICS CALCULATION ===

		// 1. Position Delta: How far object is from smoothed target
		Vector3 positionDelta = smoothedTargetPosition - grabbedRigidbody.position;

		// 2. Spring Force: Proportional to distance (Hooke's Law: F = -kx)
		Vector3 springForce = positionDelta * springStrength;

		// 3. Damping Force: Opposes velocity to prevent oscillation
		Vector3 dampingForce = -grabbedRigidbody.linearVelocity * dampingFactor;

		// 4. Combined Force
		Vector3 totalForce = springForce + dampingForce;

		// Apply the force
		grabbedRigidbody.AddForce(totalForce, ForceMode.Force);

		// Clamp velocity to prevent extreme speeds
		if (grabbedRigidbody.linearVelocity.magnitude > maxVelocity)
		{
			grabbedRigidbody.linearVelocity = grabbedRigidbody.linearVelocity.normalized * maxVelocity;
		}
	}

	/// <summary>
	/// Rotates grabbed object with mouse scroll
	/// </summary>
	void RotateGrabbedObject(float scrollDelta)
	{
		if (grabbedRigidbody == null) return;

		// Rotate around camera's up axis
		Vector3 torque = playerCamera.transform.up * scrollDelta * rotationSpeed;
		grabbedRigidbody.AddTorque(torque, ForceMode.VelocityChange);
	}

	/// <summary>
	/// Adjusts hold distance with scroll wheel
	/// </summary>
	void AdjustHoldDistance(float scrollDelta)
	{
		holdDistance += scrollDelta * 2f;
		holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
	}

	/// <summary>
	/// Releases the currently grabbed object with controlled velocity
	/// </summary>
	void ReleaseObject()
	{
		if (grabbedRigidbody == null) return;

		// Restore original physics properties
		grabbedRigidbody.linearDamping = originalDrag;
		grabbedRigidbody.angularDamping = originalAngularDrag;

		// Apply throw velocity if enabled
		if (allowThrowing)
		{
			// === CONTROLLED DROP VELOCITY ===

			// 1. Calculate base throw direction
			Vector3 forwardDirection = playerCamera.transform.forward;
			Vector3 upDirection = Vector3.up;

			// 2. Separate horizontal and vertical components
			Vector3 horizontalVelocity = new Vector3(forwardDirection.x, 0f, forwardDirection.z).normalized * horizontalThrowForce;
			Vector3 verticalVelocity = upDirection * verticalThrowForce;

			// 3. Combine throw velocities
			Vector3 throwVelocity = horizontalVelocity + verticalVelocity;

			// 4. Add camera movement if enabled (scaled down)
			if (inheritCameraVelocity)
			{
				throwVelocity += cameraVelocity * cameraVelocityMultiplier;
			}

			// 5. Clamp to maximum throw velocity
			if (throwVelocity.magnitude > maxThrowVelocity)
			{
				throwVelocity = throwVelocity.normalized * maxThrowVelocity;
			}

			// 6. Apply the controlled velocity
			grabbedRigidbody.linearVelocity = throwVelocity;
		}
		else
		{
			// If throwing disabled, just let object drop with current physics
			// You can optionally zero out velocity here for a clean drop
			// grabbedRigidbody.velocity = Vector3.zero;
		}

		// Clear reference
		grabbedRigidbody = null;
	}

	/// <summary>
	/// Draws debug ray to show grab range
	/// </summary>
	void DrawDebugRay()
	{
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		Color rayColor = grabbedRigidbody != null ? holdingColor : grabbableColor;

		Debug.DrawRay(ray.origin, ray.direction * grabDistance, rayColor);

		// Draw target position when holding
		if (grabbedRigidbody != null)
		{
			// Show smoothed target position
			Debug.DrawLine(grabbedRigidbody.position, smoothedTargetPosition, Color.cyan);

			// Show raw target position for comparison
			Vector3 rawTarget = playerCamera.transform.position +
							   playerCamera.transform.forward * holdDistance;
			Debug.DrawLine(smoothedTargetPosition, rawTarget, Color.red);
		}
	}

	/// <summary>
	/// Forces release of object (call this when player dies, enters menu, etc.)
	/// </summary>
	public void ForceRelease()
	{
		if (grabbedRigidbody != null)
		{
			grabbedRigidbody.linearDamping = originalDrag;
			grabbedRigidbody.angularDamping = originalAngularDrag;
			grabbedRigidbody = null;
		}
	}

	/// <summary>
	/// Returns true if currently holding an object
	/// </summary>
	public bool IsHoldingObject()
	{
		return grabbedRigidbody != null;
	}

	/// <summary>
	/// Returns the currently held rigidbody (null if none)
	/// </summary>
	public Rigidbody GetHeldObject()
	{
		return grabbedRigidbody;
	}
}