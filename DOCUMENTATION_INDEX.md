# HolyConnect Documentation Index

## For Developers Using GitHub Copilot

### 🚀 Quick Start
New to the codebase? Start here:
1. **[Copilot Instructions](.github/copilot-instructions.md)** - Complete development guidelines (read first!)
2. **[Quick Reference](.github/QUICK_REFERENCE.md)** - Common tasks and code snippets
3. **[Architecture](ARCHITECTURE.md)** - Understand the system design

### 📋 By Task

#### Building a New Feature
1. Check **[Quick Reference](.github/QUICK_REFERENCE.md)** for similar patterns
2. Review **[Architecture](ARCHITECTURE.md)** for layer responsibilities
3. Follow patterns in **[Copilot Instructions](.github/copilot-instructions.md)**
4. See **[Component Library](.github/COMPONENT_LIBRARY.md)** for reusable components
5. Add navigation in **[UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md)**

#### Creating a New Page
1. **[UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md)** - Routing patterns and navigation
2. **[Quick Reference](.github/QUICK_REFERENCE.md#creating-a-new-page)** - Step-by-step guide
3. **[Component Library](.github/COMPONENT_LIBRARY.md)** - Available UI components

#### Working with UI Components
1. **[Component Library](.github/COMPONENT_LIBRARY.md)** - All reusable components
2. **[Copilot Mistakes](.github/copilot-mistakes.md)** - Common UI pitfalls to avoid
3. **[Quick Reference](.github/QUICK_REFERENCE.md)** - Component usage examples

#### Fixing Bugs
1. **[Copilot Mistakes](.github/copilot-mistakes.md)** - Known issues and solutions
2. **[Copilot Instructions](.github/copilot-instructions.md#when-fixing-bugs)** - Bug fix workflow
3. **[Quick Reference](.github/QUICK_REFERENCE.md)** - Testing and debugging snippets

#### Adding a Service
1. **[Quick Reference](.github/QUICK_REFERENCE.md#adding-a-service)** - Service creation guide
2. **[Architecture](ARCHITECTURE.md)** - Service layer responsibilities
3. **[Copilot Instructions](.github/copilot-instructions.md#dependency-injection-registration)** - DI registration

#### Working with Requests
1. **[Quick Reference](.github/QUICK_REFERENCE.md#adding-a-new-request-type)** - Request type implementation
2. **[Architecture](ARCHITECTURE.md)** - Request execution architecture
3. **[Copilot Instructions](.github/copilot-instructions.md#request-execution-pattern)** - Request executor patterns

### 📚 By Topic

#### Architecture & Design
- **[Architecture](ARCHITECTURE.md)** - System architecture and layer responsibilities
- **[Copilot Instructions](.github/copilot-instructions.md#architecture-principles)** - Clean architecture principles
- **[Contributing](CONTRIBUTING.md)** - Development workflow and guidelines

#### UI & Navigation
- **[UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md)** - Complete navigation reference
- **[Component Library](.github/COMPONENT_LIBRARY.md)** - All UI components
- **[Copilot Instructions](.github/copilot-instructions.md#page-to-route-mapping)** - Route mapping table

#### Features
- **[Flows Feature](docs/FLOWS_FEATURE.md)** - Sequential request execution
- **[Bruno Import](docs/BRUNO_IMPORT.md)** - Import from Bruno API client
- **[Variables Wiki](src/HolyConnect.Maui/Components/Pages/Docs/VariablesWiki.razor)** - Variable syntax guide

#### Testing
- **[Copilot Instructions](.github/copilot-instructions.md#testing-requirements)** - Testing standards
- **[Quick Reference](.github/QUICK_REFERENCE.md#testing-quick-reference)** - Test commands
- **[Contributing](CONTRIBUTING.md)** - Test workflow

#### Common Patterns
- **[Quick Reference](.github/QUICK_REFERENCE.md#common-code-snippets)** - Reusable snippets
- **[Copilot Instructions](.github/copilot-instructions.md#common-patterns-used)** - Established patterns
- **[Copilot Mistakes](.github/copilot-mistakes.md)** - What NOT to do

### 🎯 By Role

#### First-Time Contributor
1. **[README](README.md)** - Project overview
2. **[Contributing](CONTRIBUTING.md)** - How to contribute
3. **[Architecture](ARCHITECTURE.md)** - System design
4. **[Copilot Instructions](.github/copilot-instructions.md)** - Development guidelines

#### UI Developer
1. **[Component Library](.github/COMPONENT_LIBRARY.md)** - All components
2. **[UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md)** - Navigation patterns
3. **[Copilot Mistakes](.github/copilot-mistakes.md)** - UI pitfalls
4. **[Quick Reference](.github/QUICK_REFERENCE.md)** - Common UI tasks

#### Backend Developer
1. **[Architecture](ARCHITECTURE.md)** - Layer structure
2. **[Copilot Instructions](.github/copilot-instructions.md)** - Service patterns
3. **[Quick Reference](.github/QUICK_REFERENCE.md)** - Service implementation
4. **[Contributing](CONTRIBUTING.md)** - Testing requirements

#### Feature Developer
1. **[Quick Reference](.github/QUICK_REFERENCE.md)** - Implementation guides
2. **[Copilot Instructions](.github/copilot-instructions.md)** - Development workflow
3. **[Architecture](ARCHITECTURE.md)** - System design
4. **[Flows Feature](docs/FLOWS_FEATURE.md)** - Example feature

### 📖 All Documentation Files

#### Core Documentation
- **[README](README.md)** - Project overview, features, getting started
- **[Architecture](ARCHITECTURE.md)** - System architecture and design
- **[Contributing](CONTRIBUTING.md)** - Development workflow and guidelines
- **[LICENSE](LICENSE)** - MIT License

#### Copilot Documentation (`.github/`)
- **[copilot-instructions.md](.github/copilot-instructions.md)** - Complete development guidelines ⭐
- **[copilot-mistakes.md](.github/copilot-mistakes.md)** - Common mistakes to avoid
- **[QUICK_REFERENCE.md](.github/QUICK_REFERENCE.md)** - Quick task reference ⭐
- **[UI_NAVIGATION_GUIDE.md](.github/UI_NAVIGATION_GUIDE.md)** - Navigation and routing ⭐
- **[COMPONENT_LIBRARY.md](.github/COMPONENT_LIBRARY.md)** - Component documentation ⭐

#### Feature Documentation (`docs/`)
- **[FLOWS_FEATURE.md](docs/FLOWS_FEATURE.md)** - Flows feature documentation
- **[BRUNO_IMPORT.md](docs/BRUNO_IMPORT.md)** - Import functionality
- **[FLOWS_UI_GUIDE.md](docs/FLOWS_UI_GUIDE.md)** - Flows UI guide
- **[REFACTORING_SUMMARY.md](docs/REFACTORING_SUMMARY.md)** - Refactoring notes

#### Legacy/Project Documentation (Root)
- **[FLOWS_IMPLEMENTATION_SUMMARY.md](FLOWS_IMPLEMENTATION_SUMMARY.md)** - Flows implementation
- **[BRUNO_IMPORT_IMPLEMENTATION.md](BRUNO_IMPORT_IMPLEMENTATION.md)** - Bruno import details
- **[BACKEND_ARCHITECTURE_IMPROVEMENTS.md](BACKEND_ARCHITECTURE_IMPROVEMENTS.md)** - Backend notes
- **[UI_REFACTORING_INDEX.md](UI_REFACTORING_INDEX.md)** - UI refactoring index
- **[UI_REFACTORING_PLAN.md](UI_REFACTORING_PLAN.md)** - UI refactoring plan
- **[UI_REFACTORING_QUICK_START.md](UI_REFACTORING_QUICK_START.md)** - UI refactoring guide
- **[UI_REFACTORING_VISUAL_GUIDE.md](UI_REFACTORING_VISUAL_GUIDE.md)** - Visual guide
- **[REFACTORING_PROGRESS.md](REFACTORING_PROGRESS.md)** - Refactoring progress

### 🔍 Search Tips

**Find by keyword**:
```bash
# Search all documentation
grep -r "keyword" *.md .github/*.md docs/*.md

# Search code
grep -r "pattern" src/

# Find files
find . -name "*pattern*"
```

**Common searches**:
- "navigation" → See UI Navigation Guide
- "component" → See Component Library
- "service" → See Quick Reference or Copilot Instructions
- "test" → See Copilot Instructions or Contributing
- "dialog" → See Component Library and Copilot Mistakes
- "request" → See Quick Reference or Architecture

### 🔗 External References

- **[MudBlazor Documentation](https://mudblazor.com/)** - UI component library
- **[.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)** - MAUI framework
- **[Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)** - Blazor framework
- **[xUnit Documentation](https://xunit.net/)** - Testing framework
- **[Moq Documentation](https://github.com/moq/moq4)** - Mocking library

### 📝 Keeping Documentation Updated

When making changes, update relevant documentation:

| Change Type | Update These Files |
|-------------|-------------------|
| New feature | README, Architecture, Copilot Instructions, Quick Reference |
| New page | UI Navigation Guide, Copilot Instructions |
| New component | Component Library, Copilot Instructions |
| Bug fix (common pattern) | Copilot Mistakes |
| Service pattern change | Quick Reference, Copilot Instructions |
| Build/test change | Contributing, Copilot Instructions |
| Feature documentation | Specific feature doc (e.g., FLOWS_FEATURE.md) |

### 🎓 Learning Path

**Week 1: Understanding the Codebase**
- Day 1-2: Read README, Architecture, Contributing
- Day 3-4: Review Copilot Instructions thoroughly
- Day 5: Explore Component Library and Navigation Guide

**Week 2: Building Confidence**
- Day 1-2: Follow Quick Reference examples
- Day 3-4: Review Copilot Mistakes to avoid common errors
- Day 5: Read feature documentation (Flows, Import)

**Week 3: Contributing**
- Start with small bug fixes
- Use Quick Reference for implementation patterns
- Follow Testing Requirements strictly
- Update documentation as you learn

### 💡 Pro Tips

1. **Bookmark these files**:
   - Quick Reference (most frequently used)
   - Component Library (for UI work)
   - Copilot Instructions (for guidelines)
   - Copilot Mistakes (save time!)

2. **Before asking for help**:
   - Search documentation for keywords
   - Check Copilot Mistakes for known issues
   - Review Quick Reference for examples
   - Look at similar features in the codebase

3. **When stuck**:
   - UI issue? → Component Library + Navigation Guide
   - Service issue? → Quick Reference + Architecture
   - Build error? → Copilot Mistakes + Contributing
   - Test failure? → Copilot Instructions testing section

4. **Contributing documentation**:
   - Found a common pattern? → Add to Quick Reference
   - Fixed a tricky bug? → Document in Copilot Mistakes
   - Built a new feature? → Create feature doc like FLOWS_FEATURE.md
   - Changed navigation? → Update UI Navigation Guide

---

**Last Updated**: 2026-01-10

**Documentation Status**: ✅ Comprehensive (4 new guides added: Quick Reference, UI Navigation, Component Library, this index)

**Feedback**: If documentation is unclear or missing information, please update it! This is a living documentation system.
