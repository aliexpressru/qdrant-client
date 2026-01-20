## General

* Make only high confidence suggestions when reviewing code changes.
* Always use the latest C# version, unless explicitly specified in csproj.
* Follow C# coding conventions as per Microsoft documentation: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
* Use C# best practices and idiomatic constructs.
* Use latest stable .NET SDK.
* Use latest .NET SDK features and APIs unless explicitly specified otherwise.
* When emitting `Dispose` patterns emit `DisposeAsync` when possible.
* Always use spaces for indentation. Use 4 spaces per indentation level.

## Formatting

* Apply code-formatting style defined in `.editorconfig`.
* Prefer file-scoped namespace declarations and single-line using directives.
* Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
* Ensure that the final return statement of a method is on its own line.
* Use pattern matching and switch expressions wherever possible.
* Use `nameof` instead of string literals when referring to member names.
* Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments. Use `<inheritdoc />` instead of rewriting comment of parent type's member.
* Always end sentences in XML doc comments with a period.
* Always use braces `{}` for all control flow statements, even if they are optional.
* Format ternary expressions with new lines before the `?` and `:` operators. Align the `:` with the `?`.
* Use expression-bodied members for properties and methods where appropriate.
* Use `var` when the type is apparent from the right side of the assignment; otherwise, use explicit types.
* Use `async` and `await` for asynchronous programming. Avoid blocking calls.
* Use `ConfigureAwait(false)` in library code when awaiting tasks.
* Keep one C# type per file, and name the file after the type.

### Nullable Reference Types

* Declare variables non-nullable, and check for `null` at entry points.
* Always use `is null` or `is not null` instead of `== null` or `!= null`.
* Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

### Testing

* Use NUnit SDK for tests.
* Use FluentAssertions for assertions.
* Do not emit "Act", "Arrange" or "Assert" comments.
* Copy existing style in nearby files for test method names and capitalization.
* Implement `Awaiting` and `Invoking` assertions by firstly emitting a local variable with func to call and assert and then using assrtions on that func call. Always await `ThrowAsync` calls.
