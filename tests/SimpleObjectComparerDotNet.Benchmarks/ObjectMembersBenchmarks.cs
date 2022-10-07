using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace SimpleObjectComparerDotNet.Benchmarks;

public class ObjectMembersBenchmarks
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

    //[GlobalSetup]
    //public void GlobalSetup()
    //{

    //}

    [Benchmark]
    public void ObjectMembers_simple() => ObjectMembersCollector.Collect(simple);

    [Benchmark]
    public void ObjectMembers_complex() => ObjectMembersCollector.Collect(complex);


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