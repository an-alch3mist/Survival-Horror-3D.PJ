using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SPACE_UTIL;

namespace SPACE_DOODLE
{
	/// <summary>
	/// Central manager for all highlight interactions in the scene.
	/// Handles raycasting, hover detection, click selection, and coordinates between HighlightBehavior components.
	/// </summary>
	public class AdvancedHighlightManager : MonoBehaviour
	{
		[Header("Raycast Settings")]
		[SerializeField] private Camera _cam;
		[SerializeField] private float _raycastDistance = 100f;
		[SerializeField] private LayerMask _layerMask = ~0;

		[Header("Input Settings")]
		[SerializeField] private KeyCode selectKey = KeyCode.Mouse0; // Left click
		[SerializeField] private KeyCode deselectAllKey = KeyCode.Escape;
		[SerializeField] private bool clickEmptySpaceToDeselect = true;

		[Header("Multi-Selection")]
		[SerializeField] private bool allowMultiSelect = false;
		[SerializeField] private KeyCode multiSelectModifier = KeyCode.LeftControl;

		[Header("Debug")]
		[SerializeField] private bool showDebugRays = false;
		[SerializeField] private bool logSelectionEvents = false;

		// Runtime tracking
		private HighlightBehavior _currentHovered;
		private List<HighlightBehavior> _selectedObjects;
		private HighlightBehavior _lastClicked;

		// Performance optimization
		private RaycastHit _hitInfo;
		private Ray _ray;

		private void Awake()
		{
			// Initialize camera reference
			if (_cam == null)
			{
				_cam = Camera.main;
				if (_cam == null)
				{
					Debug.LogError("AdvancedHighlightManager: No camera assigned and Camera.main not found!");
				}
			}

			_selectedObjects = new List<HighlightBehavior>();
		}

		private void Start()
		{
			Debug.Log(C.method(this));
			StartCoroutine(UpdateHighlightSystem());
		}

		/// <summary>
		/// Main update coroutine - handles hover and click detection
		/// </summary>
		private IEnumerator UpdateHighlightSystem()
		{
			while (true)
			{
				if (_cam != null)
				{
					// Handle hover detection
					UpdateHover();

					// Handle click selection
					UpdateSelection();

					// Handle deselect all hotkey
					if (Input.GetKeyDown(deselectAllKey))
					{
						DeselectAll();
					}
				}

				yield return null;
			}
		}

		/// <summary>
		/// Update hover state based on mouse position
		/// </summary>
		private void UpdateHover()
		{
			_ray = _cam.ScreenPointToRay(Input.mousePosition);

			if (showDebugRays)
			{
				Debug.DrawRay(_ray.origin, _ray.direction * _raycastDistance, Color.yellow);
			}

			if (Physics.Raycast(_ray, out _hitInfo, _raycastDistance, _layerMask))
			{
				HighlightBehavior hitBehavior = _hitInfo.transform.GetComponent<HighlightBehavior>();

				// Check if we hit a different object than currently hovered
				if (hitBehavior != _currentHovered)
				{
					// Exit previous hover
					if (_currentHovered != null)
					{
						_currentHovered.OnHoverExit();
					}

					// Enter new hover
					_currentHovered = hitBehavior;
					if (_currentHovered != null)
					{
						_currentHovered.OnHoverEnter();
					}
				}
			}
			else
			{
				// No hit - clear hover
				if (_currentHovered != null)
				{
					_currentHovered.OnHoverExit();
					_currentHovered = null;
				}
			}
		}

		/// <summary>
		/// Handle click selection logic
		/// </summary>
		private void UpdateSelection()
		{
			if (Input.GetKeyDown(selectKey))
			{
				bool multiSelectActive = allowMultiSelect && Input.GetKey(multiSelectModifier);

				if (_currentHovered != null && _currentHovered.selectOnClick)
				{
					// Click on a selectable object
					HandleObjectClick(_currentHovered, multiSelectActive);
				}
				else
				{
					// Click on empty space or non-selectable object
					if (clickEmptySpaceToDeselect && !multiSelectActive)
					{
						DeselectAll();
					}
				}
			}
		}

