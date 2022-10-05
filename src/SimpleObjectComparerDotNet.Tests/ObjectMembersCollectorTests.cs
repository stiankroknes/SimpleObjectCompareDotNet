using FluentAssertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleObjectComparerDotNet.Tests;

public class ObjectMembersCollectorTests
{
    [Fact]
    public void Should_handle_simple_object()
    {
        var instance = new Simple { Test = "1" };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(Simple), typeof(string), "Test", "1"));
    }

    [Fact]
    public void Should_handle_object_with_nested_instance()
    {
        var instance = new SimpleWithNestedObject { Test = "1", Simple = new Simple { Test = "11" } };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(SimpleWithNestedObject), typeof(string), "Test", "1"),
            new CollectedPropertyValue(typeof(Simple), typeof(string), "Simple.Test", "11"));
    }

    [Fact]
    public void Should_handle_object_with_collection()
    {
        var instance = new SimpleCollection { SimpleCollectionTest = "1", Collection = new[] { new Simple { Test = "11" } }.ToList(), OtherProp = "111" };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(SimpleCollection), typeof(string), "SimpleCollectionTest", "1"),
            new CollectedPropertyValue(typeof(Simple), typeof(string), "Collection[0].Test", "11"),
            new CollectedPropertyValue(typeof(SimpleCollection), typeof(string), "OtherProp", "111")
        );
    }

    [Fact]
    public void Should_handle_object_with_nested_collections()
    {
        var instance = new CollectionWithNestedCollection { TestCollNestedColl = "1", Collection = new[] { new SimpleCollection { SimpleCollectionTest = "11", Collection = new[] { new Simple { Test = "111" } }.ToList(), OtherProp = "1111" } }.ToList() };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(CollectionWithNestedCollection), typeof(string), "TestCollNestedColl", "1"),
            new CollectedPropertyValue(typeof(SimpleCollection), typeof(string), "Collection[0].SimpleCollectionTest", "11"),
            new CollectedPropertyValue(typeof(Simple), typeof(string), "Collection[0].Collection[0].Test", "111"),
            new CollectedPropertyValue(typeof(SimpleCollection), typeof(string), "Collection[0].OtherProp", "1111")
        );
    }

    [Fact]
    public void Should_handle_top_level_list()
    {
        var instance = new List<Simple> { new Simple { Test = "1" } };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(Simple), typeof(string), "[0].Test", "1"));
    }

    [Fact]
    public void Should_handle_property_custom_collections()
    {
        var instance = new TypeWithCustomCollection { TestString = "1", Values = new CustomCollection<string>(new[] { "1" }) };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(TypeWithCustomCollection), typeof(string), "TestString", "1"),
            new CollectedPropertyValue(typeof(CustomCollection<string>), typeof(string), "Values[0]", "1"));
    }

    [Fact]
    public void Should_handle_custom_generic_collections()
    {
        var instance = new CustomCollection<string>(new[] { "1" });

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(CustomCollection<string>), typeof(string), "[0]", "1"));
    }

    [Fact]
    public void Should_handle_custom_collections()
    {
        var instance = new CustomStringCollection(new[] { "1" });

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(CustomStringCollection), typeof(string), "[0]", "1"));
    }

    [Fact]
    public void Should_allow_set_rootpath_prefix()
    {
        var instance = new Simple { Test = "1" };

        var result = ObjectMembersCollector.Collect(instance, configure =>
        {
            configure.RootPathPrefix = nameof(Simple);
        });

        result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(Simple), typeof(string), $"{nameof(Simple)}.Test", "1"));
    }


    [Fact]
    public void Should_allow_ignore_enumerable_class()
    {
        var instance = new EnumerableClass { Test1 = "1", Test2 = "2" };

        var result = ObjectMembersCollector.Collect(instance, configure =>
        {
            configure.EnumerableFilter = enumerable => enumerable is not IEnumerable<KeyValuePair<string, object>>;
        });

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(EnumerableClass), typeof(string), "Test1", "1"),
            new CollectedPropertyValue(typeof(EnumerableClass), typeof(string), "Test2", "2"));
    }


    private class Simple
    {
        public string Test { get; set; }
    }

    private class SimpleWithNestedObject
    {
        public string Test { get; set; }
        public Simple Simple { get; set; }
    }

    private class SimpleCollection
    {
        public string SimpleCollectionTest { get; set; }
        public List<Simple> Collection { get; set; } = new List<Simple>();
        public string OtherProp { get; set; }
    }

    private class CollectionWithNestedCollection
    {
        public string TestCollNestedColl { get; set; }
        public List<SimpleCollection> Collection { get; set; } = new List<SimpleCollection>();
    }

    private class SimpleCollectionString
    {
        public string TestString { get; set; }
        public List<string> Collection { get; set; } = new List<string>();
    }

    private class TypeWithCustomCollection
    {
        public string TestString { get; set; }
        public CustomCollection<string> Values { get; set; } = new CustomCollection<string>();
    }

    private class CustomCollection<T> : List<T>
    {
        public CustomCollection() { }
        public CustomCollection(IEnumerable<T> collection) : base(collection) { }
    }

    private class CustomStringCollection : List<string>
    {
        public CustomStringCollection(IEnumerable<string> collection) : base(collection) { }
    }

    // Case from Fhir hl7 .Net.
    // https://github.com/FirelyTeam/firely-net-common/blob/develop/src/Hl7.Fhir.Support.Poco/Model/Base.cs
    private class EnumerableClass : IReadOnlyDictionary<string, object>
    {
        public string Test1 { get; set; }
        public string Test2 { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetElementPairs()
        {
            if (Test1 is not null) yield return new KeyValuePair<string, object>("test1", Test1);
            if (Test2 is not null) yield return new KeyValuePair<string, object>("test2", Test2);
        }

        // IReadOnlyDictionary
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => GetElementPairs().Select(kvp => kvp.Key);

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => GetElementPairs().Select(kvp => kvp.Value);

        int IReadOnlyCollection<KeyValuePair<string, object>>.Count => GetElementPairs().Count();

        object IReadOnlyDictionary<string, object>.this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

        bool IReadOnlyDictionary<string, object>.ContainsKey(string key) => TryGetValue(key, out _);

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => GetElementPairs().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetElementPairs().GetEnumerator();

        bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) => TryGetValue(key, out value);

        protected virtual bool TryGetValue(string key, out object value)
        {
            value = default;
            return false;
        }
    }
}