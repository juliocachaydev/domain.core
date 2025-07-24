## Overview
A C# library that abstracts a Domain layer: An aggregate, a Repository, a domain event dispatcher, and more.

## License
MIT

## Dependencies
- Net Standard 2.1
- Microsoft.Extensions.DependencyInjection.Abstractions 9.0.0

The reason I split the project this way is to easily add support for other .NET versions without needing to update the core project. The NuGet package exposes the .NET 9 project. You can certainly reimplement the DependencyInjection extension method in your code and use the core project.

## Motivation
After several years of writing domain layers for different apps, I developed a set of strategies to speed development. This package contains some of those strategies.

More on the [Wiki](https://github.com/juliocachaydev/domain.core/wiki)

