# Drag-and-Drop UI Mockup

## Collection Tree View (Sidebar)

```
┌─────────────────────────────────────┐
│  Collections                      + │
│                                     │
│  ☰ 📁 API v1                       │  <-- Root collection with drag handle
│    └─ ☰ 📁 Users                   │  <-- Subcollection (indented)
│       ├─ ☰ 🌐 Get All Users        │  <-- Request in subcollection
│       ├─ ☰ 🌐 Get User by ID       │
│       └─ ☰ 🌐 Create User          │
│    └─ ☰ 📁 Products                │
│       ├─ ☰ 🌐 List Products        │
│       └─ ☰ 🌐 Get Product          │
│                                     │
│  ☰ 📁 API v2                       │  <-- Another root collection
│    └─ ☰ 🌐 Health Check            │
│                                     │
└─────────────────────────────────────┘

Legend:
☰ = Drag handle icon (visible on hover)
📁 = Folder/Collection icon
🌐 = HTTP Request icon
```

## Drag State: Dragging "Get User by ID" Request

```
┌─────────────────────────────────────┐
│  Collections                      + │
│                                     │
│  ☰ 📁 API v1                       │
│    └─ ☰ 📁 Users                   │
│       ├─ ☰ 🌐 Get All Users        │
│       ├─╔════════════════════════╗ │  <-- Drop target with dashed border
│         ║ ☰ 🌐 Create User       ║ │      (subtle blue highlight)
│         ╚════════════════════════╝ │
│       └─ ☰ 🌐 Get Product          │
│                                     │
│  [Dragging: Get User by ID]        │  <-- Visual indicator (browser native)
│                                     │
└─────────────────────────────────────┘

When hovering over "Create User" while dragging "Get User by ID":
- "Create User" row highlights with dashed blue border
- Background tint shows it's a valid drop target
- Dropping here will reorder: Get All Users, Create User, Get User by ID
```

## After Drop: New Order Applied

```
┌─────────────────────────────────────┐
│  Collections                      + │
│                                     │
│  ☰ 📁 API v1                       │
│    └─ ☰ 📁 Users                   │
│       ├─ ☰ 🌐 Get All Users        │
│       ├─ ☰ 🌐 Create User          │  <-- Now in position 2
│       └─ ☰ 🌐 Get User by ID       │  <-- Moved to position 3
│                                     │
│  ☰ 📁 API v2                       │
│    └─ ☰ 🌐 Health Check            │
│                                     │
└─────────────────────────────────────┘

Order is saved automatically - persists across:
- Page reloads
- App restarts
- Different views (sidebar, main collection view)
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
│  │ ☰ 🌐 Get All Users                      [Edit] [Delete] │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ☰ 🌐 Create User                        [Edit] [Delete] │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ☰ 🌐 Get User by ID                     [Edit] [Delete] │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└────────────────────────────────────────────────────────────────┘

Each request card is draggable:
- Drag handle (☰) on the left side
- Full card is draggable (not just the handle)
- Hovering shows grab cursor
```

## Visual Design Details

### Colors (MudBlazor Theme)
- **Drag Handle**: Default icon color (matches theme)
- **Drag-Over Border**: Primary blue color (dashed, 2px)
- **Drag-Over Background**: Primary blue at 8% opacity (rgba)
- **Selected Item**: Primary blue at 12% opacity with solid left border
- **Normal State**: Transparent, no special styling

### Interaction States

1. **Normal State**
   ```
   ☰ 📁 Collection Name
   ```
   - Drag handle visible
   - Standard text color
   - No border or background

2. **Hover State**
   ```
   ☰ 📁 Collection Name        [tooltip: "Drag to reorder"]
   ```
   - Cursor changes to grab (on handle and row)
   - Subtle highlight (theme default)
   - Tooltip appears

3. **Dragging State** (item being dragged)
   ```
   [Browser shows dragged element following cursor]
   ```
   - Native browser drag preview
   - Original position shows gap

4. **Drop Target State** (valid drop location)
   ```
   ╔════════════════════════╗
   ║ ☰ 📁 Collection Name   ║
   ╚════════════════════════╝
   ```
   - Dashed blue border (2px)
   - Light blue background (8% opacity)
   - Indicates valid drop zone

5. **Selected State** (currently active item)
   ```
   │ ☰ 📁 Collection Name
   ```
   - Blue left border (3px solid)
   - Blue background (12% opacity)
   - Remains visible even during drag

### Responsive Behavior

**Desktop (Mouse)**
- Click and drag with mouse
- Smooth cursor tracking
- Drag handle shows grab cursor

**Mobile/Tablet (Touch)**
- Long press to start drag
- Drag follows finger
- Haptic feedback on drop (device dependent)
- Larger touch targets for easier interaction

**Keyboard (Future Enhancement)**
- Not yet implemented
- Future: Ctrl+Up/Down to reorder

## Accessibility

- **ARIA Labels**: 
  - Drag handles have `aria-label="Drag to reorder"`
  - Items have descriptive names read by screen readers
  
- **Keyboard Navigation** (Standard):
  - Tab through items
  - Enter to select
  - (Drag via keyboard not yet implemented)

- **Visual Indicators**:
  - Multiple feedback channels (border, background, cursor)
  - High contrast in drop zone state
  - Not relying solely on color

## Browser Compatibility

Tested and working in:
- ✅ Chrome/Edge (Chromium)
- ✅ Firefox
- ✅ Safari (iOS and macOS)
- ✅ Mobile browsers (Chrome Mobile, Safari Mobile)

Uses standard HTML5 Drag and Drop API - no external libraries needed.
