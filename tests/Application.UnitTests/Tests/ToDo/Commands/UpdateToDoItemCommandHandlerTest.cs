using Application.Common.Interfaces.Repositories;
using Application.Common.Result;
using Application.Features.ToDo.Commands.Update;
using Application.UnitTests.Constants;
using Application.UnitTests.Utilities;
using Domain.Common.Constants;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Tests.ToDo.Commands;

public class UpdateToDoCommandHandlerTest
{
    private readonly UpdateToDoCommandHandler _handler;
    private readonly Mock<IToDoRepository> _toDoItemRepositoryMock = new();

    public UpdateToDoCommandHandlerTest()
    {
        _handler = new UpdateToDoCommandHandler(_toDoItemRepositoryMock.Object);
    }

    #region ValidationTests

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", 5, 1, null, "InvalidGuid", "", 0)]
    [InlineData("11111111-1111-1111-1111-111111111111", 0, 1, null, "MinLength", nameof(UpdateToDoCommand.Title), DbConstraints.MinToDoNameLength)]
    [InlineData("11111111-1111-1111-1111-111111111111", DbConstraints.MaxToDoNameLength + 1, 1, null, "MaxLength", nameof(UpdateToDoCommand.Title), DbConstraints.MaxToDoNameLength)]
    [InlineData("11111111-1111-1111-1111-111111111111", 5, -1, null, "MinLength", nameof(UpdateToDoCommand.Priority), 0)]
    [InlineData("11111111-1111-1111-1111-111111111111", 5, 1, DbConstraints.MaxToDoNoteLength + 1, "MaxLength", nameof(UpdateToDoCommand.Note), DbConstraints.MaxToDoNoteLength)]
    public void Validate_ShouldReturnError_WhenInputsAreInvalid(
        string id, int titleLength, int priority, int? noteLength,
        string errorType, string propertyName, short limit)
    {
        // Arrange
        var title = new string('a', titleLength);
        var note = noteLength.HasValue ? new string('a', noteLength.Value) : null;
        var command = new UpdateToDoCommand(Guid.Parse(id), title, priority, note);

        var expectedMessage = errorType switch
        {
            "InvalidGuid" => ValidatorMessage.InvalidGuid,
            "MinLength" => ValidatorMessage.MinLength(propertyName, limit),
            "MaxLength" => ValidatorMessage.MaxLength(propertyName, limit),
            _ => throw new ArgumentException("Invalid error type")
        };

        // Act
        (bool isValid, string? errorMessage) = command.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Equal(expectedMessage, errorMessage);
    }

    [Fact]
    public void Validate_ShouldReturnValid_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new UpdateToDoCommand(ToDoTestsConstants.ValidToDoId, ToDoTestsConstants.ValidTitle, ToDoTestsConstants.ValidPriority, ToDoTestsConstants.ValidNote);

        // Act
        (bool isValid, string? errorMessage) = command.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion

    #region HandlerTests

    [Fact]
    public async Task Handle_ShouldReturnErrorResult_WhenEntityNotFound()
    {
        // Arrange
        var command = new UpdateToDoCommand(ToDoTestsConstants.ValidToDoId, ToDoTestsConstants.ValidTitle, ToDoTestsConstants.ValidPriority, ToDoTestsConstants.ValidNote);
        _toDoItemRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ToDoEntity?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorMessage.NotFound, result.Error.Message);
        _toDoItemRepositoryMock
            .Verify(repo => repo.GetByIdAsync(
                command.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResult_WhenSaveChangesFails()
    {
        // Arrange
        var command = new UpdateToDoCommand(ToDoTestsConstants.ValidToDoId, ToDoTestsConstants.ValidTitle, ToDoTestsConstants.ValidPriority, ToDoTestsConstants.ValidNote);
        var existingEntity = new ToDoEntity
        {
            Id = command.Id,
            Title = "Old Title",
            Priority = 0,
            Note = "Old Note"
        };
        _toDoItemRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);
        _toDoItemRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorMessage.SomethingWentWrong, result.Error.Message);
        _toDoItemRepositoryMock
            .Verify(repo => repo.GetByIdAsync(
                command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _toDoItemRepositoryMock
            .Verify(repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUpdatedResource_WhenToDoItemIsUpdated()
    {
        // Arrange
        var command = new UpdateToDoCommand(ToDoTestsConstants.ValidToDoId, ToDoTestsConstants.UpdatedTitle, ToDoTestsConstants.UpdatedPriority, ToDoTestsConstants.UpdatedNote);
        var existingEntity = new ToDoEntity
        {
            Id = command.Id,
            Title = ToDoTestsConstants.ValidTitle,
            Priority = ToDoTestsConstants.ValidPriority,
            Note = ToDoTestsConstants.ValidNote
        };
        _toDoItemRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);
        _toDoItemRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);
        TestUtilities.AssertEntityMatchesDto(existingEntity, result.Data);
        _toDoItemRepositoryMock
            .Verify(repo => repo.GetByIdAsync(
                command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _toDoItemRepositoryMock
            .Verify(repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
