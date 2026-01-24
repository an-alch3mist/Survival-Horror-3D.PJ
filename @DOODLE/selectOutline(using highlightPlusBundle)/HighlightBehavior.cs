using UnityEngine;
using HighlightPlus;

namespace SPACE_DOODLE
{
	/// <summary>
	/// Component attached to GameObjects to define their highlight behavior.
	/// Controls how the object responds to hover, click, and selection events.
	/// </summary>
	public class HighlightBehavior : MonoBehaviour
	{
		[Header("Highlight Modes")]
		[Tooltip("Enable highlighting when mouse hovers over the object")]
		public bool highlightOnHover = true;

		[Tooltip("Keep object selected when clicked (deselect by clicking elsewhere)")]
		public bool selectOnClick = false;

		[Tooltip("Completely disable outline rendering for this object")]
		public bool disableOutline = false;

		[Tooltip("External control - outline controlled by script (e.g., picked up items)")]
		public bool externalControl = false;

		[Header("Outline Settings")]
		[Tooltip("Outline width for all highlight states")]
		[Range(0f, 10f)]
		public float outlineWidth = 1.5f;

		[Header("Colors")]
		public Color hoverColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold
		public Color selectedColor = new Color(0.2f, 0.8f, 1f, 1f); // Cyan
		public Color externalColor = new Color(0.2f, 1f, 0.2f, 1f); // Green

		[Header("Advanced Options")]
		[Tooltip("Disable highlight when this object is picked up/held")]
		public bool disableWhenPickedUp = false;

		[Tooltip("Flash effect on selection")]
		public bool flashOnSelect = false;

		[Header("Runtime State (Read Only)")]
		[SerializeField] private bool _isHovered = false;
		[SerializeField] private bool _isSelected = false;
		[SerializeField] private bool _isPickedUp = false;
		[SerializeField] private bool _isExternallyHighlighted = false;

		// Private reference
		private HighlightEffect _highlightEffect;

		// Properties for state queries
		public bool IsHovered { get { return _isHovered; } }
		public bool IsSelected { get { return _isSelected; } }
		public bool IsPickedUp { get { return _isPickedUp; } }
		public bool IsExternallyHighlighted { get { return _isExternallyHighlighted; } }

		private void Awake()
		{
			// Get or add HighlightEffect component
			_highlightEffect = GetComponent<HighlightEffect>();
			if (_highlightEffect == null)
			{
				_highlightEffect = gameObject.AddComponent<HighlightEffect>();
			}

			// Initialize highlight effect settings
			if (_highlightEffect != null)
			{
				_highlightEffect.highlighted = false;
				UpdateHighlightSettings();
			}
		}

		private void Start()
		{
			// Apply initial disable state
			if (disableOutline && _highlightEffect != null)
			{
				_highlightEffect.enabled = false;
			}
		}

		/// <summary>
		/// Called by HighlightManager when mouse hovers over this object
		/// </summary>
		public void OnHoverEnter()
		{
			if (!CanHighlight()) return;
			if (!highlightOnHover) return;

			_isHovered = true;
			UpdateHighlightState();
		}

		/// <summary>
		/// Called by HighlightManager when mouse leaves this object
		/// </summary>
		public void OnHoverExit()
		{
			_isHovered = false;
			UpdateHighlightState();
		}

		/// <summary>
		/// Called by HighlightManager when this object is clicked
		/// </summary>
		public void OnClick()
		{
			if (!CanHighlight()) return;
			if (!selectOnClick) return;

			_isSelected = !_isSelected; // Toggle selection

			if (flashOnSelect && _isSelected)
			{
				// Flash effect could be implemented here
				// For now, just select
			}

			UpdateHighlightState();
		}

		/// <summary>
		/// Deselect this object (called when clicking elsewhere)
		/// </summary>
		public void Deselect()
		{
			_isSelected = false;
			UpdateHighlightState();
		}

		/// <summary>
		/// External control for picked up objects or custom highlighting
		/// </summary>
		public void SetExternalHighlight(bool highlighted)
		{
			if (!externalControl) return;

			_isExternallyHighlighted = highlighted;
			UpdateHighlightState();
		}

		/// <summary>
		/// Notify that this object has been picked up
		/// </summary>
		public void SetPickedUp(bool pickedUp)
		{
			_isPickedUp = pickedUp;

			if (disableWhenPickedUp)
			{
				UpdateHighlightState();
			}
		}

		/// <summary>
		/// Update the visual highlight state based on current conditions
		/// </summary>
		private void UpdateHighlightState()
		{
			if (_highlightEffect == null) return;

			// Check if we should show any highlight
			if (!CanHighlight())
			{
				_highlightEffect.highlighted = false;
				return;
			}

			// Determine highlight priority: External > Selected > Hovered
			bool shouldHighlight = false;
			Color outlineColor = hoverColor;

			if (externalControl && _isExternallyHighlighted)
			{
				shouldHighlight = true;
				outlineColor = externalColor;
			}
			else if (_isSelected)
			{
				shouldHighlight = true;
				outlineColor = selectedColor;
			}
			else if (_isHovered)
			{
				shouldHighlight = true;
				outlineColor = hoverColor;
			}

			_highlightEffect.highlighted = shouldHighlight;

			if (shouldHighlight)
			{
				UpdateHighlightSettings(outlineWidth, outlineColor);
			}
		}

		/// <summary>
		/// Check if this object can currently be highlighted
		/// </summary>
		private bool CanHighlight()
		{
			if (disableOutline) return false;
			if (disableWhenPickedUp && _isPickedUp) return false;
			if (_highlightEffect == null) return false;
			if (!_highlightEffect.enabled) return false;

			return true;
		}

		/// <summary>
		/// Update HighlightEffect component settings
		/// </summary>
		private void UpdateHighlightSettings(float width = -1f, Color color = default(Color))
		{
			if (_highlightEffect == null) return;

			if (width < 0f) width = outlineWidth;
			if (color == default(Color)) color = hoverColor;

			_highlightEffect.outlineWidth = width;
			_highlightEffect.outlineColor = color;
		}

		/// <summary>
		/// Force refresh highlight settings from inspector values
		/// </summary>
		public void RefreshSettings()
		{
			UpdateHighlightSettings();
			UpdateHighlightState();
		}

		// Editor helper
		private void OnValidate()
		{
			if (_highlightEffect != null && Application.isPlaying)
			{
				RefreshSettings();
			}
		}
	}
}