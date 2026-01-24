# Advanced Highlight System for Unity

A comprehensive, flexible highlighting system for Unity using HighlightPlus, designed for tower defense games, selection systems, and interactive objects.

## Features

✨ **Multiple Highlight Modes**
- Hover-only highlighting
- Click-to-select (with deselect on outside click)
- External control (for picked-up objects)
- Customizable outline widths and colors
- Multi-selection support

🎮 **Game-Ready Behaviors**
- Tower Defense buildings (view stats on click)
- Pickupable items (highlight changes when held)
- Interactable objects (doors, buttons, levers)
- Selectable units/objects
- Environmental decorations (hover-only)

⚙️ **Highly Customizable**
- Per-object behavior configuration
- Different outline widths for hover vs selection (1px hover, 2px selected)
- Custom colors for different states
- Disable outlines conditionally
- Flash effects on selection

🔧 **Developer Friendly**
- Preset configurations for common use cases
- Extension methods for easy setup
- Clean component-based architecture
- .NET 2.0 compatible

---

## Setup

### 1. Prerequisites
- Unity 6000 (or compatible version)
- HighlightPlus asset installed
- SPACE_UTIL namespace (contains C.method helper)

### 2. Installation
1. Copy all scripts to your project's Scripts folder:
   - `HighlightBehavior.cs`
   - `AdvancedHighlightManager.cs`
   - `HighlightPresets.cs`
   - `PickupController.cs` (optional, for pickup system)

2. Add `AdvancedHighlightManager` component to a GameObject in your scene (e.g., GameManager)

3. Configure the manager:
   - Assign your main Camera
   - Set raycast distance
   - Configure layer mask
   - Enable multi-select if needed

---

## Usage Examples

### Basic Setup - Tower Defense Building

```csharp
// On your tower/building GameObject
HighlightBehavior behavior = gameObject.AddComponent<HighlightBehavior>();

// Configure for tower defense (shows stats when clicked)
behavior.highlightOnHover = true;
behavior.selectOnClick = true;
behavior.hoverOutlineWidth = 1f;
behavior.selectedOutlineWidth = 2f;
behavior.hoverColor = new Color(1f, 1f, 0.5f); // Yellow hover
behavior.selectedColor = new Color(1f, 0.5f, 0f); // Orange when selected

// Now clicking the building will keep it selected until you click elsewhere
// Perfect for showing building stats, upgrade UI, etc.
```

### Using Presets

```csharp
using SPACE_DOODLE;

// Method 1: Add with preset
HighlightBehavior behavior = gameObject.AddHighlightBehavior(HighlightPresetType.TowerDefenseBuilding);

// Method 2: Apply preset to existing behavior
HighlightBehavior existing = GetComponent<HighlightBehavior>();
existing.ApplyPreset(HighlightPresetType.Selectable);

// Method 3: Manual preset configuration
HighlightPresets.ConfigurePickupable(behavior);
```

### Pickup System Example

```csharp
public class MyPickupSystem : MonoBehaviour
{
    private AdvancedHighlightManager highlightManager;
    private HighlightBehavior heldItem;

    void Start()
    {
        highlightManager = FindObjectOfType<AdvancedHighlightManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Get the object player is hovering over
            HighlightBehavior hoveredObject = highlightManager.GetHoveredObject();
            
            if (hoveredObject != null)
            {
                PickupItem(hoveredObject);
            }
        }
    }

    void PickupItem(HighlightBehavior item)
    {
        // Notify the item it's been picked up
        item.SetPickedUp(true);
        
        // If using external control, change the highlight
        if (item.externalControl)
        {
            item.SetExternalHighlight(true); // Green outline while held
        }
        
        heldItem = item;
        // ... rest of pickup logic
    }

    void DropItem()
    {
        if (heldItem != null)
        {
            heldItem.SetPickedUp(false);
            heldItem.SetExternalHighlight(false);
            heldItem = null;
        }
    }
}
```

