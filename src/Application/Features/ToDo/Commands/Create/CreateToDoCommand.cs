using Application.Common.Interfaces.CQRS;
using Application.Common.Interfaces.Repositories;
using Application.Common.Result;
using Application.Dtos.ToDo.Response;
using Application.Mapper;
using Domain.Common.Constants;
using Domain.Entities;

namespace Application.Features.ToDo.Commands.Create;

public record CreateToDoCommand(string Title, int Priority, string? Note = null, DateTime? Reminder = null)
    : ICommand<ToDoResponse?>
{
    private const short MinTitleLength = DbConstraints.MinToDoNameLength;
    private const short MaxTitleLength = DbConstraints.MaxToDoNameLength;

    public (bool IsValid, string? ErrorMessage) Validate()
    {
        return Title.Length switch
        {
            < MinTitleLength => (false, ValidatorMessage.MinLength(nameof(Title), MinTitleLength)),
            > MaxTitleLength => (false, ValidatorMessage.MaxLength(nameof(Title), MaxTitleLength)),
            _ => Priority < 0 ? (false, ValidatorMessage.MinLength(nameof(Priority), 0)) : (true, null)
        };
    }
}

internal sealed class CreateToDoCommandHandler : ICommandHandler<CreateToDoCommand, ToDoResponse?>
{
    private readonly IToDoRepository _toDoRepository;

    public CreateToDoCommandHandler(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public async Task<Result<ToDoResponse?>> Handle(CreateToDoCommand request, CancellationToken cancellationToken)
    {
        var entity = new ToDoEntity
        {
            Title = request.Title,
            Priority = request.Priority,
            Note = request.Note,
            Reminder = request.Reminder
        };

        await _toDoRepository.AddAsync(entity, cancellationToken);
        var result = await _toDoRepository.SaveChangesAsync(cancellationToken);
        if (result is 0)
        {
            return Result<ToDoResponse?>.Failure(ErrorMessage.SomethingWentWrong);
        }

        var dto = entity.ToDto();
        return Result<ToDoResponse?>.Success(dto);
    }
}
