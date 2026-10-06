using System.Linq.Expressions;

namespace DotNetMapper.Tests;

public sealed class MappingTests
{
    [Fact]
    public void Same_name_same_type_properties_are_copied()
    {
        var input = new BasicSource
        {
            Id = 1,
            Name = "john",
            When = new DateTime(2026, 1, 2, 3, 4, 5),
            Amount = 12.5m,
            Key = Guid.NewGuid(),
            Flag = true,
            Color = Color.Green,
            Tags = ["a", "b"],
            Optional = 7,
        };

        BasicTarget AssertCopied(BasicTarget t)
        {
            Assert.Equal(input.Id, t.Id);
            Assert.Equal(input.Name, t.Name);
            Assert.Equal(input.When, t.When);
            Assert.Equal(input.Amount, t.Amount);
            Assert.Equal(input.Key, t.Key);
            Assert.Equal(input.Flag, t.Flag);
            Assert.Equal(input.Color, t.Color);
            Assert.Same(input.Tags, t.Tags);
            Assert.Equal(input.Optional, t.Optional);
            return t;
        }

        AssertCopied(Mapper.Map<BasicSource, BasicTarget>(input));
        AssertCopied(RuntimePath.Map<BasicSource, BasicTarget>(input));
    }

    [Fact]
    public void Name_match_is_case_sensitive()
    {
        var input = new CaseSource { Name = "hello" };

        CaseTarget AssertCase(CaseTarget t)
        {
            Assert.Equal("", t.name);
            return t;
        }

        AssertCase(Mapper.Map<CaseSource, CaseTarget>(input));
        AssertCase(RuntimePath.Map<CaseSource, CaseTarget>(input));
    }

    [Fact]
    public void Type_mismatch_is_not_copied()
    {
        var input = new TypeMismatchSource { Number = 1, ToNullable = 2, Text = "x" };

        TypeMismatchTarget AssertMismatch(TypeMismatchTarget t)
        {
            Assert.Equal(0, t.Number);
            Assert.Null(t.ToNullable);
            Assert.Null(t.Text);
            return t;
        }

        AssertMismatch(Mapper.Map<TypeMismatchSource, TypeMismatchTarget>(input));
        AssertMismatch(RuntimePath.Map<TypeMismatchSource, TypeMismatchTarget>(input));
    }

    [Fact]
    public void Source_private_getter_is_not_read()
    {
        var input = new PrivateGetterSource();
        input.Value = 5;

        PlainTarget AssertGetter(PlainTarget t)
        {
            Assert.Equal(0, t.Value);
            return t;
        }

        AssertGetter(Mapper.Map<PrivateGetterSource, PlainTarget>(input));
        AssertGetter(RuntimePath.Map<PrivateGetterSource, PlainTarget>(input));
    }

    [Fact]
    public void Source_write_only_property_is_not_read()
    {
        var input = new WriteOnlySource { Value = 5 };

        PlainTarget AssertWriteOnly(PlainTarget t)
        {
            Assert.Equal(0, t.Value);
            return t;
        }

        AssertWriteOnly(Mapper.Map<WriteOnlySource, PlainTarget>(input));
        AssertWriteOnly(RuntimePath.Map<WriteOnlySource, PlainTarget>(input));
    }

    [Fact]
    public void Target_private_setter_is_not_written()
    {
        var input = new PlainTarget { Value = 5 };
        var mapped = Mapper.Map<PlainTarget, PrivateSetterTarget>(input);
        Assert.Equal(0, mapped.Value);
    }

    [Fact]
    public void Target_internal_setter_is_not_written()
    {
        var input = new PlainTarget { Value = 5 };
        var mapped = Mapper.Map<PlainTarget, InternalSetterTarget>(input);
        Assert.Equal(0, mapped.Value);
    }

