using EsnyaTweaks.Common.Reflection;
using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.Common.Tests;

public sealed class AssemblyMetadataTests
{
    [Fact]
    public void Read_Should_Return_NameAuthorVersionLink()
    {
        var asm = typeof(AssemblyMetadataTests).Assembly;
        var (name, author, version, link) = AssemblyMetadata.Read(asm);
        name.Should().NotBeNull();
        author.Should().NotBeNull();
        version.Should().NotBeNull();
        link.Should().NotBeNull();
    }
}
