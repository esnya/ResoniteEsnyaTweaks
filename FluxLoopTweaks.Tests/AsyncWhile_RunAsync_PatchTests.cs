using System.Threading.Tasks;
using FrooxEngine.ProtoFlux;
using Moq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using Xunit;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

public class AsyncWhile_RunAsync_PatchTests
{
    [Fact]
    public void Prefix_WithNonFrooxEngineContext_ShouldReturnTrue()
    {
        // Arrange
        var mockExecutionContext = new Mock<ExecutionContext>();
        var mockAsyncWhile = new Mock<AsyncWhile>();
        Task<IOperation>? result = null;

        // Act
        var shouldContinue = AsyncWhile_RunAsync_Patch.Prefix(
            mockAsyncWhile.Object,
            mockExecutionContext.Object,
            ref result!
        );

        // Assert
        Assert.True(shouldContinue);
        Assert.Null(result);
    }

    [Fact]
    public void Prefix_WithFrooxEngineContext_ShouldReturnFalse()
    {
        // Arrange
        var mockFrooxEngineContext = new Mock<FrooxEngineContext>();
        var mockAsyncWhile = new Mock<AsyncWhile>();
        Task<IOperation>? result = null;

        // Act
        var shouldContinue = AsyncWhile_RunAsync_Patch.Prefix(
            mockAsyncWhile.Object,
            mockFrooxEngineContext.Object,
            ref result!
        );

        // Assert
        Assert.False(shouldContinue);
        Assert.NotNull(result);
    }
}
