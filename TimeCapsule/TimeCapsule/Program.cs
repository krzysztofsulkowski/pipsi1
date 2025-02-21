using Microsoft.EntityFrameworkCore;
using TimeCapsule;
using TimeCapsule.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddOptions<PortalSettings>()
    .BindConfiguration("PortalSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("Database") ?? throw new ArgumentNullException("ConnectionString");

builder.Services.AddDbContext<TimeCapsuleContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableDetailedErrors();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    if (app.Environment.IsDevelopment())
    {
        endpoints.MapGet("/debug-config", ctx =>
        {
            var config = app.Services.GetRequiredService<IConfiguration>();
            return ctx.Response.WriteAsync((config as IConfigurationRoot).GetDebugView());
        });
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<TimeCapsuleContext>();
    ctx.Database.Migrate();
}
app.Run();
