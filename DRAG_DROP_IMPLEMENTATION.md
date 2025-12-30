# Drag and Drop Tree View Implementation Summary

## Overview
This document summarizes the implementation of drag-and-drop functionality for the HolyConnect tree view, allowing users to reorganize collections and requests within the hierarchy.

## Implementation Details

### 1. Domain Layer Changes

#### Added `Order` Property
- **Collection.cs**: Added `public int Order { get; set; }` property for maintaining sort order
- **Request.cs**: Added `public int Order { get; set; }` property for maintaining sort order

These properties enable persistent ordering of items within their parent containers.

### 2. Application Layer Changes

#### Service Interfaces
Added new methods to service interfaces:

**ICollectionService.cs**:
```csharp
Task MoveCollectionAsync(Guid collectionId, Guid? newParentCollectionId, int newOrder);
```

**IRequestService.cs**:
```csharp
Task MoveRequestAsync(Guid requestId, Guid? newCollectionId, int newOrder);
```

#### Service Implementations

**CollectionService.cs**:
- Implemented `MoveCollectionAsync` to handle moving collections between parents
- Added `ReorderCollectionsInParentAsync` private method to normalize sibling order values
- Ensures all siblings have sequential order values (0, 1, 2, ...)

**RequestService.cs**:
- Implemented `MoveRequestAsync` to handle moving requests between collections
- Added `ReorderRequestsInCollectionAsync` private method to normalize sibling order values
- Ensures all siblings have sequential order values (0, 1, 2, ...)

**CollectionHierarchyHelper.cs**:
- Updated `BuildHierarchy` to sort collections by `Order` then `CreatedAt`
- Updated `PopulateRequests` to sort requests by `Order` then `CreatedAt`
- Ensures the tree view displays items in the correct order

### 3. Presentation Layer Changes

#### New Model
**DraggableItem.cs**:
- Created model to represent draggable items in the tree
- Contains: `Id`, `Name`, `Type` (Collection or Request), `ParentId`, `Order`, `Item` (the actual object)

#### Component Updates

**CollectionTreeItem.razor**:
- Added HTML5 drag-and-drop event handlers to collection and request elements
- Added `draggable="true"` attribute to make items draggable
- Implemented event handlers: `ondragstart`, `ondragenter`, `ondragleave`, `ondrop`, `ondragend`, `ondragover:preventDefault`
- Added parameters for drag event callbacks to propagate events up the component tree
- Created helper methods `CreateDraggableCollection()` and `CreateDraggableRequest()`

**CollectionView.razor**:
- Added drag-and-drop support for top-level collection requests
- Implemented drag event handlers:
  - `HandleDragStart()`: Tracks the item being dragged
  - `HandleDragEnd()`: Cleans up drag state
  - `HandleDragEnter()`: Tracks which item is being hovered over
  - `HandleDragLeave()`: Clears hover state
  - `HandleDrop()`: Handles the drop logic based on item types
- Drop logic supports:
  - Moving collections into other collections (becomes child)
  - Moving requests into collections
  - Reordering requests by dropping on other requests
- Prevents dropping items on themselves
- Reloads data after successful moves
- Shows success/error messages via Snackbar

**app.css**:
Added CSS styles for visual feedback:
```css
.draggable-item {
    cursor: grab;
    transition: background-color 0.2s, opacity 0.2s;
}

.draggable-item:active {
    cursor: grabbing;
}

.draggable-item[draggable="true"]:hover {
    background-color: rgba(var(--mud-palette-action-default-hover-rgb), 0.08);
}
```

### 4. Testing

#### New Test Files
1. **CollectionServiceOrderTests.cs** (4 tests):
   - `MoveCollectionAsync_WithValidData_ShouldUpdateCollectionParentAndOrder`
   - `MoveCollectionAsync_WithNullNewParent_ShouldMoveToRoot`
   - `MoveCollectionAsync_WithNonExistentCollection_ShouldThrowException`
   - `MoveCollectionAsync_ShouldReorderSiblings`

2. **RequestServiceOrderTests.cs** (4 tests):
   - `MoveRequestAsync_WithValidData_ShouldUpdateRequestCollectionAndOrder`
   - `MoveRequestAsync_WithNullNewCollection_ShouldMoveToNoCollection`
   - `MoveRequestAsync_WithNonExistentRequest_ShouldThrowException`
   - `MoveRequestAsync_ShouldReorderSiblings`

#### Test Results
- **Total Tests**: 376 (113 domain + 263 application)
- **Result**: All tests passing ✅

## Usage

### How to Use Drag and Drop

1. **Move a Request into a Collection**:
   - Click and drag a request
   - Drop it onto a collection (folder icon)
   - The request will be moved into that collection

2. **Move a Collection into Another Collection**:
   - Click and drag a collection
   - Drop it onto another collection
   - The dragged collection becomes a subcollection of the target

3. **Reorder Requests**:
   - Click and drag a request
   - Drop it onto another request in the same or different collection
   - The request will be positioned after the target request

### Visual Feedback
- Hovering over draggable items shows a subtle background highlight
- Cursor changes to "grab" when hovering over draggable items
- Cursor changes to "grabbing" while dragging
- Success messages appear after successful moves
- Error messages appear if something goes wrong

## Architecture Compliance

This implementation follows HolyConnect's clean architecture principles:

1. **Domain Layer**: Pure entities with no dependencies, just added Order property
2. **Application Layer**: Business logic in services, interfaces define contracts
3. **Infrastructure Layer**: No changes needed (uses existing repositories)
4. **Presentation Layer**: UI logic in Blazor components, calls application services

## Known Limitations

1. **No Visual Drop Zone Indicators**: Currently, there's no visual indicator showing where an item will be dropped. This could be enhanced in the future.

2. **Order Normalization**: When items are moved, all siblings get their order values normalized to sequential integers (0, 1, 2, ...). This ensures consistency but means the exact `newOrder` parameter passed to move methods may not be preserved.

3. **Single Item Drag**: Only one item can be dragged at a time. Multi-select drag is not supported.

4. **No Undo**: There's no undo functionality for drag-and-drop operations. Users must manually move items back if needed.

5. **Touch Devices**: HTML5 drag-and-drop may have limited support on touch devices. Consider adding touch event handlers for mobile support in the future.

## Future Enhancements

1. Add visual drop zone indicators during drag
2. Add confirmation dialogs for complex moves
3. Implement multi-select drag and drop
4. Add touch device support
5. Add drag-and-drop for top-level collections in the home view
6. Add animation during reordering
7. Implement drag-and-drop between different collection views

## Backward Compatibility

- Existing collections and requests without Order property will default to Order = 0
- The hierarchy helper sorts by Order first, then CreatedAt, maintaining existing order for items with the same Order value
- No database migration needed as the Order property is a simple integer with default value

## Performance Considerations

- Order normalization happens after each move, touching all siblings
- For collections with many items, this could result in multiple database updates
- Consider batching updates in the future for better performance
- Tree reload after drop ensures UI consistency but causes a full data refresh
