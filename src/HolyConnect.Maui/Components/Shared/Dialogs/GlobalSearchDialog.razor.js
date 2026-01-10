// Dialog-level keyboard handler for GlobalSearchDialog
// Handles Escape key even before the text field receives focus

export function initialize(dotnetRef) {
    // Add keydown listener to the document
    const handleKeyDown = (event) => {
        // Handle Escape key to close the dialog
        if (event.key === 'Escape') {
            event.preventDefault();
            event.stopPropagation();
            dotnetRef.invokeMethodAsync('CloseDialog');
        }
    };
    
    // Store the handler reference so we can remove it later
    document._globalSearchDialogKeyHandler = handleKeyDown;
    document.addEventListener('keydown', handleKeyDown, true);
    
    console.log('GlobalSearchDialog keyboard handler initialized');
}

export function dispose() {
    // Remove the event listener
    if (document._globalSearchDialogKeyHandler) {
        document.removeEventListener('keydown', document._globalSearchDialogKeyHandler, true);
        delete document._globalSearchDialogKeyHandler;
        console.log('GlobalSearchDialog keyboard handler disposed');
    }
}
