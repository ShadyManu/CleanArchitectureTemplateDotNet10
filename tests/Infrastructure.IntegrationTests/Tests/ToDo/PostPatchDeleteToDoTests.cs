using System.Net;
using System.Net.Http.Json;
using Application.Common.Result;
using Application.Dtos.ToDo.Request;
using Application.Dtos.ToDo.Response;
using Domain.Common.Constants;
using Infrastructure.IntegrationTests.Utilities;
using Xunit;

namespace Infrastructure.IntegrationTests.Tests.ToDo;

public class PostPatchDeleteToDoTests : BaseToDoTest
{
    public PostPatchDeleteToDoTests(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    #region Tests

    // POST Tests
    [Fact]
    public async Task PostToDo_ShouldCreateEntity_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateToDoRequest
        {
            Title = "New ToDo",
            Priority = 4,
            Note = "This is a new ToDo item",
            Reminder = DateTime.UtcNow
        };

        // Act
        var response = await _httpClientAnonymous.PostAsJsonAsync(BaseEndpoint, request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<ToDoResponse>>();

        Assert.NotNull(result?.Data);
        Assert.Null(result.Error);
        Assert.Equal(request.Title, result.Data.Title);
        Assert.Equal(request.Priority, result.Data.Priority);
        Assert.Equal(request.Note, result.Data.Note);
    }

    [Fact]
    public async Task PostToDo_ShouldReturnBadRequest_WhenPayloadIsMissing()
    {
        // Act
        var response = await _httpClientAnonymous.PostAsJsonAsync(BaseEndpoint, new object());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", 0, 1, "MinLength", nameof(CreateToDoRequest.Title), DbConstraints.MinToDoNameLength)]
    [InlineData("POST", DbConstraints.MaxToDoNameLength + 1, 1, "MaxLength", nameof(CreateToDoRequest.Title), DbConstraints.MaxToDoNameLength)]
    [InlineData("PATCH", DbConstraints.MaxToDoNameLength + 1, 1, "MaxLength", nameof(UpdateToDoRequest.Title), DbConstraints.MaxToDoNameLength)]
    public async Task PostPatch_ShouldReturnValidationError_WhenInputsAreInvalid(
        string method, int titleLength, int priority, string errorType, string propertyName, short limit)
    {
        // Arrange
        var title = new string('a', titleLength);
        HttpResponseMessage response;

        if (method == "POST")
        {
            var request = new CreateToDoRequest { Title = title, Priority = priority };
            response = await _httpClientAnonymous.PostAsJsonAsync(BaseEndpoint, request);
        }
        else
        {
            var request = new UpdateToDoRequest { Id = FirstToDoId, Title = title, Priority = priority };
            response = await _httpClientAnonymous.PatchAsJsonAsync(BaseEndpoint, request);
        }

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<ToDoResponse>>();
        Assert.NotNull(result?.Error);

        var expectedMessage = errorType switch
        {
            "MinLength" => ValidatorMessage.MinLength(propertyName, limit),
            "MaxLength" => ValidatorMessage.MaxLength(propertyName, limit),
            _ => throw new ArgumentException("Invalid error type")
        };

        Assert.Contains(expectedMessage, result.Error.Message);
    }

    [Fact]
    public async Task PostToDo_ShouldCancelRequest_WhenRequestTimesOut()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(0));

        var request = new CreateToDoRequest
        {
            Title = "New ToDo",
            Priority = 4,
            Note = "This is a new ToDo item",
            Reminder = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await _httpClientAnonymous.PostAsJsonAsync(BaseEndpoint, request, cts.Token);
        });
    }

    // PATCH Tests
    [Fact]
    public async Task PatchToDo_ShouldUpdateEntity_WhenRequestIsValid()
    {
        // Arrange
        var updateRequest = new UpdateToDoRequest
        {
            Id = FirstToDoId,
            Title = "Updated ToDo",
            Priority = 5,
            Note = "Updated Note"
        };

        // Act
        var response = await _httpClientAnonymous.PatchAsJsonAsync(BaseEndpoint, updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<ToDoResponse>>();

        Assert.NotNull(result?.Data);
        Assert.Null(result.Error);
        TestUtilities.AssertEntityMatchesDto(updateRequest, result.Data);
    }

    [Fact]
    public async Task PatchToDo_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        // Act
        var response = await _httpClientAnonymous.PatchAsJsonAsync(BaseEndpoint, new object());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchToDo_ShouldCancelRequest_WhenRequestTimesOut()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(0));

        var updateRequest = new UpdateToDoRequest
        {
            Id = FirstToDoId,
            Title = "Updated ToDo",
            Priority = 5,
            Note = "Updated Note"
        };

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await _httpClientAnonymous.PatchAsJsonAsync(BaseEndpoint, updateRequest, cts.Token);
        });
    }

    [Fact]
    public async Task DeleteToDo_ShouldRemoveEntity_WhenIdIsValid()
    {
        // Arrange
        var idToDelete = SecondToDoId;

        // Act
        var response = await _httpClientAnonymous.DeleteAsync($"{BaseEndpoint}/{idToDelete}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.True(result?.Data);
    }

    [Fact]
    public async Task DeleteToDo_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // Arrange
        var nonExistingId = Guid.CreateVersion7();

        // Act
        var response = await _httpClientAnonymous.DeleteAsync($"{BaseEndpoint}/{nonExistingId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.NotNull(result?.Error);
        Assert.Equal(ErrorMessage.NotFound, result.Error.Message);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task DeleteToDo_ShouldReturnValidationError_WhenIdIsInvalid(string invalidIdStr)
    {
        // Arrange
        var invalidId = Guid.Parse(invalidIdStr);

        // Act
        var response = await _httpClientAnonymous.DeleteAsync($"{BaseEndpoint}/{invalidId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.NotNull(result?.Error);
        Assert.Contains(ValidatorMessage.InvalidGuid, result.Error.Message);
    }

    [Fact]
    public async Task DeleteToDo_ShouldCancelRequest_WhenRequestTimesOut()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(0));

        var idToDelete = SecondToDoId;

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await _httpClientAnonymous.DeleteAsync($"{BaseEndpoint}/{idToDelete}", cts.Token);
        });
    }

    #endregion
}