		/// <summary>
		/// Handle clicking on a HighlightBehavior object
		/// </summary>
		private void HandleObjectClick(HighlightBehavior behavior, bool multiSelect)
		{
			if (behavior == null) return;

			// Check if already selected
			bool wasSelected = _selectedObjects.Contains(behavior);

			if (multiSelect)
			{
				// Multi-select mode: toggle selection
				if (wasSelected)
				{
					_selectedObjects.Remove(behavior);
					behavior.Deselect();

					if (logSelectionEvents)
						Debug.Log("Deselected: " + behavior.gameObject.name);
				}
				else
				{
					_selectedObjects.Add(behavior);
					behavior.OnClick();

					if (logSelectionEvents)
						Debug.Log("Selected: " + behavior.gameObject.name);
				}
			}
			else
			{
				// Single select mode: deselect all others
				if (!wasSelected)
				{
					DeselectAll();
					_selectedObjects.Add(behavior);
					behavior.OnClick();

					if (logSelectionEvents)
						Debug.Log("Selected: " + behavior.gameObject.name);
				}
				else
				{
					// Clicking selected object again - toggle off
					_selectedObjects.Remove(behavior);
					behavior.Deselect();

					if (logSelectionEvents)
						Debug.Log("Deselected: " + behavior.gameObject.name);
				}
			}

			_lastClicked = behavior;
		}

		/// <summary>
		/// Deselect all currently selected objects
		/// </summary>
		public void DeselectAll()
		{
			foreach (HighlightBehavior behavior in _selectedObjects)
			{
				if (behavior != null)
				{
					behavior.Deselect();
				}
			}
			_selectedObjects.Clear();

			if (logSelectionEvents && _selectedObjects.Count > 0)
				Debug.Log("Deselected all objects");
		}

		/// <summary>
		/// Deselect a specific object
		/// </summary>
		public void Deselect(HighlightBehavior behavior)
		{
			if (behavior != null && _selectedObjects.Contains(behavior))
			{
				_selectedObjects.Remove(behavior);
				behavior.Deselect();

				if (logSelectionEvents)
					Debug.Log("Deselected: " + behavior.gameObject.name);
			}
		}

		/// <summary>
		/// Programmatically select an object
		/// </summary>
		public void Select(HighlightBehavior behavior, bool clearOthers = true)
		{
			if (behavior == null) return;

			if (clearOthers)
			{
				DeselectAll();
			}

			if (!_selectedObjects.Contains(behavior))
			{
				_selectedObjects.Add(behavior);
				behavior.OnClick();

				if (logSelectionEvents)
					Debug.Log("Programmatically selected: " + behavior.gameObject.name);
			}
		}

		/// <summary>
		/// Get all currently selected objects
		/// </summary>
		public List<HighlightBehavior> GetSelectedObjects()
		{
			return new List<HighlightBehavior>(_selectedObjects);
		}

		/// <summary>
		/// Get the currently hovered object
		/// </summary>
		public HighlightBehavior GetHoveredObject()
		{
			return _currentHovered;
		}

		/// <summary>
		/// Get the last clicked object
		/// </summary>
		public HighlightBehavior GetLastClickedObject()
		{
			return _lastClicked;
		}

		/// <summary>
		/// Check if a specific object is selected
		/// </summary>
		public bool IsSelected(HighlightBehavior behavior)
		{
			return _selectedObjects.Contains(behavior);
		}

		/// <summary>
		/// Force clear hover state (useful when disabling UI elements)
		/// </summary>
		public void ClearHover()
		{
			if (_currentHovered != null)
			{
				_currentHovered.OnHoverExit();
				_currentHovered = null;
			}
		}

		private void OnDisable()
		{
			// Clean up when disabled
			ClearHover();
			DeselectAll();
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
		}
	}
}