### Automation Tower Defense Example

```csharp
// For conveyor belts that show status when clicked
public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private GameObject statusUI;
    private AdvancedHighlightManager highlightManager;
    private HighlightBehavior highlightBehavior;

    void Start()
    {
        highlightManager = FindObjectOfType<AdvancedHighlightManager>();
        
        // Setup highlight behavior
        highlightBehavior = gameObject.AddHighlightBehavior(HighlightPresetType.TowerDefenseBuilding);
        
        statusUI.SetActive(false);
    }

    void Update()
    {
        // Check if this belt is selected
        if (highlightManager.IsSelected(highlightBehavior))
        {
            // Show status UI (throughput, items, etc.)
            statusUI.SetActive(true);
        }
        else
        {
            statusUI.SetActive(false);
        }
    }
}
```

---

## Component Reference

### HighlightBehavior

The core component attached to individual GameObjects.

**Inspector Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `highlightOnHover` | bool | Enable outline when mouse hovers |
| `selectOnClick` | bool | Stay selected when clicked |
| `disableOutline` | bool | Completely disable outline |
| `externalControl` | bool | Allow script control (for pickups) |
| `hoverOutlineWidth` | float | Outline width when hovering (default: 1) |
| `selectedOutlineWidth` | float | Outline width when selected (default: 2) |
| `hoverColor` | Color | Color when hovering |
| `selectedColor` | Color | Color when selected |
| `externalColor` | Color | Color for external control |
| `disableWhenPickedUp` | bool | Disable highlight when picked up |
| `flashOnSelect` | bool | Flash effect on selection |
| `allowMultiSelect` | bool | Allow with other selections |

**Public Methods:**

```csharp
void OnHoverEnter()         // Called by manager on hover
void OnHoverExit()          // Called by manager on hover exit
void OnClick()              // Called by manager on click
void Deselect()             // Deselect this object
void SetExternalHighlight(bool highlighted)  // External control
void SetPickedUp(bool pickedUp)  // Notify pickup state
void RefreshSettings()      // Force update visuals
```

**Properties:**

```csharp
bool IsHovered              // Is currently hovered?
bool IsSelected             // Is currently selected?
bool IsPickedUp             // Is currently picked up?
bool IsExternallyHighlighted  // External highlight active?
```

### AdvancedHighlightManager

Central manager that handles all highlight interactions.

**Inspector Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `_cam` | Camera | Camera for raycasting |
| `_raycastDistance` | float | Max raycast distance |
| `_layerMask` | LayerMask | What layers to highlight |
| `selectKey` | KeyCode | Key for selection (Mouse0) |
| `deselectAllKey` | KeyCode | Deselect all (Escape) |
| `clickEmptySpaceToDeselect` | bool | Click outside to deselect |
| `allowMultiSelect` | bool | Enable multi-selection |
| `multiSelectModifier` | KeyCode | Modifier key (Ctrl) |
| `showDebugRays` | bool | Visualize raycasts |
| `logSelectionEvents` | bool | Log selections to console |

**Public Methods:**

```csharp
void DeselectAll()
void Deselect(HighlightBehavior behavior)
void Select(HighlightBehavior behavior, bool clearOthers = true)
List<HighlightBehavior> GetSelectedObjects()
HighlightBehavior GetHoveredObject()
HighlightBehavior GetLastClickedObject()
bool IsSelected(HighlightBehavior behavior)
void ClearHover()
```

---

## Preset Types

### HighlightPresetType Enum

```csharp
public enum HighlightPresetType
{
    HoverOnly,              // Environmental objects
    Selectable,             // Units, buildings
    Pickupable,             // Items, weapons
    TowerDefenseBuilding,   // Towers, structures
    Interactable,           // Doors, buttons
    NoHighlight             // Disable all
}
```

---

## Advanced Scenarios

### Scenario 1: Tower Defense Belt with Status UI

