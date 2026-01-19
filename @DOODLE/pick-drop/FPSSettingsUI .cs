using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SPACE_FIRST_PERSON
{
	/// <summary>
	/// Example UI controller for FirstPersonController settings
	/// </summary>
	public class FPSSettingsUI : MonoBehaviour
	{
		[Header("Controller Reference")]
		[SerializeField] private FirstPersonController fpsController;

		[Header("Movement UI")]
		[SerializeField] private Slider walkSpeedSlider;
		[SerializeField] private Slider runSpeedSlider;
		[SerializeField] private Slider mouseSensitivitySlider;
		[SerializeField] private TextMeshProUGUI walkSpeedText;
		[SerializeField] private TextMeshProUGUI runSpeedText;
		[SerializeField] private TextMeshProUGUI sensitivityText;

		[Header("Audio UI")]
		[SerializeField] private Toggle audioToggle;
		[SerializeField] private Slider footstepVolumeSlider;
		[SerializeField] private Slider jumpVolumeSlider;
		[SerializeField] private Slider landVolumeSlider;
		[SerializeField] private TextMeshProUGUI footstepVolumeText;

		[Header("Visual UI")]
		[SerializeField] private Toggle headBobToggle;
		[SerializeField] private Toggle fovToggle;
		[SerializeField] private Slider normalFOVSlider;
		[SerializeField] private Slider sprintFOVSlider;

		void Start()
		{
			if (fpsController == null)
			{
				Debug.LogError("FPS Controller not assigned!");
				return;
			}

			InitializeUI();
			SetupListeners();
		}

		void InitializeUI()
		{
			// Movement
			if (walkSpeedSlider != null)
				walkSpeedSlider.value = fpsController.WalkSpeed;
			if (runSpeedSlider != null)
				runSpeedSlider.value = fpsController.RunSpeed;
			if (mouseSensitivitySlider != null)
				mouseSensitivitySlider.value = fpsController.MouseSensitivity;

			// Audio
			if (audioToggle != null)
				audioToggle.isOn = fpsController.EnableAudio;
			if (footstepVolumeSlider != null)
				footstepVolumeSlider.value = fpsController.FootstepVolumeMax;
			if (jumpVolumeSlider != null)
				jumpVolumeSlider.value = fpsController.JumpVolume;
			if (landVolumeSlider != null)
				landVolumeSlider.value = fpsController.LandVolume;

			// Visual
			if (headBobToggle != null)
				headBobToggle.isOn = fpsController.EnableHeadBob;
			if (fovToggle != null)
				fovToggle.isOn = fpsController.EnableFOVChange;
			if (normalFOVSlider != null)
				normalFOVSlider.value = fpsController.NormalFOV;
			if (sprintFOVSlider != null)
				sprintFOVSlider.value = fpsController.SprintFOV;

			UpdateAllText();
		}
		void SetupListeners()
		{
			// Movement
			walkSpeedSlider?.onValueChanged.AddListener(OnWalkSpeedChanged);
			runSpeedSlider?.onValueChanged.AddListener(OnRunSpeedChanged);
			mouseSensitivitySlider?.onValueChanged.AddListener(OnSensitivityChanged);

			// Audio
			audioToggle?.onValueChanged.AddListener(OnAudioToggled);
			footstepVolumeSlider?.onValueChanged.AddListener(OnFootstepVolumeChanged);
			jumpVolumeSlider?.onValueChanged.AddListener(OnJumpVolumeChanged);
			landVolumeSlider?.onValueChanged.AddListener(OnLandVolumeChanged);

			// Visual
			headBobToggle?.onValueChanged.AddListener(OnHeadBobToggled);
			fovToggle?.onValueChanged.AddListener(OnFOVToggled);
			normalFOVSlider?.onValueChanged.AddListener(OnNormalFOVChanged);
			sprintFOVSlider?.onValueChanged.AddListener(OnSprintFOVChanged);
		}

		// Movement Callbacks
		void OnWalkSpeedChanged(float value)
		{
			fpsController.WalkSpeed = value;
			if (walkSpeedText != null)
				walkSpeedText.text = $"Walk: {value:F1}";
		}
		void OnRunSpeedChanged(float value)
		{
			fpsController.RunSpeed = value;
			if (runSpeedText != null)
				runSpeedText.text = $"Run: {value:F1}";
		}
		void OnSensitivityChanged(float value)
		{
			fpsController.MouseSensitivity = value;
			if (sensitivityText != null)
				sensitivityText.text = $"Sensitivity: {value:F2}";
		}

		// Audio Callbacks
		void OnAudioToggled(bool enabled)
		{
			fpsController.EnableAudio = enabled;
		}
		void OnFootstepVolumeChanged(float value)
		{
			fpsController.FootstepVolumeMax = value;
			fpsController.FootstepVolumeMin = value * 0.8f; // Keep min 80% of max
			if (footstepVolumeText != null)
				footstepVolumeText.text = $"Footsteps: {value:F2}";
		}
		void OnJumpVolumeChanged(float value)
		{
			fpsController.JumpVolume = value;
		}
		void OnLandVolumeChanged(float value)
		{
			fpsController.LandVolume = value;
		}

		// Visual Callbacks
		void OnHeadBobToggled(bool enabled)
		{
			fpsController.EnableHeadBob = enabled;
		}
		void OnFOVToggled(bool enabled)
		{
			fpsController.EnableFOVChange = enabled;
		}
		void OnNormalFOVChanged(float value)
		{
			fpsController.NormalFOV = value;
		}
		void OnSprintFOVChanged(float value)
		{
			fpsController.SprintFOV = value;
		}
		void UpdateAllText()
		{
			if (walkSpeedText != null)
				walkSpeedText.text = $"Walk: {fpsController.WalkSpeed:F1}";
			if (runSpeedText != null)
				runSpeedText.text = $"Run: {fpsController.RunSpeed:F1}";
			if (sensitivityText != null)
				sensitivityText.text = $"Sensitivity: {fpsController.MouseSensitivity:F2}";
			if (footstepVolumeText != null)
				footstepVolumeText.text = $"Footsteps: {fpsController.FootstepVolumeMax:F2}";
		}

		// Public methods for buttons
		public void ResetToDefaults()
		{
			fpsController.WalkSpeed = 5f;
			fpsController.RunSpeed = 8f;
			fpsController.MouseSensitivity = 2f;
			fpsController.EnableAudio = true;
			fpsController.EnableHeadBob = true;
			fpsController.EnableFOVChange = true;

			InitializeUI();
		}
	} 
}