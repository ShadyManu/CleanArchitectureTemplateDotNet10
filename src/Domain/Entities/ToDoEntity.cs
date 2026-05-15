using Domain.Common.Models;

namespace Domain.Entities;

public class ToDoEntity : BaseAuditableEntity
{
    public required string Title { get; set; }
    public string? Note { get; set; }
    public required int Priority { get; set; }
    public DateTime? Reminder { get; init; }
}
