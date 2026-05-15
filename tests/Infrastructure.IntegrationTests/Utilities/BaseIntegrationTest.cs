using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Infrastructure.IntegrationTests.Utilities;

public abstract class BaseIntegrationTest<TEntity> : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
    where TEntity : class
{
    protected readonly HttpClient _httpClientAnonymous;
    protected readonly IntegrationTestWebAppFactory _factory;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        
        _httpClientAnonymous = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public virtual async Task InitializeAsync() => await Task.CompletedTask; 
    
    public virtual async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dbSet = context.Set<TEntity>();
        dbSet.RemoveRange(dbSet);
        await context.SaveChangesAsync();
    }
}
