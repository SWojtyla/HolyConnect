# Button-Based Reordering UI - Visual Reference

## Collection Tree View (Sidebar)

```
┌─────────────────────────────────────────────┐
│  Collections                              + │
│                                             │
│  📁 API v1                    [↑][↓][+][✎][🗑] │  <-- Root collection with reorder buttons
│    └─ 📁 Users                [↑][↓][+][✎][🗑] │  <-- Subcollection (indented)
│       ├─ 🌐 Get All Users     [↑][↓][✎][🗑]    │  <-- Request in subcollection
│       ├─ 🌐 Get User by ID    [↑][↓][✎][🗑]    │
│       └─ 🌐 Create User       [↑][↓][✎][🗑]    │
│    └─ 📁 Products             [↑][↓][+][✎][🗑] │
│       ├─ 🌐 List Products     [↑][↓][✎][🗑]    │
│       └─ 🌐 Get Product       [↑][↓][✎][🗑]    │
│                                             │
│  📁 API v2                    [↑][↓][+][✎][🗑] │  <-- Another root collection
│    └─ 🌐 Health Check         [↑][↓][✎][🗑]    │
│                                             │
└─────────────────────────────────────────────┘

Legend:
[↑] = Move up button (ArrowUpward icon)
[↓] = Move down button (ArrowDownward icon)
[+] = Add sub-collection or request
[✎] = Edit collection/request
[🗑] = Delete collection/request
📁 = Folder/Collection icon
🌐 = HTTP Request icon
```

## Button States

### First Item (Can't Move Up)
```
┌─────────────────────────────────────────────┐
│  🌐 Get All Users     [↑̸][↓][✎][🗑]         │  <-- Up button disabled (grayed out)
└─────────────────────────────────────────────┘
```

### Middle Item (Can Move Both Ways)
```
┌─────────────────────────────────────────────┐
│  🌐 Get User by ID    [↑][↓][✎][🗑]          │  <-- Both buttons enabled
└─────────────────────────────────────────────┘
```

### Last Item (Can't Move Down)
```
┌─────────────────────────────────────────────┐
│  🌐 Create User       [↑][↓̸][✎][🗑]         │  <-- Down button disabled (grayed out)
└─────────────────────────────────────────────┘
```

## Reordering Action

**Before - Initial Order:**
```
┌─────────────────────────────────────────────┐
│  📁 Users                                    │
│    ├─ 🌐 Get All Users     [↑̸][↓][✎][🗑]    │
│    ├─ 🌐 Get User by ID    [↑][↓][✎][🗑]    │
│    └─ 🌐 Create User       [↑][↓̸][✎][🗑]    │
└─────────────────────────────────────────────┘
```

**Action: Click [↓] on "Get All Users"**

**After - New Order:**
```
┌─────────────────────────────────────────────┐
│  📁 Users                                    │
│    ├─ 🌐 Get User by ID    [↑̸][↓][✎][🗑]    │  <-- Now first
│    ├─ 🌐 Get All Users     [↑][↓][✎][🗑]    │  <-- Moved down
│    └─ 🌐 Create User       [↑][↓̸][✎][🗑]    │
└─────────────────────────────────────────────┘

Order is saved automatically and persists across app restarts.
```

## Collection View Page - Request List

```
┌────────────────────────────────────────────────────────────────┐
│  API v1 / Users                                    [+ New]      │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Requests                                                        │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🌐 Get All Users              [↑][↓][✎][🗑]              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🌐 Create User                [↑][↓][✎][🗑]              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🌐 Get User by ID             [↑][↓][✎][🗑]              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└────────────────────────────────────────────────────────────────┘

Each request card has reorder buttons on the right side.
```

## Visual Design Details

### Button Styling
- **Size**: Small (matches other action buttons)
- **Spacing**: Compact gap between buttons (0.25rem)
- **Icons**: Material Icons (ArrowUpward, ArrowDownward)
- **Color**: Default (theme-based)
- **Disabled State**: Grayed out, non-clickable

### Interaction Flow
1. **Click Up Button**: Swaps order with item above
2. **Click Down Button**: Swaps order with item below
3. **Immediate Feedback**: UI updates immediately
4. **Auto-save**: Order persists without manual save
5. **Reload**: Order maintained across page reloads

### Button Placement
- **Collections**: `[↑][↓][+][✎][🗑]` (reorder, add, edit, delete)
- **Requests**: `[↑][↓][✎][🗑]` (reorder, edit, delete)
- Buttons appear on the right side of each item
- Consistent positioning across all views

## Mobile/Touch Considerations

**Touch Targets:**
- Buttons are MudBlazor MudIconButton components
- Size.Small provides adequate touch target
- Spacing between buttons prevents accidental taps
- No gesture recognition needed (simple tap)

**Responsive Layout:**
- Buttons scale with viewport
- Touch-friendly on tablets and phones
- No precision dragging required
- Clear visual feedback on tap

## Accessibility

- **Keyboard Navigation**: Tab through buttons, Enter to activate
- **Screen Readers**: aria-label describes each button action
  - "Move up"
  - "Move down"
  - "Edit collection/request"
  - "Delete collection/request"
- **Visual Clarity**: Icons clearly indicate direction
- **Disabled State**: Clear visual indication when action unavailable

## Advantages Over Drag-and-Drop

✅ **Works in MAUI**: No browser compatibility issues
✅ **Touch-Friendly**: Large, clear tap targets
✅ **Predictable**: One click = one position change
✅ **Accessible**: Keyboard and screen reader compatible
✅ **Reliable**: No gesture recognition issues
✅ **Simple**: Clear cause and effect
✅ **Cross-Platform**: Works identically everywhere

## Implementation Notes

- Uses simple order value swapping (not full reorder)
- Only two items updated per action (efficient)
- Disabled state prevents invalid actions
- No drag state to manage (simpler code)
- Fits existing button-based UI patterns
