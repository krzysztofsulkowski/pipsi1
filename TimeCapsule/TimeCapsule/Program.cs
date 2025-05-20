using Microsoft.EntityFrameworkCore;
using TimeCapsule;
using TimeCapsule.Models;
using Microsoft.AspNetCore.Identity;
using TimeCapsule.Services;
using TimeCapsule.Interfaces;
using TimeCapsule.Seeders;
using TimeCapsule.Extensions;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<TimeCapsuleContext>().AddDefaultTokenProviders().AddDefaultUI();

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Time Capsule", Version = "v1" });
});

var supportedCultures = new[]
{
    new CultureInfo("pl-PL"),
    new CultureInfo("en-US")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pl-PL");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});


builder.Services
    .AddOptions<PortalSettings>()
    .BindConfiguration("PortalSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<CapsuleService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IFormManagementService, FormManagementService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddScoped<SectionSeeder>();
builder.Services.AddScoped<QuestionSeeder>();
builder.Services.AddScoped<SeedManager>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

var connectionString = builder.Configuration.GetConnectionString("Database") ?? throw new ArgumentNullException("ConnectionString");

builder.Services.AddDbContext<TimeCapsuleContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableDetailedErrors();
});

builder.Services.AddMemoryCache();

//Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

await app.UseSeeders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/swagger"), subApp =>
{
    subApp.UseAuthentication();
    subApp.UseAuthorization();

    subApp.Use(async (ctx, next) =>
    {
        if (!ctx.User.Identity.IsAuthenticated || !ctx.User.IsInRole("Admin"))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsync("Dostêp zabroniony. Wymagane uprawnienia administratora.");
            return;
        }
        await next();
    });

    // Swagger i UI tylko dla Admin
    subApp.UseSwagger();
    subApp.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeCapsule V1");
    });
});


//middleware
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions.Value);


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
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

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<TimeCapsuleContext>();
    ctx.Database.Migrate();
}
app.Run();

