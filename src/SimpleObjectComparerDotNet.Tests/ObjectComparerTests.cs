using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit;

namespace SimpleObjectComparerDotNet.Tests;

public class ObjectComparerTests
{
    public class Basic
    {
        [Fact]
        public void Should_handle_simple_equal()
        {
            var result1 = new Simple { Test = "1" };
            var result2 = new Simple { Test = "2" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(false, nameof(Simple.Test), result1.Test, result2.Test));
        }

        [Fact]
        public void Should_handle_simple_arg2_null_class()
        {
            var result1 = new SimpleUsage { Test = new Simple { Test = "1" } };
            var result2 = default(SimpleUsage);

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(false, nameof(SimpleUsage.Test), result1.Test.ToString(), "null"));
        }

        [Fact]
        public void Should_handle_actual_have_class_prop_null()
        {
            var result1 = new SimpleUsage { Test = new Simple { Test = "1" } };
            var result2 = new SimpleUsage { Test = default };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(false, nameof(SimpleUsage.Test), result1.Test.ToString(), "null"));
        }

        [Fact]
        public void Should_handle_simple_both_null()
        {
            var result1 = new SimpleUsage { Test = default };
            var result2 = new SimpleUsage { Test = default };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(true, nameof(SimpleUsage.Test), "null", "null"));
        }

        [Fact]
        public void Should_handle_simple_equal_with_derived()
        {
            // TODO maybe Type should match and it should be configurable ? 
            var result1 = new SimpleWithDerived { Data = new SimpleDerived1 { Value = "1", BaseForDerivedProp = "11" } };
            var result2 = new SimpleWithDerived { Data = new SimpleDerived2 { Value = "1", BaseForDerivedProp = "11" } };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(true, "Data.Value", "1", "1"),
                new CompareResult(true, "Data.BaseForDerivedProp", "11", "11"));
        }

        [Fact]
        public void Should_handle_simple_not_equal()
        {
            var result1 = new Simple { Test = "1" };
            var result2 = new Simple { Test = "1" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(true, nameof(Simple.Test), result1.Test, result2.Test));
        }

        [Fact]
        public void Should_handle_simple_arg2_null()
        {
            var result1 = new Simple { Test = "1" };
            var result2 = default(Simple);

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(false, nameof(Simple.Test), result1.Test, "null"));
        }

        [Fact]
        public void Should_handle_simple_arg1_null()
        {
            var result1 = default(Simple);
            var result2 = new Simple { Test = "2" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(false, nameof(Simple.Test), "null", result2.Test));
        }

        [Fact]
        public void Should_handle_nested_equal()
        {
            var result1 = new SimpleNested { TestNested = "1", Simple = new Simple { Test = "11" } };
            var result2 = new SimpleNested { TestNested = "1", Simple = new Simple { Test = "11" } };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(true, nameof(SimpleNested.TestNested), result1.TestNested, result2.TestNested),
                new CompareResult(true, $"{nameof(SimpleNested.Simple)}.{nameof(Simple.Test)}", result1.Simple.Test, result2.Simple.Test));
        }

        [Fact]
        public void Should_handle_nested_not_equal()
        {
            var result1 = new SimpleNested { TestNested = "1", Simple = new Simple { Test = "11" } };
            var result2 = new SimpleNested { TestNested = "2", Simple = new Simple { Test = "22" } };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(false, nameof(SimpleNested.TestNested), result1.TestNested, result2.TestNested),
                new CompareResult(false, $"{nameof(SimpleNested.Simple)}.{nameof(Simple.Test)}", result1.Simple.Test, result2.Simple.Test));
        }

        [Fact]
        public void Should_handle_backing_field()
        {
            var result1 = new SimpleBacking { Test = "1" };
            var result2 = new SimpleBacking { Test = "1" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(true, nameof(SimpleBacking.Test), result1.Test, result2.Test));
        }

        [Fact]
        public void Should_handle_public_backing_field()
        {
            var result1 = new SimpleBackingPublic { Test = "1" };
            var result2 = new SimpleBackingPublic { Test = "1" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(true, nameof(SimpleBackingPublic.Test), result1.Test, result2.Test));
        }

        private class SimpleBacking
        {
            private string _test;

            public string Test { get => _test; set => _test = value; }
        }

        private class SimpleBackingPublic
        {
            public string _test;

            public string Test { get => _test; set => _test = value; }
        }


        private class SimpleUsage
        {
            public Simple Test { get; set; }
        }

        private class SimpleWithDerived
        {
            public BaseForDerived Data { get; set; }
        }

        private abstract class BaseForDerived
        {
            public string BaseForDerivedProp { get; set; }
        }

        private class SimpleDerived1 : BaseForDerived
        {
            public string Value { get; set; }
        }

        private class SimpleDerived2 : BaseForDerived
        {
            public string Value { get; set; }
        }
    }

    public class Collections
    {
        [Fact]
        public void Should_handle_collection_equal()
        {
            var result1 = new SimpleCollection { OtherProp = "2", TestCol = "1", Collection = new[] { new Simple { Test = "11" } }.ToList() };
            var result2 = new SimpleCollection { OtherProp = "2", TestCol = "1", Collection = new[] { new Simple { Test = "11" } }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(true, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                new CompareResult(true, $"{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}", result1.Collection[0].Test, result2.Collection[0].Test),
                new CompareResult(true, nameof(SimpleCollection.OtherProp), result1.OtherProp, result2.OtherProp));
        }

        [Fact]
        public void Should_handle_same_collection_reference_equal()
        {
            var collection = new[] { new Simple { Test = "11" } }.ToList();
            var result1 = new SimpleCollection { TestCol = "1", Collection = collection, OtherProp = "2" };
            var result2 = new SimpleCollection { TestCol = "1", Collection = collection, OtherProp = "2" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(true, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                new CompareResult(true, $"{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}", result1.Collection[0].Test, result2.Collection[0].Test),
                new CompareResult(true, nameof(SimpleCollection.OtherProp), result1.OtherProp, result2.OtherProp));
        }

        [Fact]
        public void Should_handle_collection_not_equal()
        {
            var result1 = new SimpleCollection { TestCol = "1", Collection = new[] { new Simple { Test = "11" } }.ToList(), OtherProp = "111", };
            var result2 = new SimpleCollection { TestCol = "2", Collection = new[] { new Simple { Test = "22" } }.ToList(), OtherProp = "222", };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(false, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}", result1.Collection[0].Test, result2.Collection[0].Test),
                new CompareResult(false, nameof(SimpleCollection.OtherProp), result1.OtherProp, result2.OtherProp));
        }

        [Fact]
        public void Should_handle_collection_not_equal_count_second_hasMore()
        {
            var result1 = new SimpleCollection { TestCol = "1", Collection = new[] { new Simple { Test = "11" } }.ToList() };
            var result2 = new SimpleCollection { TestCol = "2", Collection = new[] { new Simple { Test = "22" }, new Simple { Test = "222" } }.ToList(), OtherProp = "2222" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(false, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}", result1.Collection[0].Test, result2.Collection[0].Test),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}[1].{nameof(Simple.Test)}", "null", result2.Collection[1].Test),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}.Count", result1.Collection.Count.ToString(), result2.Collection.Count.ToString()),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}.Length", result1.Collection.Count.ToString(), result2.Collection.Count.ToString()),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}.LongLength", result1.Collection.Count.ToString(), result2.Collection.Count.ToString()),
                new CompareResult(false, nameof(SimpleCollection.OtherProp), "null", result2.OtherProp)
            );
        }

        [Fact]
        public void Should_handle_collection_not_equal_count_first_hasMore()
        {
            var result1 = new SimpleCollection { TestCol = "1", Collection = new[] { new Simple { Test = "11" }, new Simple { Test = "111" } }.ToList(), OtherProp = "11", };
            var result2 = new SimpleCollection { TestCol = "2", Collection = new[] { new Simple { Test = "22" } }.ToList(), OtherProp = "22", };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(false, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}", result1.Collection[0].Test, result2.Collection[0].Test),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}[1].{nameof(Simple.Test)}", result1.Collection[1].Test, "null"),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}.Count", result1.Collection.Count.ToString(), result2.Collection.Count.ToString()),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}.Length", result1.Collection.Count.ToString(), result2.Collection.Count.ToString()),
                new CompareResult(false, $"{nameof(SimpleCollection.Collection)}.LongLength", result1.Collection.Count.ToString(), result2.Collection.Count.ToString()),
                new CompareResult(false, nameof(SimpleCollection.OtherProp), result1.OtherProp, result2.OtherProp)
            );
        }

        [Fact]
        public void Should_handle_collection_not_equal_first_null_collection()
        {
            var result1 = new SimpleCollection { TestCol = "1", Collection = null };
            var result2 = new SimpleCollection { TestCol = "2", Collection = new[] { new Simple { Test = "22" } }.ToList(), OtherProp = "222" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                    new CompareResult(false, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                    new CompareResult(false, $"{nameof(SimpleCollection.Collection)}", "null", result2.Collection.ToString()),
                    new CompareResult(false, nameof(SimpleCollection.OtherProp), "null", result2.OtherProp)
            );
        }

        [Fact]
        public void Should_handle_collection_not_equal_second_null_collection()
        {
            var result1 = new SimpleCollection { TestCol = "1", Collection = new[] { new Simple { Test = "11" } }.ToList(), OtherProp = "111" };
            var result2 = new SimpleCollection { TestCol = "2", Collection = null };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                    new CompareResult(false, nameof(SimpleCollection.TestCol), result1.TestCol, result2.TestCol),
                    new CompareResult(false, $"{nameof(SimpleCollection.Collection)}", result1.Collection.ToString(), "null"),
                    new CompareResult(false, nameof(SimpleCollection.OtherProp), result1.OtherProp, "null")
            );
        }

        [Fact]
        public void Should_handle_collection_string_equal()
        {
            var result1 = new SimpleCollectionString { TestString = "1", Collection = new[] { "11" }.ToList() };
            var result2 = new SimpleCollectionString { TestString = "1", Collection = new[] { "11" }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new[]
            {
                new CompareResult(true, nameof(SimpleCollectionString.TestString), result1.TestString, result2.TestString),
                new CompareResult(true, $"{nameof(SimpleCollectionString.Collection)}[0]", result1.Collection[0], result2.Collection[0]),
            });
        }

        [Fact]
        public void Should_handle_collection_string_not_equal()
        {
            var result1 = new SimpleCollectionString { TestString = "1", Collection = new[] { "11" }.ToList() };
            var result2 = new SimpleCollectionString { TestString = "2", Collection = new[] { "22" }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new[]
            {
            new CompareResult(false, nameof(SimpleCollectionString.TestString), result1.TestString, result2.TestString),
            new CompareResult(false, $"{nameof(SimpleCollectionString.Collection)}[0]", result1.Collection[0], result2.Collection[0]),
        });
        }

        [Fact]
        public void Should_handle_collection_primitive_double_custom_type_comparer_equal()
        {
            var result1 = new SimpleCollectionDouble { TestString = "1", Collection = new[] { 1.02 }.ToList() };
            var result2 = new SimpleCollectionDouble { TestString = "1", Collection = new[] { 1.03 }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
            {
                config.AddCustomTypeComparer(typeof(double), new DoubleComparer(0.1));
            });

            result.Should().BeEquivalentTo(new[]
            {
            new CompareResult(true, nameof(SimpleCollectionString.TestString), result1.TestString, result2.TestString),
            new CompareResult(true, $"{nameof(SimpleCollectionString.Collection)}[0]", result1.Collection[0].ToString(), result2.Collection[0].ToString()),
        });
        }

        [Fact]
        public void Should_handle_collection_nestedobj_equal()
        {
            var result1 = new CollectionWithNestedObject { TestNestedCol = "1", Collection = new[] { new SimpleNested { TestNested = "11", Simple = new Simple { Test = "111" } } }.ToList() };
            var result2 = new CollectionWithNestedObject { TestNestedCol = "1", Collection = new[] { new SimpleNested { TestNested = "11", Simple = new Simple { Test = "111" } } }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(true, nameof(CollectionWithNestedObject.TestNestedCol), result1.TestNestedCol, result2.TestNestedCol),
                new CompareResult(true, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleNested.TestNested)}", result1.Collection[0].TestNested, result2.Collection[0].TestNested),
                new CompareResult(true, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleNested.Simple)}.{nameof(Simple.Test)}", result1.Collection[0].Simple.Test, result2.Collection[0].Simple.Test)
            );
        }

        [Fact]
        public void Should_handle_collection_nestedobj_not_equal()
        {
            var result1 = new CollectionWithNestedObject { TestNestedCol = "1", Collection = new[] { new SimpleNested { TestNested = "11", Simple = new Simple { Test = "111" } } }.ToList() };
            var result2 = new CollectionWithNestedObject { TestNestedCol = "2", Collection = new[] { new SimpleNested { TestNested = "22", Simple = new Simple { Test = "222" } } }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(false, nameof(CollectionWithNestedObject.TestNestedCol), result1.TestNestedCol, result2.TestNestedCol),
                new CompareResult(false, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleNested.TestNested)}", result1.Collection[0].TestNested, result2.Collection[0].TestNested),
                new CompareResult(false, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleNested.Simple)}.{nameof(Simple.Test)}", result1.Collection[0].Simple.Test, result2.Collection[0].Simple.Test)
            );
        }

        [Fact]
        public void Should_handle_collection_nested_collection_equal()
        {
            var result1 = new CollectionWithNestedObjectWithCollection { TestCollNestedColl = "1", Collection = new[] { new SimpleCollection { TestCol = "11", Collection = new[] { new Simple { Test = "111" } }.ToList(), OtherProp = "22", } }.ToList() };
            var result2 = new CollectionWithNestedObjectWithCollection { TestCollNestedColl = "1", Collection = new[] { new SimpleCollection { TestCol = "11", Collection = new[] { new Simple { Test = "111" } }.ToList(), OtherProp = "22", } }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
             new CompareResult(true, nameof(CollectionWithNestedObjectWithCollection.TestCollNestedColl), result1.TestCollNestedColl, result2.TestCollNestedColl),
             new CompareResult(true, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleCollection.TestCol)}", result1.Collection[0].TestCol, result2.Collection[0].TestCol),
             new CompareResult(true, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}",
                            result1.Collection[0].Collection[0].Test, result2.Collection[0].Collection[0].Test),
                new CompareResult(true, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleCollection.OtherProp)}", result1.Collection[0].OtherProp, result2.Collection[0].OtherProp)
            );
        }

        [Fact]
        public void Should_handle_collection_nested_collection_not_equal()
        {
            var result1 = new CollectionWithNestedObjectWithCollection { TestCollNestedColl = "1", Collection = new[] { new SimpleCollection { TestCol = "11", Collection = new[] { new Simple { Test = "111" } }.ToList(), OtherProp = "111" } }.ToList() };
            var result2 = new CollectionWithNestedObjectWithCollection { TestCollNestedColl = "2", Collection = new[] { new SimpleCollection { TestCol = "22", Collection = new[] { new Simple { Test = "222" } }.ToList(), OtherProp = "222" } }.ToList() };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(false, nameof(CollectionWithNestedObjectWithCollection.TestCollNestedColl), result1.TestCollNestedColl, result2.TestCollNestedColl),
                new CompareResult(false, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleCollection.TestCol)}", result1.Collection[0].TestCol, result2.Collection[0].TestCol),
                new CompareResult(false, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleCollection.Collection)}[0].{nameof(Simple.Test)}",
                result1.Collection[0].Collection[0].Test, result2.Collection[0].Collection[0].Test),
                new CompareResult(false, $"{nameof(CollectionWithNestedObject.Collection)}[0].{nameof(SimpleCollection.OtherProp)}", result1.Collection[0].OtherProp, result2.Collection[0].OtherProp)
            );
        }

        [Fact]
        public void Should_handle_toplevel_list()
        {
            var result1 = new List<Simple> { new Simple { Test = "1" } };
            var result2 = new List<Simple> { new Simple { Test = "2" } };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(false, $"[0].{nameof(Simple.Test)}", result1[0].Test, result2[0].Test));
        }

        [Fact]
        public void Should_handle_property_custom_collections()
        {
            var result1 = new TypeWithCustomCollection { TestString = "1", Values = new CustomCollection<string>(new[] { "1" }) };
            var result2 = new TypeWithCustomCollection { TestString = "1", Values = new CustomCollection<string>(new[] { "1" }) };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(
                new CompareResult(true, nameof(TypeWithCustomCollection.TestString), result1.TestString, result2.TestString),
                new CompareResult(true, $"{nameof(TypeWithCustomCollection.Values)}[0]", result1.Values[0], result2.Values[0]));
        }


        [Fact]
        public void Should_handle_custom_collections()
        {
            var result1 = new CustomCollection<string>(new[] { "1" });
            var result2 = new CustomCollection<string>(new[] { "1" });

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            result.Should().BeEquivalentTo(new CompareResult(true, $"[0]", result1[0], result2[0]));
        }

        private class SimpleCollection
        {
            public string TestCol { get; set; }
            public List<Simple> Collection { get; set; } = new List<Simple>();
            public string OtherProp { get; set; }
        }

        private class SimpleCollectionString
        {
            public string TestString { get; set; }
            public List<string> Collection { get; set; } = new List<string>();
        }

        private class SimpleCollectionDouble
        {
            public string TestString { get; set; }
            public List<double> Collection { get; set; } = new List<double>();
        }

        private class CollectionWithNestedObject
        {
            public string TestNestedCol { get; set; }
            public List<SimpleNested> Collection { get; set; } = new List<SimpleNested>();
        }

        private class CollectionWithNestedObjectWithCollection
        {
            public string TestCollNestedColl { get; set; }
            public List<SimpleCollection> Collection { get; set; } = new List<SimpleCollection>();
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
    }

    public class Configuration
    {
        [Fact]
        public void Should_handle_custom_type_comparer()
        {
            var result1 = new SimpleDouble { Value = 1.02 };
            var result2 = new SimpleDouble { Value = 1.03 };

            var resultDefault = ObjectComparer.ComparePublicMembers(result1, result2);
            resultDefault.Should().BeEquivalentTo(new CompareResult(false, $"{nameof(SimpleDouble.Value)}", result1.Value.ToString(), result2.Value.ToString()));

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
             {
                 config.AddCustomTypeComparer(typeof(double), new DoubleComparer(0.1));
             });

            result.Should().BeEquivalentTo(new CompareResult(true, $"{nameof(SimpleDouble.Value)}", result1.Value.ToString(), result2.Value.ToString()));
        }

        private class SimpleDouble
        {
            public double Value { get; set; }
        }

        [Fact]
        public void Should_handle_custom_type_comparer_toplevel_list()
        {
            var result1 = new List<double> { 1.02, 1.01 };
            var result2 = new List<double> { 1.03, 1.05 };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
            {
                config.AddCustomTypeComparer(typeof(double), new DoubleComparer(0.1));
            });


            result.Should().BeEquivalentTo(
                new CompareResult(true, $"[0]", result1[0].ToString(), result2[0].ToString()),
                new CompareResult(true, $"[1]", result1[1].ToString(), result2[1].ToString()));
        }

        [Fact]
        public void Should_exclude_configured_members()
        {
            var result1 = new TestClass { Value1 = "1", Value3 = "3", Value4 = "4" };
            var result2 = new TestClass { Value1 = "1", Value3 = "3", Value4 = "4" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
            {
                config.ExcludeMemberName("Value1");
            });

            result.Should().BeEquivalentTo(
                new CompareResult(true, $"{nameof(TestClass.Value2)}", "null", "null"),
                new CompareResult(true, $"{nameof(TestClass.Value3)}", result1.Value3.ToString(), result2.Value3.ToString()),
                new CompareResult(true, $"{nameof(TestClass.Value4)}", result1.Value4.ToString(), result2.Value4.ToString()));
        }

        [Fact]
        public void Should_only_include_members_with_attribute()
        {
            var result1 = new TestClassAttributeUsage { Value1 = "1", Value2 = "2" };
            var result2 = new TestClassAttributeUsage { Value1 = "1", Value2 = "2" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
            {
                config.IncludeMemberWithAttribute(typeof(MyAttribute));
            });

            result.Should().BeEquivalentTo(new CompareResult(true, $"{nameof(TestClass.Value2)}", result1.Value2.ToString(), result2.Value2.ToString()));
        }

        [Fact]
        public void Should_respect_ignore_data_member_attribute()
        {
            var result1 = new TestClassIgnoreDataMemberAttribute { Value1 = "1", Value2 = "2" };
            var result2 = new TestClassIgnoreDataMemberAttribute { Value1 = "1", Value2 = "2" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
            {
                config.RespectIgnoreDataMember = true;
            });

            result.Should().BeEquivalentTo(new CompareResult(true, $"{nameof(TestClass.Value2)}", result1.Value2.ToString(), result2.Value2.ToString()));
        }

        [Fact]
        public void Should_ignore_nullvalues()
        {
            var result1 = new TestClass { Value1 = "1", Value3 = "3", Value4 = "4" };
            var result2 = new TestClass { Value1 = "1", Value3 = "3", Value4 = "4" };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, config =>
            {
                config.IgnoreNullValues = true;
            });

            result.Should().BeEquivalentTo(
                new CompareResult(true, $"{nameof(TestClass.Value1)}", result1.Value1.ToString(), result2.Value1.ToString()),
                new CompareResult(true, $"{nameof(TestClass.Value3)}", result1.Value3.ToString(), result2.Value3.ToString()),
                new CompareResult(true, $"{nameof(TestClass.Value4)}", result1.Value4.ToString(), result2.Value4.ToString()));
        }

        [Fact]
        public void Should_use_default_formats()
        {
            var now = DateTime.Now;
            var nowOffset = DateTimeOffset.Now;

            var result1 = new TestFormats { DateTime1 = now, DateTimeOffset1 = nowOffset };
            var result2 = new TestFormats { DateTime1 = now, DateTimeOffset1 = nowOffset };

            var result = ObjectComparer.ComparePublicMembers(result1, result2);

            var defaultOptions = new ValueFormatOptions();

            result.Should().BeEquivalentTo(
                new CompareResult(true, $"{nameof(TestFormats.DateTime1)}", result1.DateTime1.ToString(defaultOptions.DateTimeValueFormat), result2.DateTime1.ToString(defaultOptions.DateTimeValueFormat)),
                new CompareResult(true, $"{nameof(TestFormats.DateTimeOffset1)}", result1.DateTimeOffset1.ToString(defaultOptions.DateTimeOffsetValueFormat), result2.DateTimeOffset1.ToString(defaultOptions.DateTimeOffsetValueFormat)));
        }

        [Fact]
        public void Should_use_datetime_format_for_offset()
        {
            var now = DateTime.Now;
            var nowOffset = DateTimeOffset.Now;

            var result1 = new TestFormats { DateTime1 = now, DateTimeOffset1 = nowOffset };
            var result2 = new TestFormats { DateTime1 = now, DateTimeOffset1 = nowOffset };

            var result = ObjectComparer.ComparePublicMembers(result1, result2, configure =>
                configure.ValueFormats.UseDateTimeFormatForDateTimeOffset = true);

            var defaultOptions = new ValueFormatOptions();

            result.Should().BeEquivalentTo(
                new CompareResult(true, $"{nameof(TestFormats.DateTime1)}", result1.DateTime1.ToString(defaultOptions.DateTimeValueFormat), result2.DateTime1.ToString(defaultOptions.DateTimeValueFormat)),
                new CompareResult(true, $"{nameof(TestFormats.DateTimeOffset1)}", result1.DateTimeOffset1.ToString(defaultOptions.DateTimeValueFormat), result2.DateTimeOffset1.ToString(defaultOptions.DateTimeValueFormat)));
        }


        private class TestFormats
        {
            public DateTime DateTime1 { get; set; }
            public DateTimeOffset DateTimeOffset1 { get; set; }
        }

        private class TestClass
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }

            public string Value3;

            private string value4;
            public string Value4 { get => value4; set => value4 = value; }
        }

        internal class MyAttribute : Attribute { }

        private class TestClassAttributeUsage
        {
            public string Value1 { get; set; }

            [My]
            public string Value2 { get; set; }
        }

        private class TestClassIgnoreDataMemberAttribute
        {
            [IgnoreDataMember]
            public string Value1 { get; set; }

            public string Value2 { get; set; }
        }
    }

    private class Simple
    {
        public string Test { get; set; }
    }

    private class SimpleNested
    {
        public string TestNested { get; set; }
        public Simple Simple { get; set; }
    }

    private class DoubleComparer : ValueComparer
    {
        private readonly double precision;

        public DoubleComparer(double precision)
        {
            this.precision = precision;
        }

        public override bool IsEqual(object value1, object value2, ObjectCompareOptions config)
        {
            if (value1 == null && value2 == null)
            {
                return false;
            }
            double double1 = value1 != null ? (double)value1 : 0;
            double double2 = value2 != null ? (double)value2 : 0;
            var consideredEqual = Math.Abs(double1 - double2) <= precision;
            return consideredEqual;
        }
    }
}