# Contributing to Bwadl

We love your input! We want to make contributing to this project as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## Development Process

We use GitHub to host code, to track issues and feature requests, as well as accept pull requests.

### Pull Requests

Pull requests are the best way to propose changes to the codebase. We actively welcome your pull requests:

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code lints.
6. Issue that pull request!

### Code Standards

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Maintain Clean Architecture principles
- Keep test coverage above 80%
- Follow SOLID principles

### Commit Messages

- Use clear and meaningful commit messages
- Start with a verb (Add, Fix, Update, Remove, etc.)
- Keep the first line under 50 characters
- Reference issues and pull requests when applicable

Example:
```
Add user validation for email uniqueness

- Implement email uniqueness check in validator
- Add corresponding unit tests
- Update documentation

Fixes #123
```

## Any Contributions You Make Will Be Under the MIT Software License

In short, when you submit code changes, your submissions are understood to be under the same [MIT License](http://choosealicense.com/licenses/mit/) that covers the project.

## Report Bugs Using GitHub Issues

We use GitHub issues to track public bugs. Report a bug by [opening a new issue](https://github.com/yourusername/bwadl/issues).

**Great Bug Reports** tend to have:

- A quick summary and/or background
- Steps to reproduce
  - Be specific!
  - Give sample code if you can
- What you expected would happen
- What actually happens
- Notes (possibly including why you think this might be happening, or stuff you tried that didn't work)

## Feature Requests

We welcome feature requests! Please provide:

- Clear description of the feature
- Use cases and benefits
- Possible implementation approach
- Any related issues or discussions

## Development Setup

1. Clone the repository
2. Install .NET 8 SDK
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet test` to ensure everything works

## Testing

- Write unit tests for new functionality
- Ensure integration tests pass
- Maintain code coverage above 80%
- Test both happy path and error scenarios

## Documentation

- Update README.md for significant changes
- Add XML documentation for public APIs
- Update architecture diagrams if needed
- Include code examples where helpful

## Code of Conduct

### Our Pledge

We pledge to make participation in our project a harassment-free experience for everyone.

### Our Standards

Examples of behavior that contributes to creating a positive environment include:

- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

### Enforcement

Project maintainers have the right and responsibility to remove, edit, or reject comments, commits, code, wiki edits, issues, and other contributions that are not aligned to this Code of Conduct.

## License

By contributing, you agree that your contributions will be licensed under its MIT License.

## References

This document was adapted from the open-source contribution guidelines for [Facebook's Draft](https://github.com/facebook/draft-js/blob/a9316a723f9e918afde44dea68b5f9f39b7d9b00/CONTRIBUTING.md).
