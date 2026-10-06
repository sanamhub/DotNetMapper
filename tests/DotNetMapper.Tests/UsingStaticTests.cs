using static DotNetMapper.Mapper;

namespace DotNetMapper.Tests;

public sealed class UsingStaticTests
{
    [Fact]
    public void Using_static_form_maps()
    {
        var input = new BasicSource { Id = 3, Name = "u" };

        var mapped = Map<BasicSource, BasicTarget>(input);
        Assert.Equal(3, mapped.Id);
        Assert.Equal("u", mapped.Name);
    }
}
