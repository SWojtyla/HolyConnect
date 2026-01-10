# Reorder Items Feature

## Overview
The Reorder Items feature provides a clean, intuitive interface for reorganizing collections, sub-collections, and requests without requiring drag-and-drop gestures. This makes it accessible and efficient, especially when working with large hierarchies.

## Features

### Search and Filter
- **Real-time Search**: Filter items by name, URL, or location with 300ms debouncing
- **Instant Results**: Search results update immediately as you type
- **Clear Indicators**: Shows total counts for collections and requests

### Visual Organization
- **Clear Icons**: Different icons for each item type (Collection, REST, GraphQL, WebSocket)
- **Color Coding**: Visual distinction between item types using MudBlazor colors
- **Location Display**: Shows full collection path for each item
- **Type Badges**: Chips showing whether item is Collection, Sub-Collection, or Request type

### Moving Items

#### Move Collections
- Move collections to root level or under another collection
- **Circular Reference Prevention**: System prevents moving a collection into its own descendants
- Maintains hierarchy integrity automatically

#### Move Requests
- Move requests between collections
- Remove requests from collections (orphan them)
- Requests can be moved to root level (no collection)

### User Interface

#### Main Page (`/reorder`)
Located in the navigation menu with a "Reorder Items" link (SwapVert icon).

**Components:**
1. **Search Bar**: Filter items by name, URL, or location
2. **Stats Display**: Shows total collections and requests
3. **Item List**: All items displayed with:
   - Icon and color coding
   - Item name
   - Type badge (Collection/Sub-Collection/Request type)
   - Location information
   - Action buttons

**Actions:**
- **Move to...**: Opens dialog to select new destination
- **Remove from collection**: (Requests only) Removes from parent collection

#### Move Dialog
- **Radio Selection**: Choose between root level or specific collection
- **Collection List**: All valid destinations shown with full path
- **Smart Filtering**: Shows only valid destinations (prevents circular references)
- **Search**: When more than 5 collections, search filter appears
- **Visual Hierarchy**: Displays full collection path for each option

## Technical Implementation

### Service Layer
Two new methods added to service interfaces:

#### `ICollectionService.MoveCollectionAsync`
```csharp
Task<Collection> MoveCollectionAsync(Guid collectionId, Guid? newParentCollectionId);
```
- Moves collection to new parent (or null for root)
- Validates against circular references
- Throws `InvalidOperationException` if collection not found or circular reference detected

#### `IRequestService.MoveRequestAsync`
```csharp
Task<Request> MoveRequestAsync(Guid requestId, Guid? newCollectionId);
```
- Moves request to new collection (or null to remove from all collections)
- Throws `InvalidOperationException` if request not found

### UI Components

#### `ReorderItems.razor`
Main page component located at `/reorder`
- Displays all collections and requests in a searchable list
- Provides move and remove functionality
- Shows item metadata and location

#### `MoveItemDialog.razor`
Modal dialog for selecting move destination
- Radio group for destination selection
- Filters available collections based on item type
- Prevents invalid moves (circular references)

### Styling and Animations
- **Smooth Transitions**: 0.2s ease-in-out for hover effects
- **Fade-in Animation**: Items appear with subtle animation
- **Hover Effects**: Cards translate and shadow on hover
- **Responsive Design**: Works on different screen sizes

## Usage Examples

### Scenario 1: Reorganize Sub-Collection
1. Navigate to `/reorder`
2. Find the sub-collection in the list
3. Click "Move to..." icon
4. Select new parent collection or root level
5. Click "Move Here"
6. ✅ Sub-collection is moved with all its contents

### Scenario 2: Move Request Between Collections
1. Navigate to `/reorder`
2. Search for the request by name
3. Click "Move to..." icon
4. Select destination collection
5. Click "Move Here"
6. ✅ Request appears in new collection

### Scenario 3: Remove Request from Collection
1. Navigate to `/reorder`
2. Find the request
3. Click "Remove from collection" icon
4. Confirm the action
5. ✅ Request becomes orphaned (not in any collection)

## Performance Considerations

### Scalability
- **Efficient Filtering**: Uses LINQ with immediate evaluation
- **Debounced Search**: 300ms delay prevents excessive filtering
- **Lazy Rendering**: Only visible items are rendered
- **Memory Efficient**: Loads all data once, filters in memory

### Large Datasets
The UI is designed to handle 100+ items:
- Search/filter quickly narrows results
- Visual hierarchy helps locate items
- No drag-and-drop means no performance overhead
- Simple list-based layout scales well

## Testing

### Unit Tests
7 comprehensive tests covering:
- ✅ Move collection with valid destination
- ✅ Move collection to root level
- ✅ Prevent circular references
- ✅ Handle non-existent collections
- ✅ Move request between collections
- ✅ Remove request from collection
- ✅ Handle non-existent requests

All tests passing in `HolyConnect.Application.Tests`

### Manual Testing Checklist
- [ ] Search filters items correctly
- [ ] Move collection to another collection
- [ ] Move collection to root
- [ ] Circular reference prevention works
- [ ] Move request between collections
- [ ] Remove request from collection
- [ ] UI updates after successful move
- [ ] Error messages display correctly
- [ ] Works with 100+ items
- [ ] Works with nested hierarchies (5+ levels deep)

## Future Enhancements

Potential improvements:
1. **Batch Operations**: Select and move multiple items at once
2. **Undo/Redo**: Ability to undo recent moves
3. **Sort Options**: Sort by name, date, type, etc.
4. **Virtualization**: For extremely large datasets (1000+ items)
5. **Keyboard Navigation**: Keyboard shortcuts for common actions
6. **Recent Moves**: History of recent reorganizations
7. **Drag and Drop**: Optional gesture-based reordering for mouse users

## Architecture Compliance

This feature follows HolyConnect's clean architecture principles:
- **Domain Layer**: No changes (entities already support parent/collection relationships)
- **Application Layer**: New methods in services with business logic
- **Infrastructure Layer**: Uses existing repository pattern
- **Presentation Layer**: New Blazor components following MudBlazor patterns

## Related Documentation
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Overall architecture
- [Collections Documentation](Collections.md) - Collection hierarchy details
- [Keyboard Shortcuts](KeyboardShortcuts.md) - Navigation shortcuts
