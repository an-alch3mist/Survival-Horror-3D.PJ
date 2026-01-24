using UnityEngine;
using SPACE_UTIL;

namespace SPACE_DOODLE
{
	/// <summary>
	/// Example script showing how to integrate the highlight system with game mechanics.
	/// This demonstrates:
	/// - Picking up objects
	/// - External highlight control
	/// - Dynamic behavior changes
	/// </summary>
	public class PickupController : MonoBehaviour
	{
		[Header("Pickup Settings")]
		[SerializeField] private KeyCode pickupKey = KeyCode.E;
		[SerializeField] private float pickupDistance = 3f;
		[SerializeField] private Transform holdPosition;
		[SerializeField] private LayerMask pickupLayer = ~0;

		[Header("References")]
		[SerializeField] private AdvancedHighlightManager highlightManager;
		[SerializeField] private Camera playerCamera;

		[Header("Highlight Control")]
		[SerializeField] private bool highlightHeldObject = true;
		[SerializeField] private bool showHoverWhenHolding = false;

		private GameObject _heldObject;
		private HighlightBehavior _heldBehavior;
		private Vector3 _originalPosition;
		private Quaternion _originalRotation;
		private Transform _originalParent;

		private void Start()
		{
			Debug.Log(C.method(this));

			if (playerCamera == null)
				playerCamera = Camera.main;

			if (highlightManager == null)
				highlightManager = FindObjectOfType<AdvancedHighlightManager>();

			if (holdPosition == null)
			{
				// Create a hold position in front of camera
				GameObject holdObj = new GameObject("HoldPosition");
				holdObj.transform.SetParent(playerCamera.transform);
				holdObj.transform.localPosition = new Vector3(0f, -0.5f, 2f);
				holdPosition = holdObj.transform;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(pickupKey))
			{
				if (_heldObject == null)
				{
					TryPickup();
				}
				else
				{
					Drop();
				}
			}

			// Update held object position
			if (_heldObject != null && holdPosition != null)
			{
				_heldObject.transform.position = holdPosition.position;
				_heldObject.transform.rotation = holdPosition.rotation;
			}
		}

		private void TryPickup()
		{
			HighlightBehavior hoveredBehavior = highlightManager.GetHoveredObject();
			
			if (hoveredBehavior != null)
			{
				GameObject targetObject = hoveredBehavior.gameObject;
				float distance = Vector3.Distance(playerCamera.transform.position, targetObject.transform.position);

				if (distance <= pickupDistance)
				{
					Pickup(targetObject);
				}
				else
				{
					Debug.Log("Object too far to pickup!");
				}
			}
			else
			{
				Debug.Log("No object to pickup!");
			}
		}

		private void Pickup(GameObject obj)
		{
			_heldObject = obj;
			_heldBehavior = obj.GetComponent<HighlightBehavior>();

			// Store original transform
			_originalPosition = obj.transform.position;
			_originalRotation = obj.transform.rotation;
			_originalParent = obj.transform.parent;

			// Disable physics if present
			Rigidbody rb = obj.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.isKinematic = true;
			}

			// Configure highlight behavior for picked up state
			if (_heldBehavior != null)
			{
				// Notify that object is picked up
				_heldBehavior.SetPickedUp(true);

				// If using external control, enable it
				if (_heldBehavior.externalControl)
				{
					_heldBehavior.SetExternalHighlight(highlightHeldObject);
				}

				// Optionally disable hover highlighting while held
				if (!showHoverWhenHolding)
				{
					_heldBehavior.highlightOnHover = false;
				}

				// Deselect from manager
				highlightManager.Deselect(_heldBehavior);
			}

			Debug.Log("Picked up: " + obj.name);
		}

		private void Drop()
		{
			if (_heldObject == null) return;

			// Re-enable physics
			Rigidbody rb = _heldObject.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.isKinematic = false;
			}

			// Restore highlight behavior
			if (_heldBehavior != null)
			{
				_heldBehavior.SetPickedUp(false);
				
				if (_heldBehavior.externalControl)
				{
					_heldBehavior.SetExternalHighlight(false);
				}

				_heldBehavior.highlightOnHover = true;
			}

			// Restore parent
			_heldObject.transform.SetParent(_originalParent);

			Debug.Log("Dropped: " + _heldObject.name);

			_heldObject = null;
			_heldBehavior = null;
		}

		/// <summary>
		/// Example: Toggle highlight on held object
		/// </summary>
		public void ToggleHeldHighlight()
		{
			if (_heldBehavior != null && _heldBehavior.externalControl)
			{
				highlightHeldObject = !highlightHeldObject;
				_heldBehavior.SetExternalHighlight(highlightHeldObject);
			}
		}

		/// <summary>
		/// Get currently held object
		/// </summary>
		public GameObject GetHeldObject()
		{
			return _heldObject;
		}

		/// <summary>
		/// Check if holding an object
		/// </summary>
		public bool IsHoldingObject()
		{
			return _heldObject != null;
		}
	}
}
