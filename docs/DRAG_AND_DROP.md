# Collection & Request Reordering Feature

## Overview
HolyConnect supports reordering of collections and requests using an intuitive button-based interface that works reliably across all platforms including MAUI.

## Features

### Visual Design
- **Up/Down Arrow Buttons**: Each collection and request has arrow buttons for reordering
- **Clear Feedback**: Buttons are disabled at boundaries (first item can't move up, last can't move down)
- **Immediate Updates**: Changes are reflected instantly in the UI
- **Persistent Ordering**: Order is saved automatically and persists across app restarts

### Functionality

#### Reordering Collections
1. Navigate to any collection in the sidebar or collection view
2. Click the **up arrow (↑)** button to move a collection up
3. Click the **down arrow (↓)** button to move a collection down
4. Order is saved automatically!

**Note**: Collections can only be reordered within their parent scope:
- Root-level collections stay at root level
- Subcollections can only be reordered with sibling subcollections

#### Reordering Requests
1. Open a collection containing requests
2. Click the **up arrow (↑)** button to move a request up
3. Click the **down arrow (↓)** button to move a request down
4. Order is saved automatically!

**Note**: Requests can only be reordered within their parent collection

### User Experience

#### Button-Based Design
- Clear up/down arrow icons
- Disabled state when movement not possible
- Touch-friendly for mobile devices
- No precision gestures required
- Familiar UI pattern

#### Mobile-Friendly
- Works with simple taps (no gestures)
- Adequate touch targets
- No drag precision needed
- Responsive feedback

#### Visual Hierarchy
- Order is preserved in all views:
  - Navigation sidebar
  - Collection tree view
  - Request list views
- Nested subcollections and requests maintain their order independently

## Technical Implementation

### Domain Model
Two new properties added to entities:
- `Collection.Order` (int): Sorting order for collections
- `Request.Order` (int): Sorting order for requests

### Service Layer
Reordering methods:
- `ICollectionService.ReorderCollectionsAsync(IEnumerable<Guid> collectionIds)`
- `IRequestService.ReorderRequestsAsync(IEnumerable<Guid> requestIds)`

Note: The button-based UI uses simple order swapping (updates only 2 items per action) rather than batch reordering for efficiency.

### UI Components
- `CollectionTreeItem.razor`: Updated with up/down arrow buttons
- `CollectionView.razor`: Request reordering with arrow buttons
- `NavMenu.razor`: Displays collections sorted by Order property

### Platform Compatibility
- ✅ MAUI Desktop (Windows, macOS, Linux)
- ✅ MAUI Mobile (Android, iOS)
- ✅ Touch devices
- ✅ Desktop browsers (if running as Blazor WebAssembly)

## Usage Tips

1. **Organization Strategy**: Order collections logically (e.g., by API version, by feature area)
2. **Request Flow**: Arrange requests in the order you typically execute them
3. **Visual Grouping**: Use order to group related items together visually
4. **Quick Access**: Put frequently used collections/requests near the top

## Future Enhancements

Potential improvements for future versions:
- Drag collections into other collections (move to different parent)
- Drag requests between collections
- Keyboard shortcuts for reordering (Ctrl+Up/Down)
- Bulk reorder via a dedicated reorder dialog
- Undo/redo for reorder operations
