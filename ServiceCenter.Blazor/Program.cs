using Microsoft.EntityFrameworkCore;
using ServiceCenter.Blazor.Components;
using ServiceCenter.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(AppDbContext.ConnectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DbSeed.Ensure(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
