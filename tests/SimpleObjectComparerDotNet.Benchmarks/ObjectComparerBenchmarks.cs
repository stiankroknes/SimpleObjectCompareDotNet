using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace SimpleObjectComparerDotNet.Benchmarks;

public class ObjectComparerBenchmarks
{
    private readonly Simple simple = new() { Test = "1" };

    private readonly CollectionWithNestedCollection complex = new()
    {
        TestCollNestedColl = "1",
        Collection = new[]
        {
            new SimpleCollection { SimpleCollectionTest = "11", Collection = new[] { new Simple { Test = "111" } }.ToList() }
        }.ToList()
    };

    private readonly CollectionWithNestedCollection complexCompare = new()
    {
        TestCollNestedColl = "2",
        Collection = new[]
        {
            new SimpleCollection { SimpleCollectionTest = "22", Collection = new[] { new Simple { Test = "2222" } }.ToList() }
        }.ToList()
    };

    private readonly Simple simpleCompare = new() { Test = "3" };

    //[GlobalSetup]
    //public void GlobalSetup()
    //{

    //}

    [Benchmark]
    public void ObjectComparer_simple() => ObjectComparer.ComparePublicMembers(simple, simpleCompare);

    [Benchmark]
    public void ObjectComparer_complex() => ObjectComparer.ComparePublicMembers(complex, complexCompare);


    private class Simple
    {
        public string Test { get; set; }
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
}