**Requirements:**
- Hover shows yellow outline (1px)
- Click shows orange outline (2px) and status panel
- Click elsewhere hides status panel

**Setup:**
```csharp
// On belt GameObject
HighlightBehavior behavior = gameObject.AddComponent<HighlightBehavior>();
behavior.highlightOnHover = true;
behavior.selectOnClick = true;
behavior.hoverOutlineWidth = 1f;
behavior.selectedOutlineWidth = 2f;
behavior.hoverColor = Color.yellow;
behavior.selectedColor = new Color(1f, 0.5f, 0f);

// In belt script
void Update()
{
    bool isSelected = highlightManager.IsSelected(highlightBehavior);
    statusPanel.SetActive(isSelected);
}
```

### Scenario 2: Pickup Item

**Requirements:**
- Green outline when hovering (1.5px)
- When picked up, outline appears in front of camera
- After pickup, outline disappears
- When dropped, returns to hover behavior

**Setup:**
```csharp
// On item GameObject
HighlightBehavior behavior = gameObject.AddComponent<HighlightBehavior>();
behavior.highlightOnHover = true;
behavior.selectOnClick = false;
behavior.externalControl = true;
behavior.disableWhenPickedUp = true;
behavior.hoverOutlineWidth = 1.5f;
behavior.hoverColor = Color.green;
behavior.externalColor = Color.green;

// When picking up
behavior.SetPickedUp(true);
behavior.SetExternalHighlight(true);

// When dropping
behavior.SetPickedUp(false);
behavior.SetExternalHighlight(false);
```

### Scenario 3: Multi-Select Units (RTS-style)

**Setup in Manager:**
```csharp
// On AdvancedHighlightManager
allowMultiSelect = true;
multiSelectModifier = KeyCode.LeftControl;
clickEmptySpaceToDeselect = true;
```

**On each unit:**
```csharp
HighlightBehavior behavior = gameObject.AddHighlightBehavior(HighlightPresetType.Selectable);
behavior.allowMultiSelect = true;
```

**Usage:**
- Click unit = select (deselects others)
- Ctrl+Click = add to selection
- Click empty = deselect all
- Escape = deselect all

---

## Performance Notes

- Raycasting happens every frame in coroutine (efficient)
- Only one raycast per frame
- Hover state changes only when needed
- List operations optimized for .NET 2.0
- No LINQ dependencies

---

## Troubleshooting

**Outline not appearing:**
- Ensure HighlightPlus is installed
- Check LayerMask in manager includes object's layer
- Verify camera is assigned in manager
- Check `disableOutline` is false

**Selection not working:**
- Ensure `selectOnClick` is true
- Check object has collider
- Verify layermask includes object
- Check raycast distance is sufficient

**Pickup outline not changing:**
- Set `externalControl` to true
- Call `SetPickedUp(true)` when picking up
- Call `SetExternalHighlight(true)` to show outline

**Multi-select not working:**
- Enable `allowMultiSelect` in manager
- Hold modifier key (Ctrl) when clicking
- Check `allowMultiSelect` on behavior if using per-object control

---

## Integration with Game Systems

### Tower Defense Example

```csharp
public class TowerManager : MonoBehaviour
{
    private AdvancedHighlightManager highlightManager;
    private UIPanel towerStatsPanel;

    void Update()
    {
        // Get selected tower
        List<HighlightBehavior> selected = highlightManager.GetSelectedObjects();
        
        if (selected.Count > 0)
        {
            Tower tower = selected[0].GetComponent<Tower>();
            if (tower != null)
            {
                towerStatsPanel.Show(tower);
            }
        }
        else
        {
            towerStatsPanel.Hide();
        }
    }
}
```

---

## Credits

- Built for Unity 6000
- Uses HighlightPlus for rendering
- .NET 2.0 compatible
- Part of SPACE_DOODLE namespace

---

## License

Feel free to use and modify for your projects!
