using FluentAssertions;
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
        var instance = new SimpleCollection { SimpleCollectionTest = "1", Collection = new[] { new Simple { Test = "11" } }.ToList() };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(SimpleCollection), typeof(string), "SimpleCollectionTest", "1"),
            new CollectedPropertyValue(typeof(Simple), typeof(string), "Collection[0].Test", "11")
        );
    }

    [Fact]
    public void Should_handle_object_with_nested_collections()
    {
        var instance = new CollectionWithNestedCollection { TestCollNestedColl = "1", Collection = new[] { new SimpleCollection { SimpleCollectionTest = "11", Collection = new[] { new Simple { Test = "111" } }.ToList() } }.ToList() };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(
            new CollectedPropertyValue(typeof(CollectionWithNestedCollection), typeof(string), "TestCollNestedColl", "1"),
            new CollectedPropertyValue(typeof(SimpleCollection), typeof(string), "Collection[0].SimpleCollectionTest", "11"),
            new CollectedPropertyValue(typeof(Simple), typeof(string), "Collection[0].Collection[0].Test", "111")
        );
    }

    [Fact]
    public void Should_handle_top_level_list()
    {
        var instance = new List<Simple> { new Simple { Test = "1" } };

        var result = ObjectMembersCollector.Collect(instance);

        result.Should().BeEquivalentTo(new CollectedPropertyValue(typeof(Simple), typeof(string), "[0].Test", "1"));
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
}