using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IToDoRepository : IBaseRepository<ToDoEntity>
{
    Task<IReadOnlyList<ToDoEntity>> GetAllOrderedByPriorityAsNoTrackingAsync(CancellationToken cancellationToken);
}
