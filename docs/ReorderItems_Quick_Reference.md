# Reorder Items - Quick Reference

## 🚀 Quick Start

### Access
**Navigation:** Main Menu → "Reorder Items" (or navigate to `/reorder`)

### Overview
The Reorder Items page allows you to reorganize your collections, sub-collections, and requests without drag-and-drop.

## 🔍 Search

**How to Search:**
1. Type in the search box at the top
2. Results filter automatically (300ms delay)
3. Click X or clear the box to see all items

**What You Can Search:**
- Item names
- Request URLs  
- Collection paths

## 📤 Move Items

### Move a Collection

1. **Find** the collection you want to move
2. **Click** the "Move to..." icon (📤)
3. **Select** destination:
   - "Root Level" - Make it a top-level collection
   - Or choose a parent collection from the list
4. **Click** "Move Here"
5. ✅ Done! The collection moves with all its contents

**Note:** You cannot move a collection into itself or its descendants.

### Move a Request

1. **Find** the request you want to move
2. **Click** the "Move to..." icon (📤)
3. **Select** destination:
   - "Root Level" - Remove from all collections
   - Or choose a collection from the list
4. **Click** "Move Here"
5. ✅ Done! The request moves instantly

## 🚫 Remove from Collection

### Remove a Request from its Collection

1. **Find** the request in the list
2. **Click** the "Remove from collection" icon (🚫)
3. **Confirm** the action in the dialog
4. ✅ Done! Request is orphaned (not in any collection)

**Note:** This button is only enabled for requests that are in a collection.

## 🎨 Visual Guide

### Icons

| Icon | Type |
|------|------|
| 📁 | Collection / Sub-Collection |
| 🌐 | REST Request |
| 📊 | GraphQL Request |
| 🔌 | WebSocket Request |

### Colors

| Color | Type |
|-------|------|
| 🔵 Blue | Collections |
| 🟣 Purple | REST Requests |
| 🟢 Green | GraphQL Requests |
| 🟠 Orange | WebSocket Requests |

### Badges

Each item shows a badge indicating its type:
- "Collection" - Top-level collection
- "Sub-Collection" - Collection inside another collection
- "REST Request" - HTTP REST API request
- "GraphQL Request" - GraphQL query/mutation
- "WebSocket Request" - WebSocket connection

### Location

Each item shows its current location:
- **Collections:** "Root level" or "Parent: Collection Name"
- **Requests:** "In: Collection / Sub-Collection" or "Not in any collection"

## ⚡ Tips & Tricks

### Efficient Navigation

1. **Use Search** - Quickly find items by typing part of their name
2. **Clear Search** - Click the X button to see all items again
3. **Read Locations** - Full paths help you understand hierarchy
4. **Check Badges** - Quickly identify item types

### Best Practices

1. **Plan Before Moving** - Know where you want to move items
2. **Use Descriptive Names** - Makes searching easier
3. **Organize Logically** - Group related items in collections
4. **Keep It Flat** - Don't nest too deeply (3-4 levels max recommended)

### Common Workflows

**Reorganize a Project:**
1. Create new collections for organization
2. Use search to find related items
3. Move items to appropriate collections
4. Remove orphaned requests from old collections

**Clean Up Orphaned Requests:**
1. Search for requests
2. Look for "Not in any collection"
3. Move to appropriate collections

**Flatten Hierarchy:**
1. Find deeply nested sub-collections
2. Move to root level or higher-level collection
3. Reorganize as needed

## ❗ Important Notes

### Circular References
You **cannot** move a collection into one of its own descendants. The system prevents this automatically.

**Example:**
```
📁 API Tests
  └─ 📁 Authentication
      └─ 📁 OAuth

❌ You cannot move "API Tests" into "OAuth"
✅ You can move "OAuth" into "API Tests" or root
```

### Item Counts
The page shows total counts at the top:
- 📁 X Collections
- ✓ Y Requests

This helps you keep track of your organization.

### Refresh
Click the **Refresh** button (top right) to reload all data if something seems out of sync.

## 🆘 Troubleshooting

### "No items found"
**Cause:** No collections or requests exist
**Solution:** Create collections and requests first

### "No items match your search"
**Cause:** Search term doesn't match any items
**Solution:** Try different search terms or clear the search

### "Error moving item"
**Cause:** Could be various reasons
**Solution:** 
1. Try refreshing the page
2. Check if the destination still exists
3. Check if you're trying to create a circular reference

### Move button disabled
**Cause:** For collections, trying to move to invalid destination
**Solution:** Select a valid destination that doesn't create circular reference

### Remove button disabled
**Cause:** Request is not in any collection
**Solution:** This is expected - nothing to remove from

## 📚 Related Documentation

For more details, see:
- **Feature Documentation:** `docs/ReorderItems.md`
- **Visual Guide:** `docs/ReorderItems_Visual_Guide.md`
- **Implementation:** `docs/ReorderItems_Implementation_Summary.md`

## 🎯 Example Scenarios

### Scenario 1: Move Request to Different Collection

**Starting State:**
- Request "GET User" is in "Collection A"
- Want to move to "Collection B"

**Steps:**
1. Search for "GET User"
2. Click 📤 icon
3. Select "Collection B"
4. Click "Move Here"
5. ✅ "GET User" is now in "Collection B"

### Scenario 2: Create Sub-Collection

**Starting State:**
- Have "API Tests" collection
- Have "E-commerce" collection
- Want "E-commerce" under "API Tests"

**Steps:**
1. Find "E-commerce" in the list
2. Click 📤 icon
3. Select "API Tests"
4. Click "Move Here"
5. ✅ "E-commerce" is now a sub-collection of "API Tests"

### Scenario 3: Orphan a Request

**Starting State:**
- Request "Test Request" is in a collection
- Want to remove it from all collections

**Steps:**
1. Find "Test Request"
2. Click 🚫 icon (Remove from collection)
3. Confirm
4. ✅ Request is now orphaned

**Alternative:**
1. Find "Test Request"
2. Click 📤 icon (Move to...)
3. Select "Root Level"
4. Click "Move Here"
5. ✅ Same result - request is orphaned

## ✨ Summary

The Reorder Items feature provides a simple, intuitive way to organize your API testing workspace. Use search to find items quickly, move them with clear dialogs, and keep your collections organized efficiently.

**Key Benefits:**
- ✅ No drag-and-drop needed (keyboard/mouse friendly)
- ✅ Clear visual feedback
- ✅ Fast search and filter
- ✅ Handles complex hierarchies
- ✅ Safe (prevents mistakes)

**Questions?** See the detailed documentation in the `docs/` folder.
