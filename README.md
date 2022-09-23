[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/stiankroknes/SimpleObjectCompareDotNet)](https://github.com/stiankroknes/SimpleObjectCompareDotNet/issues)
[![GitHub forks](https://img.shields.io/github/forks/stiankroknes/SimpleObjectCompareDotNet)](https://github.com/stiankroknes/SimpleObjectCompareDotNet/members)
[![GitHub stars](https://img.shields.io/github/stars/stiankroknes/SimpleObjectCompareDotNet)](https://github.com/stiankroknes/SimpleObjectCompareDotNet/stargazers)

[![NuGet Downloads (official NuGet)](https://img.shields.io/nuget/dt/SimpleObjectCompareDotNet?label=NuGet%20Downloads)](https://www.nuget.org/packages/SimpleObjectCompareDotNet)

# SimpleObjectCompareDotNet

## Get Started

Install the NuGet package: [SimpleObjectCompareDotNet](https://www.nuget.org/packages/SimpleObjectCompareDotNet/)

Install using the Package Manager in your IDE or using the command line:

```bash
dotnet add package SimpleObjectCompareDotNet
```

## ObjectComparer

```csharp
[Fact]
public void Should_handle_simple_equal()
{
    var result1 = new Simple { Test = "1" };
    var result2 = new Simple { Test = "2" };

    var result = ObjectComparer.ComparePublicMembers(result1, result2);

    result.Should().BeEquivalentTo(new CompareResult(false, nameof(Simple.Test), result1.Test, result2.Test));
}

```

## ObjectMembersCollector
```csharp
[Fact]
public void Should_handle_simple_object()
{
    var instance = new Simple { Test = "1" };

    var result = ObjectMembersCollector.Collect(instance);

    result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(Simple), typeof(string), "Test", "1"));
}
```

## Benchmarks
[Report](src/SimpleObjectComparerDotNet.Benchmarks/BenchmarkDotNet.Artifacts/results/SimpleObjectComparerDotNet.Benchmarks.Benchmarks-report-github.md)
