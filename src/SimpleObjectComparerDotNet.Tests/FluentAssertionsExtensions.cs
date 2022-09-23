using FluentAssertions;
using FluentAssertions.Collections;
using System.Collections.Generic;

namespace SimpleObjectComparerDotNet.Tests;

public static class FluentAssertionsExtensions
{
    public static AndConstraint<TAssertions> BeEquivalentTo<TCollection, T, TAssertions>(this GenericCollectionAssertions<TCollection, T, TAssertions> self, params T[] expectation)
        where TCollection : IEnumerable<T>
        where TAssertions : GenericCollectionAssertions<TCollection, T, TAssertions> => self.BeEquivalentTo(expectation);
}