    [Fact]
    public void Target_getter_only_is_not_written()
    {
        var input = new PlainTarget { Value = 5 };

        GetterOnlyTarget AssertGetterOnly(GetterOnlyTarget t)
        {
            Assert.Equal(0, t.Value);
            return t;
        }

        AssertGetterOnly(Mapper.Map<PlainTarget, GetterOnlyTarget>(input));
        AssertGetterOnly(RuntimePath.Map<PlainTarget, GetterOnlyTarget>(input));
    }

    [Fact]
    public void Target_init_setter_is_written()
    {
        var input = new PlainTarget { Value = 5 };

        InitTarget AssertInit(InitTarget t)
        {
            Assert.Equal(5, t.Value);
            return t;
        }

        AssertInit(Mapper.Map<PlainTarget, InitTarget>(input));
        AssertInit(RuntimePath.Map<PlainTarget, InitTarget>(input));
    }

    [Fact]
    public void Record_with_init_and_parameterless_constructor_works()
    {
        var input = new BasicSource { Id = 9, Name = "record" };

        RecordTarget AssertRecord(RecordTarget t)
        {
            Assert.Equal(input.Id, t.Id);
            Assert.Equal(input.Name, t.Name);
            return t;
        }

        AssertRecord(Mapper.Map<BasicSource, RecordTarget>(input));
        AssertRecord(RuntimePath.Map<BasicSource, RecordTarget>(input));
    }

    [Fact]
    public void Struct_to_struct_maps()
    {
        var input = new StructSource { Id = 3, Name = "s" };

        StructTarget AssertStruct(StructTarget t)
        {
            Assert.Equal(input.Id, t.Id);
            Assert.Equal(input.Name, t.Name);
            return t;
        }

        AssertStruct(Mapper.Map<StructSource, StructTarget>(input));
        AssertStruct(RuntimePath.Map<StructSource, StructTarget>(input));
    }

    [Fact]
    public void Class_to_struct_maps()
    {
        var input = new SmallClassSource { Value = 4 };
        Assert.Equal(4, Mapper.Map<SmallClassSource, SmallStructTarget>(input).Value);
        Assert.Equal(4, RuntimePath.Map<SmallClassSource, SmallStructTarget>(input).Value);
    }

    [Fact]
    public void Struct_to_class_maps()
    {
        var input = new SmallStructTarget { Value = 4 };
        Assert.Equal(4, Mapper.Map<SmallStructTarget, SmallClassSource>(input).Value);
        Assert.Equal(4, RuntimePath.Map<SmallStructTarget, SmallClassSource>(input).Value);
    }

    [Fact]
    public void Inherited_properties_are_copied()
    {
        var input = new DerivedSource { BaseId = 1, DerivedId = 2 };

        DerivedTarget AssertInherited(DerivedTarget t)
        {
            Assert.Equal(input.BaseId, t.BaseId);
            Assert.Equal(input.DerivedId, t.DerivedId);
            return t;
        }

        AssertInherited(Mapper.Map<DerivedSource, DerivedTarget>(input));
        AssertInherited(RuntimePath.Map<DerivedSource, DerivedTarget>(input));
    }

    [Fact]
    public void Hidden_property_uses_most_derived_declaration()
    {
        var input = new HiddenDerivedSource { Name = "derived" };
        ((HiddenBaseSource)input).Name = "base";

        HiddenTarget AssertHidden(HiddenTarget t)
        {
            Assert.Equal("derived", t.Name);
            return t;
        }

        AssertHidden(Mapper.Map<HiddenDerivedSource, HiddenTarget>(input));
        AssertHidden(RuntimePath.Map<HiddenDerivedSource, HiddenTarget>(input));
    }

    [Fact]
    public void Indexers_are_ignored()
    {
        var input = new IndexerSource();
        input[0] = "x";

        IndexerTarget AssertIndexer(IndexerTarget t)
        {
            Assert.NotNull(t);
            return t;
        }

        AssertIndexer(Mapper.Map<IndexerSource, IndexerTarget>(input));
        AssertIndexer(RuntimePath.Map<IndexerSource, IndexerTarget>(input));
    }

