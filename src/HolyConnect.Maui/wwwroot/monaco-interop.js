// Monaco Editor Interop for Blazor
window.monacoEditorInterop = {
    editors: {},
    completionProviders: {},

    // Initialize Monaco Editor
    initializeEditor: function (editorId, initialValue, language, theme, readOnly) {
        try {
            const container = document.getElementById(editorId);
            if (!container) {
                console.error('Container not found:', editorId);
                return false;
            }

            // Dispose existing editor if any
            if (this.editors[editorId]) {
                this.editors[editorId].dispose();
            }

            // Create editor with improved options
            const editor = monaco.editor.create(container, {
                value: initialValue || '',
                language: language || 'graphql',
                theme: theme || 'vs-dark',
                readOnly: readOnly || false,
                automaticLayout: true,
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                fontSize: 14,
                lineNumbers: 'on',
                renderLineHighlight: 'all',
                contextmenu: true,
                quickSuggestions: false,
                suggestOnTriggerCharacters: false,
                acceptSuggestionOnEnter: 'on',
                tabCompletion: 'on',
                wordBasedSuggestions: false,
                padding: { top: 8, bottom: 8 },
                lineDecorationsWidth: 10,
                lineNumbersMinChars: 3,
                glyphMargin: false,
                folding: true,
                renderWhitespace: 'selection',
                cursorBlinking: 'smooth',
                smoothScrolling: true,
                scrollbar: {
                    useShadows: false,
                    verticalScrollbarSize: 10,
                    horizontalScrollbarSize: 10
                }
            });

            this.editors[editorId] = editor;

            return true;
        } catch (error) {
            console.error('Error initializing Monaco Editor:', error);
            return false;
        }
    },

    // Get editor value
    getValue: function (editorId) {
        const editor = this.editors[editorId];
        if (editor) {
            return editor.getValue();
        }
        return null;
    },

    // Set editor value
    setValue: function (editorId, value) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.setValue(value || '');
        }
    },

    // Set editor language
    setLanguage: function (editorId, language) {
        const editor = this.editors[editorId];
        if (editor) {
            const model = editor.getModel();
            if (model) {
                monaco.editor.setModelLanguage(model, language);
            }
        }
    },

    // Update editor options
    updateOptions: function (editorId, options) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.updateOptions(options);
        }
    },

    // Register completion provider for GraphQL
    registerGraphQLCompletionProvider: function (editorId, schemaJson) {
        try {
            // Dispose existing provider if any
            if (this.completionProviders[editorId]) {
                this.completionProviders[editorId].dispose();
                delete this.completionProviders[editorId];
            }

            if (!schemaJson) {
                return false;
            }

            const schema = JSON.parse(schemaJson);
            
            // Extract types and fields from schema
            const suggestions = this.buildSuggestionsFromSchema(schema);

            // Register completion provider
            const provider = monaco.languages.registerCompletionItemProvider('graphql', {
                triggerCharacters: ['{', ' ', '\n', '.'],
                provideCompletionItems: (model, position) => {
                    const word = model.getWordUntilPosition(position);
                    const range = {
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn
                    };

                    return {
                        suggestions: suggestions.map(s => ({
                            label: s.label,
                            kind: s.kind,
                            insertText: s.insertText,
                            detail: s.detail,
                            documentation: s.documentation,
                            range: range
                        }))
                    };
                }
            });

            this.completionProviders[editorId] = provider;
            return true;
        } catch (error) {
            console.error('Error registering completion provider:', error);
            return false;
        }
    },

    // Build suggestions from GraphQL schema
    buildSuggestionsFromSchema: function (schema) {
        const suggestions = [];

        if (!schema || !schema.data || !schema.data.__schema) {
            return suggestions;
        }

        const schemaData = schema.data.__schema;
        const types = schemaData.types || [];

        // Add query type fields
        if (schemaData.queryType) {
            const queryType = types.find(t => t.name === schemaData.queryType.name);
            if (queryType && queryType.fields) {
                queryType.fields.forEach(field => {
                    suggestions.push({
                        label: field.name,
                        kind: monaco.languages.CompletionItemKind.Field,
                        insertText: this.buildFieldSnippet(field),
                        detail: this.getFieldType(field.type),
                        documentation: field.description || ''
                    });
                });
            }
        }

        // Add mutation type fields
        if (schemaData.mutationType) {
            const mutationType = types.find(t => t.name === schemaData.mutationType.name);
            if (mutationType && mutationType.fields) {
                mutationType.fields.forEach(field => {
                    suggestions.push({
                        label: field.name,
                        kind: monaco.languages.CompletionItemKind.Method,
                        insertText: this.buildFieldSnippet(field),
                        detail: this.getFieldType(field.type),
                        documentation: field.description || ''
                    });
                });
            }
        }

        // Add subscription type fields
        if (schemaData.subscriptionType) {
            const subscriptionType = types.find(t => t.name === schemaData.subscriptionType.name);
            if (subscriptionType && subscriptionType.fields) {
                subscriptionType.fields.forEach(field => {
                    suggestions.push({
                        label: field.name,
                        kind: monaco.languages.CompletionItemKind.Event,
                        insertText: this.buildFieldSnippet(field),
                        detail: this.getFieldType(field.type),
                        documentation: field.description || ''
                    });
                });
            }
        }

        // Add type fields for all object types
        types.forEach(type => {
            if (type.kind === 'OBJECT' && type.fields) {
                type.fields.forEach(field => {
                    suggestions.push({
                        label: `${type.name}.${field.name}`,
                        kind: monaco.languages.CompletionItemKind.Property,
                        insertText: field.name,
                        detail: this.getFieldType(field.type),
                        documentation: field.description || `Field from ${type.name}`
                    });
                });
            }
        });

        return suggestions;
    },

    // Build field snippet with arguments
    buildFieldSnippet: function (field) {
        if (!field.args || field.args.length === 0) {
            return field.name;
        }

        const args = field.args.map((arg, index) => {
            const argType = this.getFieldType(arg.type);
            return `${arg.name}: \${${index + 1}:${argType}}`;
        }).join(', ');

        return `${field.name}(${args})`;
    },

    // Get field type as string
    getFieldType: function (type) {
        if (!type) return '';
        
        if (type.kind === 'NON_NULL') {
            return this.getFieldType(type.ofType) + '!';
        } else if (type.kind === 'LIST') {
            return '[' + this.getFieldType(type.ofType) + ']';
        } else {
            return type.name || '';
        }
    },

    // Register value change callback
    onValueChanged: function (editorId, dotNetHelper) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.onDidChangeModelContent(() => {
                const value = editor.getValue();
                dotNetHelper.invokeMethodAsync('OnValueChanged', value);
            });
        }
    },

    // Dispose editor
    disposeEditor: function (editorId) {
        if (this.editors[editorId]) {
            this.editors[editorId].dispose();
            delete this.editors[editorId];
        }

        if (this.completionProviders[editorId]) {
            this.completionProviders[editorId].dispose();
            delete this.completionProviders[editorId];
        }
    },

    // Trigger suggestions
    triggerSuggest: function (editorId) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.trigger('keyboard', 'editor.action.triggerSuggest', {});
        }
    },

    // Format document
    formatDocument: function (editorId) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.getAction('editor.action.formatDocument').run();
        }
    },

    // Initialize Monaco Diff Editor
    initializeDiffEditor: function (editorId, originalContent, modifiedContent, language, theme, readOnly) {
        try {
            const container = document.getElementById(editorId);
            if (!container) {
                console.error('Container not found:', editorId);
                return false;
            }

            // Dispose existing editor if any
            if (this.editors[editorId]) {
                this.editors[editorId].dispose();
            }

            // Create diff editor
            const diffEditor = monaco.editor.createDiffEditor(container, {
                theme: theme || 'vs-dark',
                readOnly: readOnly || true,
                automaticLayout: true,
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                fontSize: 14,
                lineNumbers: 'on',
                renderLineHighlight: 'all',
                contextmenu: true,
                padding: { top: 8, bottom: 8 },
                lineDecorationsWidth: 10,
                lineNumbersMinChars: 3,
                glyphMargin: false,
                folding: true,
                renderWhitespace: 'selection',
                cursorBlinking: 'smooth',
                smoothScrolling: true,
                scrollbar: {
                    useShadows: false,
                    verticalScrollbarSize: 10,
                    horizontalScrollbarSize: 10
                },
                renderSideBySide: true,
                ignoreTrimWhitespace: false,
                enableSplitViewResizing: true
            });

            // Set models
            const originalModel = monaco.editor.createModel(originalContent || '', language || 'plaintext');
            const modifiedModel = monaco.editor.createModel(modifiedContent || '', language || 'plaintext');

            diffEditor.setModel({
                original: originalModel,
                modified: modifiedModel
            });

            this.editors[editorId] = diffEditor;

            return true;
        } catch (error) {
            console.error('Error initializing Monaco Diff Editor:', error);
            return false;
        }
    },

    // Update diff editor content
    updateDiffEditor: function (editorId, originalContent, modifiedContent, language) {
        try {
            const diffEditor = this.editors[editorId];
            if (!diffEditor || !diffEditor.getOriginalEditor) {
                return false;
            }

            const originalModel = diffEditor.getOriginalEditor().getModel();
            const modifiedModel = diffEditor.getModifiedEditor().getModel();

            if (originalModel && modifiedModel) {
                // Update content
                originalModel.setValue(originalContent || '');
                modifiedModel.setValue(modifiedContent || '');

                // Update language if provided
                if (language) {
                    monaco.editor.setModelLanguage(originalModel, language);
                    monaco.editor.setModelLanguage(modifiedModel, language);
                }

                return true;
            }

            return false;
        } catch (error) {
            console.error('Error updating Monaco Diff Editor:', error);
            return false;
        }
    }
};
