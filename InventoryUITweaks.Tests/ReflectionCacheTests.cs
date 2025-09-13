using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.InventoryUITweaks.Tests;

public class ReflectionCacheTests
{
    [Fact]
    public void DirectoryField_ShouldExist()
    {
        Assert.NotNull(ReflectionCache.InventoryItemUI_Directory);
    }
}
