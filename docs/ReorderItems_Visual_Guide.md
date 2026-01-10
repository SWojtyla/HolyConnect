# Reorder Items - Visual Guide

## Page Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Reorder Items                                              [Refresh]    │
│  Organize your collections, sub-collections, and requests               │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │ 🔍 Search items...                                        [X]  │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  [ℹ️ 15 Collections]  [✓ 42 Requests]                                   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│  Items (57)                                                              │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │ 📁 API Tests                                      [📤] [Edit] [Del] │ │
│  │    [Collection]                                                    │ │
│  │    Root level                                                      │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │ 📁 Authentication                                 [📤] [Edit] [Del] │ │
│  │    [Sub-Collection]                                                │ │
│  │    Parent: API Tests                                               │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │ 🌐 GET User Profile                               [📤] [🚫]         │ │
│  │    [REST Request]                                                  │ │
│  │    In: API Tests / Authentication                                  │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │ 📊 GetUsers Query                                 [📤] [🚫]         │ │
│  │    [GraphQL Request]                                               │ │
│  │    In: API Tests / GraphQL                                         │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐ │
│  │ 🔌 WebSocket Chat                                 [📤] [🚫]         │ │
│  │    [WebSocket Request]                                             │ │
│  │    In: API Tests / Realtime                                        │ │
│  └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## Move Dialog

When clicking the [📤] (Move to...) button:

```
┌─────────────────────────────────────────────────────────────────┐
│  Move Item                                          [X]          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ℹ️ Select a destination collection for GET User Profile        │
│                                                                  │
│  ◯ 🏠 Root Level                                                │
│     Not in any collection                                       │
│                                                                  │
│  ─────────────────────────────────────────────────────────────  │
│                                                                  │
│  📁 Available Collections                                        │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🔍 Search collections...                             [X] │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ● 📁 API Tests                                                 │
│     API Tests                                                   │
│                                                                  │
│  ◯ 📁 Authentication                                            │
│     API Tests / Authentication                                  │
│                                                                  │
│  ◯ 📁 GraphQL                                                   │
│     API Tests / GraphQL                                         │
│                                                                  │
│  ◯ 📁 Payments                                                  │
│     E-commerce / Payments                                       │
│                                                                  │
│  ◯ 📁 Orders                                                    │
│     E-commerce / Orders                                         │
│                                                                  │
│                                                 [Cancel] [Move Here] │
└─────────────────────────────────────────────────────────────────┘
```

## Color Scheme

### Item Type Colors (MudBlazor)
- **Collections**: 🔵 Blue (Info)
- **REST Requests**: 🟣 Purple (Primary)
- **GraphQL Requests**: 🟢 Green (Secondary)
- **WebSocket Requests**: 🟠 Orange (Tertiary)

### UI Elements
- **Search Bar**: Outlined with primary color on focus
- **Item Cards**: 
  - Default: Light border
  - Hover: Slight shadow, subtle shift right
  - Transition: 200ms ease-in-out
- **Buttons**:
  - Move: Primary color (blue)
  - Remove: Warning color (yellow/orange)
  - Delete: Error color (red)

## Interaction Flow

### Moving an Item

1. **User clicks "Move to..." icon** (📤)
   - Animation: Button scales slightly
   - Dialog slides in from center with fade

2. **Dialog opens**
   - Default selection: Root level
   - Collections listed with full paths
   - Search appears if >5 collections

3. **User searches/selects destination**
   - Type in search: Results filter instantly
   - Click radio button: Selection updates
   - Full path shown for context

4. **User clicks "Move Here"**
   - Dialog closes with fade out
   - Success snackbar appears: "Request 'X' moved successfully"
   - Item list refreshes
   - Moved item appears in new location

### Removing from Collection

1. **User clicks "Remove from collection" icon** (🚫)
   - Only enabled if item is in a collection
   - Confirmation dialog appears

2. **User confirms**
   - Success snackbar: "Request 'X' removed from collection"
   - Item list refreshes
   - Location changes to "Not in any collection"

### Searching

1. **User types in search box**
   - 300ms debounce
   - Results filter immediately after debounce
   - Matching text can be in:
     - Item name
     - Item URL (for requests)
     - Location path

2. **Clear search**
   - Click X button
   - Shows all items again

## Responsive Behavior

### Desktop (>1200px)
- Full layout with all elements visible
- Item cards span full width
- Comfortable spacing between elements

### Tablet (768px - 1200px)
- Slightly reduced padding
- Button icons may be smaller
- Dialog remains centered and readable

### Mobile (<768px)
- Stack elements vertically
- Touch-friendly button sizes
- Dialog fills most of screen
- Scrollable lists

## Accessibility

### Keyboard Navigation
- Tab through search, items, and buttons
- Enter/Space to activate buttons
- Escape to close dialogs
- Arrow keys in radio groups

### Screen Readers
- ARIA labels on all buttons
- Semantic HTML structure
- Clear focus indicators
- Descriptive error messages

### Visual Indicators
- High contrast between elements
- Clear focus states
- Large touch targets (44px minimum)
- Color + icon + text (not color alone)

## Animation Details

### Fade In (Items)
```css
@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}
```
- Duration: 300ms
- Easing: ease-in
- Applied to: Each item card on load

### Hover Effect (Items)
```css
.item-card:hover {
    transform: translateX(4px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}
```
- Duration: 200ms (from transition)
- Easing: ease-in-out
- Effect: Slight right shift + shadow

### Dialog Transitions
- Open: Fade in + scale from 0.95 to 1.0
- Close: Fade out + scale to 0.95
- Duration: 200ms
- Provided by MudBlazor

## Empty States

### No Items
```
┌─────────────────────────────────────────────────────────────┐
│                                                              │
│                          ℹ️                                  │
│                                                              │
│         No items found. Create collections and              │
│         requests to organize them here.                     │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### No Search Results
```
┌─────────────────────────────────────────────────────────────┐
│                                                              │
│                          ℹ️                                  │
│                                                              │
│         No items match your search query "auth".            │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### No Available Collections (in dialog)
```
┌─────────────────────────────────────────────────────────────┐
│                                                              │
│         ⚠️ No available collections to move to.             │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Loading State

```
┌─────────────────────────────────────────────────────────────┐
│                                                              │
│                          ⏳                                  │
│                    (spinning circle)                         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Success Notifications

All appear as snackbars (bottom-right):

- ✅ "Collection 'X' moved successfully"
- ✅ "Request 'X' moved successfully"
- ✅ "Request 'X' removed from collection"
- ✅ "Data refreshed"

## Error Notifications

- ❌ "Error moving item: [message]"
- ❌ "Error removing request: [message]"
- ℹ️ "Collection is already in that location"
- ℹ️ "Request is already in that location"

## Performance Indicators

With 100+ items:
- Search remains responsive (<50ms)
- Filtering completes in <100ms
- Smooth animations maintained
- No lag in scrolling
- Dialog opens instantly

## Future Enhancements Preview

### Batch Operations (not implemented)
```
┌─────────────────────────────────────────────────────────────┐
│  [☑️] Select All    5 items selected                         │
│                                                              │
│  ☑️ 📁 Collection 1                                          │
│  ☑️ 🌐 Request 1                                             │
│  ☐  🌐 Request 2                                             │
│                                                              │
│  [Move Selected] [Delete Selected]                           │
└─────────────────────────────────────────────────────────────┘
```

### Sort Options (not implemented)
```
Sort by: [Name ▼] [Type] [Location] [Date]
```
