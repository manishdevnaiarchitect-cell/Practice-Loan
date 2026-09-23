---

name: dotnet-coding
description: Apply consistent coding practices when developing or modifying .NET and C# application code.
---

# .NET Coding Skill

Use this skill when creating, modifying, refactoring, or reviewing .NET/C# code.

## Coding Guidelines

* Use modern C# features where they improve readability and maintainability.
* Follow SOLID principles and keep classes focused on a single responsibility.
* Prefer dependency injection over creating dependencies directly inside classes.
* Keep controllers thin; business logic should reside in service classes.
* Use interfaces when they provide clear abstraction or enable testability.
* Use `async`/`await` for I/O-bound operations.
* Avoid unnecessary complexity and duplicated code.
* Follow consistent naming conventions:

  * PascalCase for classes, methods, and public members.
  * camelCase for local variables and parameters.
* Prefer meaningful names over abbreviations.
* Handle exceptions appropriately and avoid swallowing exceptions.

## API Development

When creating or modifying APIs:

* Validate incoming request data.
* Use DTOs for API request and response models where appropriate.
* Return appropriate HTTP status codes.
* Keep API controllers focused on HTTP concerns.
* Do not expose internal implementation details through API responses.
* Do not hardcode secrets, passwords, connection strings, or API keys.

## Testing

When modifying application logic:

* Add or update unit tests for the changed behavior.
* Follow the Arrange-Act-Assert pattern.
* Include positive and negative scenarios.
* Mock external dependencies where appropriate.
* Do not modify tests simply to make failing tests pass unless the test itself is incorrect.

## Before Completing a Task

After making code changes:

1. Review the modified code for consistency with the existing architecture.
2. Check for compilation errors.
3. Run relevant unit tests.
4. Fix issues identified by the tests.
5. Summarize the changes made and the tests executed.
