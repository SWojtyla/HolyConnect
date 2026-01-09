// Keyboard shortcuts handler for HolyConnect

window.keyboardShortcuts = {
    dotnetReference: null,
    
    initialize: function(dotnetRef) {
        this.dotnetReference = dotnetRef;
        
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
        console.log('Keyboard shortcuts initialized');
    },
    
    handleKeyDown: function(event) {
        // Don't trigger shortcuts when typing in input fields (except for Ctrl+K)
        const target = event.target;
        const isInputField = target.tagName === 'INPUT' || 
                            target.tagName === 'TEXTAREA' || 
                            target.contentEditable === 'true';
        
        // Get the key and modifiers
        const key = event.key.toLowerCase();
        const ctrlKey = event.ctrlKey || event.metaKey; // metaKey for Mac Cmd
        const shiftKey = event.shiftKey;
        const altKey = event.altKey;
        
        // Always allow Ctrl+K to open search, even in input fields
        if (ctrlKey && key === 'k') {
            event.preventDefault();
            this.dotnetReference.invokeMethodAsync('HandleKeyPress', key, ctrlKey, shiftKey, altKey);
            return;
        }
        
        // For other shortcuts, skip if in input field
        if (isInputField) {
            return;
        }
        
        // Allow ? key to open shortcuts
        if (key === '?' && !ctrlKey && !shiftKey && !altKey) {
            event.preventDefault();
            this.dotnetReference.invokeMethodAsync('HandleKeyPress', key, ctrlKey, shiftKey, altKey);
            return;
        }
        
        // Handle other shortcuts with modifiers
        if (ctrlKey || altKey) {
            this.dotnetReference.invokeMethodAsync('HandleKeyPress', key, ctrlKey, shiftKey, altKey)
                .then(handled => {
                    if (handled) {
                        event.preventDefault();
                    }
                });
        }
    },
    
    dispose: function() {
        document.removeEventListener('keydown', this.handleKeyDown.bind(this));
        this.dotnetReference = null;
        console.log('Keyboard shortcuts disposed');
    }
};
