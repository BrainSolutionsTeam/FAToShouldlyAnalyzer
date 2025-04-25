# FluentAssertionsToShouldlyAnalyzer

[![License](https://img.shields.io/github/license/BrainSolutionsTeam/FAToShouldlyAnalyzer)](https://github.com/BrainSolutionsTeam/FAToShouldlyAnalyzer/blob/master/LICENSE)

A basic Roslyn analyzer to migrate from [FluentAssertions](https://github.com/fluentassertions/fluentassertions) to [Shouldly](https://github.com/shouldly/shouldly).

## Context

With the recent license change of [FluentAssertions](https://github.com/fluentassertions/fluentassertions) to a [commercial model](https://github.com/fluentassertions/fluentassertions/pull/2943), many developers are seeking for alternatives. [Shouldly](https://github.com/shouldly/shouldly) offers a clean and expressive syntax for assertions, pretty muche the same as **FA**.

This analyzer aims to ease the migration process by identifying usages of _FluentAssertions_ and suggesting equivalent _Shouldly_ assertions.

## Features

- Detects _FluentAssertions_ usages in C# code and reports diagnostics with messages suggesting to `Migrate to Shouldly`.
- (Planned) Provides automatic code fixes to apply _Shouldly_ syntax.

## Installation

1. Add the NuGet package (coming soon) to your test project.
2. Build your project to see diagnostics.
3. Apply code fixes (when implemented).

## Supported Assertions

Currently, based on [this guide](https://github.com/shouldly/shouldly/issues/1034) from the _Shouldly_ repo, the analyzer detects and suggests replacements for :

| FluentAssertions                                 | Shouldly                                          |
| ------------------------------------------------ | ------------------------------------------------- |
| result.Should().Be(expected);                    | result.ShouldBe(expected);                        |
| result.Should().NotBe(unexpected);               | result.ShouldNotBe(unexpected);                   |
| result.Should().BeNull();                        | result.ShouldBeNull();                            |
| result.Should().NotBeNull();                     | result.ShouldNotBeNull();                         |
| result.Should().BeTrue();                        | result.ShouldBeTrue();                            |
| result.Should().BeFalse();                       | result.ShouldBeFalse();                           |
| result.Should().BeSameAs(expected);              | result.ShouldBeSameAs(expected);                  |
| result.Should().NotBeSameAs(unexpected);         | result.ShouldNotBeSameAs(unexpected);             |
| result.Should().BeOfType();                      | result.ShouldBeOfType();                          |
| result.Should().NotBeOfType();                   | result.ShouldNotBeOfType();                       |
| result.Should().BeGreaterThan(value);            | result.ShouldBeGreaterThan(value);                |
| result.Should().BeGreaterThanOrEqualTo(value);   | result.ShouldBeGreaterThanOrEqualTo(value);       |
| result.Should().BeLessThan(value);               | result.ShouldBeLessThan(value);                   |
| result.Should().BeLessThanOrEqualTo(value);      | result.ShouldBeLessThanOrEqualTo(value);          |
| result.Should().BePositive();                    | result.ShouldBePositive();                        |
| result.Should().BeNegative();                    | result.ShouldBeNegative();                        |
| result.Should().BeInRange(low, high);            | result.ShouldBeInRange(low, high);                |
| result.Should().NotBeInRange(low, high);         | result.ShouldNotBeInRange(low, high);             |
| collection.Should().Contain(item);               | collection.ShouldContain(item);                   |
| collection.Should().NotContain(item);            | collection.ShouldNotContain(item);                |
| collection.Should().BeEmpty();                   | collection.ShouldBeEmpty();                       |
| collection.Should().NotBeEmpty();                | collection.ShouldNotBeEmpty();                    |
| collection.Should().HaveCount(count);            | collection.Count.ShouldBe(count);                 |
| collection.Should().HaveCountGreaterThan(value); | collection.Count.ShouldBeGreaterThan(value);      |
| collection.Should().HaveCountLessThan(value);    | collection.Count.ShouldBeLessThan(value);         |
| collection.Should().AllBeAssignableTo();         | collection.ShouldAllBeAssignableTo();             |
| collection.Should().OnlyHaveUniqueItems();       | collection.ShouldAllBeUnique();                   |
| dictionary.Should().ContainKey(key);             | dictionary.ShouldContainKey(key);                 |
| dictionary.Should().NotContainKey(key);          | dictionary.ShouldNotContainKey(key);              |
| dictionary.Should().ContainValue(value);         | dictionary.ShouldContainValue(value);             |
| dictionary.Should().NotContainValue(value);      | dictionary.ShouldNotContainValue(value);          |
| action.Should().Throw();                         | action.ShouldThrow();                             |
| action.Should().NotThrow();                      | action.ShouldNotThrow();                          |
| action.Should().NotThrow();                      | action.ShouldNotThrow();                          |
| action.Should().ThrowExactly();                  | action.ShouldThrowExactly();                      |
| action.Should().Throw().WithMessage("message");  | action.ShouldThrow().Message.ShouldBe("message"); |

## TODO

- [ ] Implement code fix provider for automatic assertion replacement.
- [ ] Expand support to other common _FluentAssertions_ methods.
- [ ] Add test coverage and CI integration.

## Contribution

Contributions are welcome! If you find a bug/issue/discussion, Feel free to open issues or pull requests.

## License

This project is licensed under the [MIT License](https://github.com/BrainSolutionsTeam/FAToShouldlyAnalyzer/blob/master/LICENSE).

## References

- [Code analysis using .NET compiler platform (Roslyn) analyzers - Visual Studio (Windows) | Microsoft Learn](https://learn.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview?view=vs-2022)
- [Tutorial: Write your first analyzer and code fix - C# | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix)
- https://github.com/agoda-com/Shouldly.FromAssert
- [Onboarding guide for FluentAssertions · Issue #1034 · shouldly/shouldly](https://github.com/shouldly/shouldly/issues/1034)
- [fluentassertions/fluentassertions](https://github.com/fluentassertions/fluentassertions)

---

## Thanks to

- The [Shouldly](https://github.com/shouldly/shouldly) Team for making one of the best assertion library for .NET projects. 👏🏽
- The AWesome [Roslyn](https://github.com/dotnet/roslyn) Team for making analyzer creation feel like a peace of cake.
- The [Shouldly.FromAssert](https://github.com/agoda-com/Shouldly.FromAssert?tab=readme-ov-file) repo for the idea. :)
- All the contributors.
