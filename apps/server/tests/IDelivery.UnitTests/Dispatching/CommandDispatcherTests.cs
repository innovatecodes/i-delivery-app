using FluentAssertions;
using IDelivery.Application.Abstractions.CQRS;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Dispatching;
using IDelivery.SharedKernel.Common.Result;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Dispatching;

public class CommandDispatcherTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CommandDispatcher _dispatcher;

    public CommandDispatcherTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _dispatcher = new CommandDispatcher(_mockServiceProvider.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Dispatch_WithValidCommand_ShouldResolveHandlerAndExecute()
    {
        var command = new TestCommand("test-value");
        var handler = new Mock<ICommandHandler<TestCommand>>();
        handler.Setup(x => x.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockServiceProvider.Setup(x => x.GetService(typeof(ICommandHandler<TestCommand>)))
            .Returns(handler.Object);

        var result = await _dispatcher.Dispatch(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        handler.Verify(x => x.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispatch_WithValidCommand_ShouldCallSaveChangesOnce()
    {
        var command = new TestCommand("test-value");
        var handler = new Mock<ICommandHandler<TestCommand>>();
        handler.Setup(x => x.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockServiceProvider.Setup(x => x.GetService(typeof(ICommandHandler<TestCommand>)))
            .Returns(handler.Object);

        await _dispatcher.Dispatch(command, CancellationToken.None);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispatch_WithFailingCommand_ShouldNotCallSaveChanges()
    {
        var command = new TestCommand("test-value");
        var handler = new Mock<ICommandHandler<TestCommand>>();
        handler.Setup(x => x.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("Test.Error", "Test error")));

        _mockServiceProvider.Setup(x => x.GetService(typeof(ICommandHandler<TestCommand>)))
            .Returns(handler.Object);

        var result = await _dispatcher.Dispatch(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Dispatch_WithResultCommand_ShouldResolveHandlerAndExecute()
    {
        var command = new TestCommandWithResult("test-value");
        var handler = new Mock<ICommandHandler<TestCommandWithResult, string>>();
        handler.Setup(x => x.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("result-value"));

        _mockServiceProvider.Setup(x => x.GetService(typeof(ICommandHandler<TestCommandWithResult, string>)))
            .Returns(handler.Object);

        var result = await _dispatcher.Dispatch<TestCommandWithResult, string>(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("result-value");
        handler.Verify(x => x.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispatch_WithResultCommand_ShouldCallSaveChangesOnce()
    {
        var command = new TestCommandWithResult("test-value");
        var handler = new Mock<ICommandHandler<TestCommandWithResult, string>>();
        handler.Setup(x => x.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("result-value"));

        _mockServiceProvider.Setup(x => x.GetService(typeof(ICommandHandler<TestCommandWithResult, string>)))
            .Returns(handler.Object);

        await _dispatcher.Dispatch<TestCommandWithResult, string>(command, CancellationToken.None);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispatch_WithFailingResultCommand_ShouldNotCallSaveChanges()
    {
        var command = new TestCommandWithResult("test-value");
        var handler = new Mock<ICommandHandler<TestCommandWithResult, string>>();
        handler.Setup(x => x.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(new Error("Test.Error", "Test error")));

        _mockServiceProvider.Setup(x => x.GetService(typeof(ICommandHandler<TestCommandWithResult, string>)))
            .Returns(handler.Object);

        var result = await _dispatcher.Dispatch<TestCommandWithResult, string>(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class TestCommand(string value) : ICommand
{
    public string Value { get; } = value;
}

public class TestCommandWithResult(string value) : ICommand<string>
{
    public string Value { get; } = value;
}