    [Fact]
    public void Static_properties_are_ignored()
    {
        StaticSource.StaticProp = 42;
        StaticTarget.StaticProp = 0;

        var input = new StaticSource { InstanceProp = 7 };
        var mapped = Mapper.Map<StaticSource, StaticTarget>(input);
        Assert.Equal(7, mapped.InstanceProp);
        Assert.Equal(0, StaticTarget.StaticProp);
    }

    [Fact]
    public void Null_reference_input_throws_ArgumentNullException()
    {
        AssertBothThrows<BasicSource, BasicTarget>();
    }

    [Fact]
    public void Struct_input_is_never_null_checked()
    {
        var mapped = Mapper.Map<StructSource, StructTarget>(default);
        Assert.Equal(0, mapped.Id);

        var runtime = RuntimePath.Map<StructSource, StructTarget>(default);
        Assert.Equal(0, runtime.Id);
    }

    [Fact]
    public void Each_call_returns_a_new_instance()
    {
        var input = new BasicSource { Id = 1, Name = "n" };

        var first = Mapper.Map<BasicSource, BasicTarget>(input);
        var second = Mapper.Map<BasicSource, BasicTarget>(input);
        Assert.NotSame(first, second);

        var rf = RuntimePath.Map<BasicSource, BasicTarget>(input);
        var rs = RuntimePath.Map<BasicSource, BasicTarget>(input);
        Assert.NotSame(rf, rs);
    }

    [Fact]
    public void No_matching_properties_returns_new_target()
    {
        var input = new NoMatchSource { A = 1 };

        NoMatchTarget AssertNoMatch(NoMatchTarget t)
        {
            Assert.Equal(0, t.B);
            return t;
        }

        AssertNoMatch(Mapper.Map<NoMatchSource, NoMatchTarget>(input));
        AssertNoMatch(RuntimePath.Map<NoMatchSource, NoMatchTarget>(input));
    }

    [Fact]
    public void Same_type_mapping_is_a_shallow_copy()
    {
        var input = new BasicSource { Id = 5, Name = "x", Tags = ["t"] };

        BasicSource AssertSame(BasicSource t)
        {
            Assert.NotSame(input, t);
            Assert.Equal(input.Id, t.Id);
            Assert.Same(input.Tags, t.Tags);
            return t;
        }

        AssertSame(Mapper.Map<BasicSource, BasicSource>(input));
        AssertSame(RuntimePath.Map<BasicSource, BasicSource>(input));
    }

    [Fact]
    public void Call_inside_expression_tree_lambda_uses_runtime_path()
    {
        Expression<Func<BasicSource, BasicTarget>> expression = x => Mapper.Map<BasicSource, BasicTarget>(x);
        var compiled = expression.Compile();

        var result = compiled(new BasicSource { Id = 1, Name = "n" });
        Assert.Equal(1, result.Id);
        Assert.Equal("n", result.Name);
    }

    [Fact]
    public void Private_nested_types_map()
    {
        var input = new PrivateSource { Id = 8 };

        var mapped = Mapper.Map<PrivateSource, PrivateTarget>(input);
        Assert.Equal(8, mapped.Id);

        var runtime = RuntimePath.Map<PrivateSource, PrivateTarget>(input);
        Assert.Equal(8, runtime.Id);
    }

    private static void AssertBothThrows<TSource, TTarget>()
        where TTarget : new()
    {
        var direct = Assert.Throws<ArgumentNullException>(() => Mapper.Map<TSource, TTarget>(default!));
        Assert.Equal("inputObject", direct.ParamName);

        var runtime = Assert.Throws<ArgumentNullException>(() => RuntimePath.Map<TSource, TTarget>(default!));
        Assert.Equal("inputObject", runtime.ParamName);
    }

    private sealed class PrivateSource
    {
        public int Id { get; set; }
    }

    private sealed class PrivateTarget
    {
        public int Id { get; set; }
    }
}
