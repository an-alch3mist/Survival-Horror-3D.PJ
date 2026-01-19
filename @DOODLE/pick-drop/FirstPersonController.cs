using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
/*
// Runtime audio modification
FirstPersonController fpc = GetComponent<FirstPersonController>();

// Change volumes
fpc.FootstepVolumeMax = 0.7f;
fpc.JumpVolume = 0.5f;

// Change step timing
fpc.WalkStepInterval = 0.6f; // Slower footsteps

// Add new surface at runtime
AudioClip[] metalClips = new AudioClip[] { metal1, metal2, metal3 };
fpc.AddSurfaceSound("Metal", metalPhysicMaterial, metalClips);

// Play custom sound
fpc.PlayCustomSound(customClip, 0.8f, 1.2f);
*/
public class FirstPersonController : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField] private float walkSpeed = 4f;
	[SerializeField] private float runSpeed = 8f;
	[SerializeField] private float crouchSpeed = 2.5f;

	[Header("Jump Settings")]
	[SerializeField] private float jumpHeight = 2f;
	[SerializeField] private float gravity = -9.81f;

	[Header("Mouse Look Settings")]
	[SerializeField] private float mouseSensitivity = 4f;
	[SerializeField] private float maxLookAngle = 70f;

	[Header("Head Bob Settings")]
	[SerializeField] private bool enableHeadBob = true;
	[SerializeField] private float walkBobSpeed = 14f;
	[SerializeField] private float walkBobAmount = 0.03f;
	[SerializeField] private float runBobSpeed = 18f;
	[SerializeField] private float runBobAmount = 0.05f;

	[Header("FOV Settings")]
	[SerializeField] private bool enableFOVChange = true;
	[SerializeField] private float normalFOV = 60f;
	[SerializeField] private float sprintFOV = 70f;
	[SerializeField] private float fovTransitionSpeed = 10f;

	[Header("Audio Settings")]
	[SerializeField] private bool enableAudio = true;
	[SerializeField] private AudioSource audioSource;

	[Header("Footstep Audio")]
	[SerializeField] private float walkStepInterval = 0.5f;
	[SerializeField] private float runStepInterval = 0.3f;
	[SerializeField] private float crouchStepInterval = 0.7f;
	[SerializeField] private float footstepVolumeMin = 0.8f;
	[SerializeField] private float footstepVolumeMax = 1.0f;
	[SerializeField] private float footstepPitchMin = 0.95f;
	[SerializeField] private float footstepPitchMax = 1.05f;

	[Header("Surface Footsteps")]
	[SerializeField] private List<SurfaceSound> surfaceSounds = new List<SurfaceSound>();
	[SerializeField] private AudioClip[] defaultFootsteps;
	[SerializeField] private LayerMask groundCheckLayer = ~0;

	[Header("Action Sounds")]
	[SerializeField] private AudioClip[] jumpSounds;
	[SerializeField] private AudioClip[] landSounds;
	[SerializeField] private float jumpVolume = 0.7f;
	[SerializeField] private float landVolume = 0.8f;
	[SerializeField] private float landVelocityThreshold = -5f; // Play land sound if falling faster than this

	[Header("Camera Reference")]
	[SerializeField] private Camera playerCamera;

	[Header("Input Actions")]
	[SerializeField] private InputActionAsset inputActions;

	// Private variables
	private CharacterController controller;
	private Vector3 velocity;
	private float rotationX = 0f;
	private float defaultCameraYPos;
	private float bobTimer = 0f;
	private float targetFOV;
	private float stepTimer = 0f;
	private bool wasGrounded = true;
	private float lastYVelocity = 0f;

	// Input Action references
	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction jumpAction;
	private InputAction sprintAction;
	private InputAction crouchAction;

	// Input values
	private Vector2 moveInput;
	private Vector2 lookInput;
	private bool isSprinting;
	private bool isCrouching;

	#region Public API Properties

	// Movement Properties
	public float WalkSpeed
	{
		get => walkSpeed;
		set => walkSpeed = Mathf.Max(0f, value);
	}

	public float RunSpeed
	{
		get => runSpeed;
		set => runSpeed = Mathf.Max(0f, value);
	}

	public float CrouchSpeed
	{
		get => crouchSpeed;
		set => crouchSpeed = Mathf.Max(0f, value);
	}

	// Jump Properties
	public float JumpHeight
	{
		get => jumpHeight;
		set => jumpHeight = Mathf.Max(0f, value);
	}

	public float Gravity
	{
		get => gravity;
		set => gravity = value;
	}

	// Mouse Look Properties
	public float MouseSensitivity
	{
		get => mouseSensitivity;
		set => mouseSensitivity = Mathf.Max(0f, value);
	}

	public float MaxLookAngle
	{
		get => maxLookAngle;
		set => maxLookAngle = Mathf.Clamp(value, 0f, 90f);
	}

	// Head Bob Properties
	public bool EnableHeadBob
	{
		get => enableHeadBob;
		set => enableHeadBob = value;
	}

	public float WalkBobSpeed
	{
		get => walkBobSpeed;
		set => walkBobSpeed = Mathf.Max(0f, value);
	}

	public float WalkBobAmount
	{
		get => walkBobAmount;
		set => walkBobAmount = Mathf.Max(0f, value);
	}

	public float RunBobSpeed
	{
		get => runBobSpeed;
		set => runBobSpeed = Mathf.Max(0f, value);
	}

	public float RunBobAmount
	{
		get => runBobAmount;
		set => runBobAmount = Mathf.Max(0f, value);
	}

	// FOV Properties
	public bool EnableFOVChange
	{
		get => enableFOVChange;
		set => enableFOVChange = value;
	}

	public float NormalFOV
	{
		get => normalFOV;
		set
		{
			normalFOV = Mathf.Clamp(value, 1f, 179f);
			if (playerCamera != null && !isSprinting)
			{
				targetFOV = normalFOV;
			}
		}
	}

	public float SprintFOV
	{
		get => sprintFOV;
		set
		{
			sprintFOV = Mathf.Clamp(value, 1f, 179f);
			if (isSprinting && playerCamera != null)
			{
				targetFOV = sprintFOV;
			}
		}
	}

	public float FOVTransitionSpeed
	{
		get => fovTransitionSpeed;
		set => fovTransitionSpeed = Mathf.Max(0f, value);
	}

	// Audio Properties
	public bool EnableAudio
	{
		get => enableAudio;
		set => enableAudio = value;
	}

	public float WalkStepInterval
	{
		get => walkStepInterval;
		set => walkStepInterval = Mathf.Max(0.1f, value);
	}

	public float RunStepInterval
	{
		get => runStepInterval;
		set => runStepInterval = Mathf.Max(0.1f, value);
	}

	public float CrouchStepInterval
	{
		get => crouchStepInterval;
		set => crouchStepInterval = Mathf.Max(0.1f, value);
	}

	public float FootstepVolumeMin
	{
		get => footstepVolumeMin;
		set => footstepVolumeMin = Mathf.Clamp01(value);
	}

	public float FootstepVolumeMax
	{
		get => footstepVolumeMax;
		set => footstepVolumeMax = Mathf.Clamp01(value);
	}

	public float FootstepPitchMin
	{
		get => footstepPitchMin;
		set => footstepPitchMin = Mathf.Max(0.1f, value);
	}

	public float FootstepPitchMax
	{
		get => footstepPitchMax;
		set => footstepPitchMax = Mathf.Max(0.1f, value);
	}

	public float JumpVolume
	{
		get => jumpVolume;
		set => jumpVolume = Mathf.Clamp01(value);
	}

	public float LandVolume
	{
		get => landVolume;
		set => landVolume = Mathf.Clamp01(value);
	}

	// Read-only State Properties
	public bool IsGrounded => controller != null && controller.isGrounded;
	public bool IsSprinting => isSprinting;
	public bool IsCrouching => isCrouching;
	public Vector2 MoveInput => moveInput;
	public float CurrentSpeed => isSprinting ? runSpeed : (isCrouching ? crouchSpeed : walkSpeed);
	public AudioSource AudioSource => audioSource;

	#endregion

	#region Audio Methods

	/// <summary>
	/// Add a surface sound configuration
	/// </summary>
	public void AddSurfaceSound(string surfaceName, PhysicsMaterial material, AudioClip[] clips)
	{
		SurfaceSound newSurface = new SurfaceSound
		{
			surfaceName = surfaceName,
			physicMaterial = material,
			footstepClips = clips
		};
		surfaceSounds.Add(newSurface);
	}

	/// <summary>
	/// Remove a surface sound configuration by name
	/// </summary>
	public void RemoveSurfaceSound(string surfaceName)
	{
		surfaceSounds.RemoveAll(s => s.surfaceName == surfaceName);
	}

	/// <summary>
	/// Set footstep clips for a specific surface
	/// </summary>
	public void SetSurfaceFootsteps(string surfaceName, AudioClip[] clips)
	{
		SurfaceSound surface = surfaceSounds.Find(s => s.surfaceName == surfaceName);
		if (surface != null)
		{
			surface.footstepClips = clips;
		}
	}

	/// <summary>
	/// Set default footstep clips (used when no surface is detected)
	/// </summary>
	public void SetDefaultFootsteps(AudioClip[] clips)
	{
		defaultFootsteps = clips;
	}

	/// <summary>
	/// Set jump sound clips
	/// </summary>
	public void SetJumpSounds(AudioClip[] clips)
	{
		jumpSounds = clips;
	}

	/// <summary>
	/// Set landing sound clips
	/// </summary>
	public void SetLandSounds(AudioClip[] clips)
	{
		landSounds = clips;
	}

	/// <summary>
	/// Play a custom sound through the controller's audio source
	/// </summary>
	public void PlayCustomSound(AudioClip clip, float volume = 1f, float pitch = 1f)
	{
		if (!enableAudio || audioSource == null || clip == null) return;

		audioSource.pitch = pitch;
		audioSource.PlayOneShot(clip, volume);
	}

	#endregion

	void Awake()
	{
		controller = GetComponent<CharacterController>();

		// Get or create AudioSource
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}

		if (audioSource != null)
		{
			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 0f; // 2D sound for first person
		}

		// Get Input Actions from the asset
		if (inputActions != null)
		{
			var actionMap = inputActions.FindActionMap("player");

			moveAction = actionMap.FindAction("move");
			lookAction = actionMap.FindAction("look");
			jumpAction = actionMap.FindAction("jump");
			sprintAction = actionMap.FindAction("sprint");
			crouchAction = actionMap.FindAction("crouch");

			// Enable all actions
			moveAction.Enable();
			lookAction.Enable();
			jumpAction.Enable();
			sprintAction.Enable();
			crouchAction.Enable();
		}
		else
		{
			Debug.LogError("Input Action Asset not assigned!");
		}
	}

	void Start()
	{
		// Lock and hide cursor
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// Store default camera position and FOV
		if (playerCamera != null)
		{
			defaultCameraYPos = playerCamera.transform.localPosition.y;
			normalFOV = playerCamera.fieldOfView;
			targetFOV = normalFOV;
		}
		else
		{
			Debug.LogError("Player Camera not assigned!");
		}
	}

	void Update()
	{
		ReadInputs();
		HandleMouseLook();
		HandleMovement();
		HandleJump();
		HandleHeadBob();
		HandleFOV();
		HandleFootsteps();
		HandleLandingSound();

		// Toggle cursor lock with Escape
		if (Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = !Cursor.visible;
		}

		// Update grounded state tracking
		wasGrounded = controller.isGrounded;
		lastYVelocity = velocity.y;
	}

	void ReadInputs()
	{
		if (moveAction != null)
			moveInput = moveAction.ReadValue<Vector2>();

		if (lookAction != null)
			lookInput = lookAction.ReadValue<Vector2>();

		if (sprintAction != null)
			isSprinting = sprintAction.ReadValue<float>() > 0.5f;

		if (crouchAction != null)
			isCrouching = crouchAction.ReadValue<float>() > 0.5f;
	}

	void HandleMouseLook()
	{
		if (Cursor.lockState != CursorLockMode.Locked) return;

		float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
		float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

		// Rotate player body left/right
		transform.Rotate(Vector3.up * mouseX);

		// Rotate camera up/down with clamping
		rotationX -= mouseY;
		rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

		if (playerCamera != null)
		{
			playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
		}
	}

	void HandleMovement()
	{
		// Determine current speed
		float currentSpeed = walkSpeed;
		if (isSprinting)
		{
			currentSpeed = runSpeed;
		}
		else if (isCrouching)
		{
			currentSpeed = crouchSpeed;
		}

		// Calculate movement direction
		Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
		controller.Move(move * currentSpeed * Time.deltaTime);

		// Apply gravity
		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f; // Small downward force to keep grounded
		}

		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	void HandleJump()
	{
		if (jumpAction != null && jumpAction.triggered && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			PlayJumpSound();
		}
	}

	void HandleHeadBob()
	{
		if (!enableHeadBob || playerCamera == null) return;

		// Only bob when moving and grounded
		bool isMoving = (Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f) && controller.isGrounded;

		if (isMoving)
		{
			float bobSpeed = isSprinting ? runBobSpeed : walkBobSpeed;
			float bobAmount = isSprinting ? runBobAmount : walkBobAmount;

			bobTimer += Time.deltaTime * bobSpeed;
			float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

			Vector3 newCameraPos = playerCamera.transform.localPosition;
			newCameraPos.y = defaultCameraYPos + bobOffset;
			playerCamera.transform.localPosition = newCameraPos;
		}
		else
		{
			// Reset bob timer and smoothly return to default position
			bobTimer = 0f;
			Vector3 newCameraPos = playerCamera.transform.localPosition;
			newCameraPos.y = Mathf.Lerp(newCameraPos.y, defaultCameraYPos, Time.deltaTime * 5f);
			playerCamera.transform.localPosition = newCameraPos;
		}
	}

	void HandleFOV()
	{
		if (!enableFOVChange || playerCamera == null) return;

		// Set target FOV based on sprint state
		bool isMoving = (Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f);
		targetFOV = (isSprinting && isMoving) ? sprintFOV : normalFOV;

		// Smoothly transition FOV
		playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
	}

	void HandleFootsteps()
	{
		if (!enableAudio || audioSource == null) return;

		// Only play footsteps when moving and grounded
		bool isMoving = (Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f) && controller.isGrounded;

		if (isMoving)
		{
			// Determine step interval based on movement state
			float currentStepInterval = walkStepInterval;
			if (isSprinting)
				currentStepInterval = runStepInterval;
			else if (isCrouching)
				currentStepInterval = crouchStepInterval;

			stepTimer += Time.deltaTime;

			if (stepTimer >= currentStepInterval)
			{
				PlayFootstepSound();
				stepTimer = 0f;
			}
		}
		else
		{
			stepTimer = 0f;
		}
	}

	void HandleLandingSound()
	{
		// Play landing sound when transitioning from air to ground
		if (!wasGrounded && controller.isGrounded && lastYVelocity < landVelocityThreshold)
		{
			PlayLandSound();
		}
	}

	void PlayFootstepSound()
	{
		if (!enableAudio || audioSource == null) return;

		AudioClip[] clipsToUse = GetCurrentSurfaceFootsteps();

		if (clipsToUse == null || clipsToUse.Length == 0)
			return;

		AudioClip clip = clipsToUse[Random.Range(0, clipsToUse.Length)];
		float volume = Random.Range(footstepVolumeMin, footstepVolumeMax);
		float pitch = Random.Range(footstepPitchMin, footstepPitchMax);

		audioSource.pitch = pitch;
		audioSource.PlayOneShot(clip, volume);
	}

	void PlayJumpSound()
	{
		if (!enableAudio || audioSource == null || jumpSounds == null || jumpSounds.Length == 0)
			return;

		AudioClip clip = jumpSounds[Random.Range(0, jumpSounds.Length)];
		audioSource.pitch = Random.Range(0.95f, 1.05f);
		audioSource.PlayOneShot(clip, jumpVolume);
	}

	void PlayLandSound()
	{
		if (!enableAudio || audioSource == null || landSounds == null || landSounds.Length == 0)
			return;

		AudioClip clip = landSounds[Random.Range(0, landSounds.Length)];

		// Volume based on fall velocity (harder falls = louder)
		float velocityFactor = Mathf.Clamp01(Mathf.Abs(lastYVelocity) / 20f);
		float volume = Mathf.Lerp(landVolume * 0.5f, landVolume, velocityFactor);

		audioSource.pitch = Random.Range(0.9f, 1.1f);
		audioSource.PlayOneShot(clip, volume);
	}

	AudioClip[] GetCurrentSurfaceFootsteps()
	{
		// Raycast downward to detect surface
		RaycastHit hit;
		Vector3 rayStart = transform.position + Vector3.up * 0.1f;

		if (Physics.Raycast(rayStart, Vector3.down, out hit, controller.height, groundCheckLayer))
		{
			// Try to get the collider's material
			Collider hitCollider = hit.collider;

			if (hitCollider != null)
			{
				/*
				// Check if we have footsteps for this material
				PhysicsMaterial material = hitCollider.sharedMaterial;

				foreach (SurfaceSound surface in surfaceSounds)
				{
					if (surface.physicMaterial == material && surface.footstepClips != null && surface.footstepClips.Length > 0)
					{
						return surface.footstepClips;
					}
				}
				*/

				// Check by tag if material didn't match
				string surfaceTag = hit.collider.tag;
				Debug.Log($"surfaceTag: {surfaceTag}");
				foreach (SurfaceSound surface in surfaceSounds)
				{
					if (surface.surfaceTag == surfaceTag && !string.IsNullOrEmpty(surfaceTag) &&
						surface.footstepClips != null && surface.footstepClips.Length > 0)
					{
						return surface.footstepClips;
					}
				}
			}
		}

		// Return default footsteps if no surface matched
		return defaultFootsteps;
	}

	void OnEnable()
	{
		// Enable all input actions when script is enabled
		moveAction?.Enable();
		lookAction?.Enable();
		jumpAction?.Enable();
		sprintAction?.Enable();
		crouchAction?.Enable();
	}

	void OnDisable()
	{
		// Disable all input actions when script is disabled
		moveAction?.Disable();
		lookAction?.Disable();
		jumpAction?.Disable();
		sprintAction?.Disable();
		crouchAction?.Disable();
	}
}

/// <summary>
/// Defines footstep sounds for different surface types
/// </summary>
[System.Serializable]
public class SurfaceSound
{
	[Tooltip("Name identifier for this surface")]
	public string surfaceName = "Default";

	[Tooltip("Physics material to detect (optional)")]
	public PhysicsMaterial physicMaterial;

	[Tooltip("Surface tag to detect (optional, used as fallback)")]
	public string surfaceTag = "";

	[Tooltip("Footstep audio clips for this surface")]
	public AudioClip[] footstepClips;
}