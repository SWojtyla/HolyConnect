# Drag-and-Drop Reorganization Feature

## Overview
HolyConnect now supports drag-and-drop reordering of collections and requests, providing a user-friendly and visually appealing way to organize your API testing workspace.

## Features

### Visual Design
- **Drag Handle Icons**: Each collection and request displays a drag handle icon (☰) on the left
- **Visual Feedback**: When dragging an item over a valid drop target:
  - The target highlights with a subtle blue background
  - A dashed border appears around the drop zone
  - The cursor changes to indicate a valid drop
- **Persistent Ordering**: Order is saved immediately and persists across app restarts

### Functionality

#### Reordering Collections
1. Navigate to any collection in the sidebar or collection view
2. Hover over a collection name to see the drag handle icon
3. Click and hold the drag handle (or anywhere on the collection row)
4. Drag the collection up or down to your desired position
5. Release to drop - the order is saved automatically

**Note**: Collections can only be reordered within their parent scope:
- Root-level collections stay at root level
- Subcollections can only be reordered with sibling subcollections

#### Reordering Requests
1. Open a collection that contains requests
2. Locate the requests in the sidebar or main view
3. Hover over a request name to see the drag handle icon
4. Click and hold the drag handle (or anywhere on the request row)
5. Drag the request up or down within the same collection
6. Release to drop - the order is saved automatically

**Note**: Requests can only be reordered within their parent collection

### User Experience

#### Compact Design
- Minimal visual clutter with small, unobtrusive drag handles
- Handles only appear on hover (via tooltip) to reduce UI noise
- Inline reordering without modal dialogs or extra buttons

#### Mobile-Friendly
- Touch-enabled drag-and-drop for mobile devices
- Works with finger swipes and gestures
- Responsive feedback for touch interactions

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
New methods added to services:
- `ICollectionService.ReorderCollectionsAsync(IEnumerable<Guid> collectionIds)`
- `IRequestService.ReorderRequestsAsync(IEnumerable<Guid> requestIds)`

### UI Components
- `DraggableCollectionTreeItem.razor`: New component with drag-and-drop support
- Updated `CollectionView.razor` with request reordering
- Updated `NavMenu.razor` and `CollectionTreeItem.razor` to sort by Order property

### Browser Compatibility
Uses HTML5 Drag and Drop API with the following events:
- `ondragstart`: Initiates the drag operation
- `ondragover`: Allows dropping by preventing default
- `ondrop`: Handles the drop and reorders items
- `ondragenter`/`ondragleave`: Provides visual feedback

## Usage Tips

1. **Organization Strategy**: Order collections logically (e.g., by API version, by feature area)
2. **Request Flow**: Arrange requests in the order you typically execute them
3. **Visual Grouping**: Use order to group related items together visually
4. **Quick Access**: Put frequently used collections/requests near the top

## Future Enhancements

Potential improvements for future versions:
- Drag collections into other collections (move to different parent)
- Drag requests between collections
- Bulk reorder via a dedicated reorder dialog
- Keyboard shortcuts for reordering (Ctrl+Up/Down)
- Undo/redo for reorder operations
