using UnityEngine;

namespace SPACE_DOODLE
{
	/// <summary>
	/// Helper class with preset configurations and utility methods for HighlightBehavior
	/// </summary>
	public static class HighlightPresets
	{
		/// <summary>
		/// Configure for hover-only highlighting (no selection)
		/// Example: Environmental objects, decorations
		/// </summary>
		public static void ConfigureHoverOnly(HighlightBehavior behavior)
		{
			if (behavior == null) return;

			behavior.highlightOnHover = true;
			behavior.selectOnClick = false;
			behavior.disableOutline = false;
			behavior.externalControl = false;
			behavior.disableWhenPickedUp = false;
			behavior.outlineWidth = 1.5f;
			behavior.hoverColor = new Color(1f, 0.8f, 0.2f, 1f);
		}

		/// <summary>
		/// Configure for selection system (hover + click to select)
		/// Example: Units, buildings in strategy game
		/// </summary>
		public static void ConfigureSelectable(HighlightBehavior behavior)
		{
			if (behavior == null) return;

			behavior.highlightOnHover = true;
			behavior.selectOnClick = true;
			behavior.disableOutline = false;
			behavior.externalControl = false;
			behavior.disableWhenPickedUp = false;
			behavior.outlineWidth = 1.5f;
			behavior.hoverColor = new Color(1f, 0.8f, 0.2f, 1f);
			behavior.selectedColor = new Color(0.2f, 0.8f, 1f, 1f);
		}

		/// <summary>
		/// Configure for pickup system
		/// Example: Items, weapons, collectibles
		/// </summary>
		public static void ConfigurePickupable(HighlightBehavior behavior)
		{
			if (behavior == null) return;

			behavior.highlightOnHover = true;
			behavior.selectOnClick = false;
			behavior.disableOutline = false;
			behavior.externalControl = true;
			behavior.disableWhenPickedUp = true;
			behavior.outlineWidth = 1.5f;
			behavior.hoverColor = new Color(0.2f, 1f, 0.2f, 1f); // Green
			behavior.externalColor = new Color(0.2f, 1f, 0.2f, 1f);
		}

		/// <summary>
		/// Configure for tower defense building/tower
		/// Example: Towers, belts, structures that show info on click
		/// </summary>
		public static void ConfigureTowerDefenseBuilding(HighlightBehavior behavior)
		{
			if (behavior == null) return;

			behavior.highlightOnHover = true;
			behavior.selectOnClick = true;
			behavior.disableOutline = false;
			behavior.externalControl = false;
			behavior.disableWhenPickedUp = false;
			behavior.outlineWidth = 2f;
			behavior.hoverColor = new Color(1f, 1f, 0.5f, 1f); // Light yellow
			behavior.selectedColor = new Color(1f, 0.5f, 0f, 1f); // Orange
			behavior.flashOnSelect = true;
		}

		/// <summary>
		/// Configure for interactable objects (doors, buttons, levers)
		/// </summary>
		public static void ConfigureInteractable(HighlightBehavior behavior)
		{
			if (behavior == null) return;

			behavior.highlightOnHover = true;
			behavior.selectOnClick = false;
			behavior.disableOutline = false;
			behavior.externalControl = false;
			behavior.outlineWidth = 1.5f;
			behavior.hoverColor = new Color(0.5f, 1f, 0.5f, 1f); // Light green
		}

		/// <summary>
		/// Disable all highlighting
		/// </summary>
		public static void ConfigureNoHighlight(HighlightBehavior behavior)
		{
			if (behavior == null) return;

			behavior.disableOutline = true;
			behavior.highlightOnHover = false;
			behavior.selectOnClick = false;
		}
	}

	/// <summary>
	/// Extension methods for easy HighlightBehavior setup
	/// </summary>
	public static class HighlightExtensions
	{
		/// <summary>
		/// Add and configure HighlightBehavior with a preset
		/// </summary>
		public static HighlightBehavior AddHighlightBehavior(this GameObject go, HighlightPresetType preset)
		{
			HighlightBehavior behavior = go.GetComponent<HighlightBehavior>();
			if (behavior == null)
			{
				behavior = go.AddComponent<HighlightBehavior>();
			}

			ApplyPreset(behavior, preset);
			return behavior;
		}

		/// <summary>
		/// Apply a preset configuration to existing behavior
		/// </summary>
		public static void ApplyPreset(this HighlightBehavior behavior, HighlightPresetType preset)
		{
			switch (preset)
			{
				case HighlightPresetType.HoverOnly:
					HighlightPresets.ConfigureHoverOnly(behavior);
					break;
				case HighlightPresetType.Selectable:
					HighlightPresets.ConfigureSelectable(behavior);
					break;
				case HighlightPresetType.Pickupable:
					HighlightPresets.ConfigurePickupable(behavior);
					break;
				case HighlightPresetType.TowerDefenseBuilding:
					HighlightPresets.ConfigureTowerDefenseBuilding(behavior);
					break;
				case HighlightPresetType.Interactable:
					HighlightPresets.ConfigureInteractable(behavior);
					break;
				case HighlightPresetType.NoHighlight:
					HighlightPresets.ConfigureNoHighlight(behavior);
					break;
			}

			behavior.RefreshSettings();
		}

		/// <summary>
		/// Quick method to enable/disable highlight
		/// </summary>
		public static void SetHighlightEnabled(this HighlightBehavior behavior, bool enabled)
		{
			if (behavior == null) return;
			behavior.disableOutline = !enabled;
			behavior.RefreshSettings();
		}
	}

	/// <summary>
	/// Preset types for easy configuration
	/// </summary>
	public enum HighlightPresetType
	{
		HoverOnly,
		Selectable,
		Pickupable,
		TowerDefenseBuilding,
		Interactable,
		NoHighlight
	}
}