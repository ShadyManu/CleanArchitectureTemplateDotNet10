using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.IntegrationTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.IntegrationTests.Tests.ToDo;

public abstract partial class BaseToDoTest : BaseIntegrationTest<ToDoEntity>
{
    protected BaseToDoTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await context.ToDos.AddRangeAsync(ToDoEntitiesSeed);
        await context.SaveChangesAsync();
    }
    
    public override async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await context.ToDos.ExecuteDeleteAsync();
        
        await base.DisposeAsync();
    }
}

// Seed Data
public abstract partial class BaseToDoTest
{
    protected const string BaseEndpoint = "/api/todo";
    
    protected static readonly Guid FirstToDoId = Guid.CreateVersion7();
    protected static readonly Guid SecondToDoId = Guid.CreateVersion7();
    
    protected static readonly IReadOnlyList<ToDoEntity> ToDoEntitiesSeed =
    [
        new()
        {
            Id = FirstToDoId,
            Title = "First ToDo",
            Priority = 1,
            Note = "This is the first to-do item."
        },
        new()
        {
            Id = SecondToDoId,
            Title = "Second ToDo",
            Priority = 2,
            Note = "This is the second to-do item."
        },
    ];
}
