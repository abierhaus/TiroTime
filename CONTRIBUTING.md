# Contributing to TiroTime

Thank you for considering contributing to TiroTime! 🎉

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue with:
- A clear, descriptive title
- Steps to reproduce the issue
- Expected vs actual behavior
- Screenshots if applicable
- Your environment (OS, .NET version, browser)

### Suggesting Features

Feature suggestions are welcome! Please:
- Check existing issues first
- Provide a clear use case
- Explain why this would be useful
- Consider implementation complexity

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards**:
   - Use meaningful variable names
   - Add XML documentation for public methods
   - Follow C# naming conventions
   - Keep methods focused and small

3. **Write tests** for new functionality

4. **Ensure your code builds** without warnings:
   ```bash
   dotnet build
   ```

5. **Update documentation** if needed

6. **Create a Pull Request** with:
   - Clear description of changes
   - Reference to related issues
   - Screenshots for UI changes

## Development Setup

See the main [README.md](README.md) for setup instructions.

## Coding Guidelines

### C# Style
- Use PascalCase for public members
- Use camelCase for private fields (with `_` prefix)
- Use `var` when type is obvious
- Prefer expression-bodied members when appropriate

### Architecture
- Keep domain logic in the Domain layer
- Use DTOs for cross-layer communication
- Follow DDD patterns (Entities, Value Objects, Aggregates)
- Don't reference UI from business logic

### Git Commits
- Use present tense ("Add feature" not "Added feature")
- Keep first line under 50 characters
- Reference issues when applicable

## Questions?

Feel free to open a discussion or reach out via issues!

---

Thank you for contributing! 🙏
