using Application.Common.Interfaces.Repositories;
using Application.Common.Result;
using Application.Features.ToDo.Commands.Create;
using Application.UnitTests.Constants;
using Domain.Common.Constants;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Tests.ToDo.Commands;

public class CreateToDoCommandHandlerTest
{
    private readonly CreateToDoCommandHandler _handler;
    private readonly Mock<IToDoRepository> _toDoItemRepositoryMock = new();

    public CreateToDoCommandHandlerTest()
    {
        _handler = new CreateToDoCommandHandler(_toDoItemRepositoryMock.Object);
    }

    #region ValidationTests

    [Theory]
    [InlineData(0, ToDoTestsConstants.ValidPriority, nameof(CreateToDoCommand.Title), DbConstraints.MinToDoNameLength, true)]
    [InlineData(DbConstraints.MaxToDoNameLength + 1, ToDoTestsConstants.ValidPriority, nameof(CreateToDoCommand.Title), DbConstraints.MaxToDoNameLength, false)]
    [InlineData(1, -1, nameof(CreateToDoCommand.Priority), 0, true)]
    public void Validate_ShouldReturnError_WhenInputsAreInvalid(int titleLength, int priority, string propertyName, short limit, bool isMin)
    {
        // Arrange
        var title = new string('a', titleLength);
        var command = new CreateToDoCommand(title, priority);
        var expectedMessage = isMin
            ? ValidatorMessage.MinLength(propertyName, limit)
            : ValidatorMessage.MaxLength(propertyName, limit);

        // Act
        (bool isValid, string? errorMessage) = command.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Equal(expectedMessage, errorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnValid_WhenTitleAndPriorityAreValid()
    {
        // Arrange
        var command = new CreateToDoCommand(ToDoTestsConstants.ValidTitle, ToDoTestsConstants.ValidPriority);

        // Act
        (bool isValid, string? errorMessage) = command.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion

    #region HandlerTests

    [Fact]
    public async Task Handle_ShouldReturnErrorResult_WhenSaveChangesFails()
    {
        // Arrange
        var command = new CreateToDoCommand(ToDoTestsConstants.ValidTitle, ToDoTestsConstants.ValidPriority);
        _toDoItemRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorMessage.SomethingWentWrong, result.Error.Message);
        _toDoItemRepositoryMock
            .Verify(repo => repo.AddAsync(It.Is<ToDoEntity>(e =>
                        e.Title == command.Title &&
                        e.Priority == command.Priority),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        _toDoItemRepositoryMock
            .Verify(repo => repo.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCreatedResource_WhenToDoItemIsCreated()
    {
        // Arrange
        var command = new CreateToDoCommand(ToDoTestsConstants.ValidTitle, ToDoTestsConstants.ValidPriority);
        _toDoItemRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);
        Assert.Equal(command.Title, result.Data.Title);
        Assert.Equal(command.Priority, result.Data.Priority);
        _toDoItemRepositoryMock
            .Verify(repo => repo.AddAsync(It.Is<ToDoEntity>(e =>
                        e.Title == command.Title &&
                        e.Priority == command.Priority),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        _toDoItemRepositoryMock
            .Verify(repo => repo.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    #endregion
}
