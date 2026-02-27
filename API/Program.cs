using API.Extensions;
using Application.Repositories;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Infrastructure.Repositories;
using Infrastructure.Security.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;


//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//    .MinimumLevel.Debug()
//    .Enrich.FromLogContext()
//    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
//    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "nxt_logs_.txt"),
//                  rollingInterval: RollingInterval.Day,
//                  restrictedToMinimumLevel: LogEventLevel.Information)
//    .CreateLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

    builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
    })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddInfrastructureJwtAuthentication(builder.Configuration);

    builder.Services.AddHttpContextAccessor();


    builder.Services.AddScoped<Application.Abstractions.IUnitOfWork, Infrastructure.Persistence.UnitOfWork.UnitOfWork>();

    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());




    builder.Services.AddEntityServices();

    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"API.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer"
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();
    builder.Logging.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Logging.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", Microsoft.Extensions.Logging.LogLevel.Warning);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@local";
        var adminPwd = builder.Configuration["Admin:Password"] ?? "Admin123!";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FirstName = "Sys", LastName = "Admin" };
            var res = await userManager.CreateAsync(admin, adminPwd);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }


    app.UseMiddleware<API.Middlewares.ExceptionMiddleware>();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
  


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}