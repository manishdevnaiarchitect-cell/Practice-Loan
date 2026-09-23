# Project Instructions

## Technology

* This is a .NET 8 Web API application using C#.
* Use the existing project structure and dependencies.
* Follow the conventions already established in the repository.

## Architecture

* Follow SOLID principles.
* Keep controllers focused on HTTP concerns.
* Put business logic in service classes.
* Use dependency injection for application dependencies.
* Use DTOs for API contracts where appropriate.

## Coding

* Use nullable reference types.
* Prefer async/await for I/O operations.
* Use meaningful names and avoid unnecessary abstractions.
* Do not hardcode secrets, credentials, or connection strings.

## Testing

* Add or update unit tests when changing business logic.
* Follow the existing test framework and conventions.
* Use Arrange-Act-Assert for unit tests.

## Before Completing a Change

* Check for compilation errors.
* Run relevant tests.
* Preserve existing functionality unless the change explicitly requires otherwise.
* Summarize the files changed and tests executed.
