using Application;
using Carter;
using Infrastructure;
using Infrastructure.Data;
using Presentation;
using Web;
using Web.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services from the different layers.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationDependencyInjection();
builder.Services.AddPresentationDependencyInjection();
builder.Services.AddWebDependencyInjection();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors(CorsPolicyConstants.LocalPolicy);
}
else
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseCors(CorsPolicyConstants.ProdPolicy);
}

var shouldApplyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigration");
if (shouldApplyMigrations)
{
    await app.Services.MigrateAsync();
}

var shouldSeedData = app.Configuration.GetValue<bool>("Database:SeedData");
if (shouldSeedData)
{
    await app.Services.SeedAsync();
}

app.MapFallbackToFile("index.html");

app.UseForwardedHeaders();

app.UseRouting();

app.UseRateLimiter();

app.MapCarter(); // Will add with reflection all endpoints which extend ICarterModule

app.Run();

namespace Web
{
    public partial class Program { }
}
