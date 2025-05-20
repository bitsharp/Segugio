using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Segugio;
using Segugio.Ports;
using Segugio.Providers;
using SenderClient.Data;
using SenderClient.AuditAdapter;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SegugioConnection");

// Aggiungi il DbContext usando SQL Server
builder.Services.AddDbContext<AppDbDbContext>(options =>
    options.UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
);

// Configura Audit
Audit.EntityFramework.Configuration.Setup()
    .ForContext<AppDbDbContext>(config => config
            .IncludeEntityObjects()        // Traccia le entità incluse nelle query
            .AuditEventType("{context} - {action}") // Definisce il tipo di evento
    );


// Aggiungi i servizi per i controller e Swagger
builder.Services.AddControllers(); // Abilita i controller
builder.Services.AddHttpClient(); // Abilita HttpClient
builder.Services.AddHttpContextAccessor(); // Abilita HttpContextAccessor
builder.Services.AddEndpointsApiExplorer(); // Per abilitare endpoint explorer con Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API di esempio",
        Version = "v1"
    });
});

// Configure the CompositeDataProvider
builder.Services.AddScoped<IContestoAudit, ContestoApplicativo>();
builder.Services.AddScoped<ISegugioAuditor, SegugioAuditor>();

var app = builder.Build();

var segugioAuditor = builder.Services.BuildServiceProvider().GetRequiredService<ISegugioAuditor>();
segugioAuditor.Setup(new List<ISegugioProvider>
{
    new SqlServerProvider(
        new AuditTableConfiguration(connectionString,"Audit","EntityAuditLog","UserName","DataJSon", 
            "LastUpdate", "UserRole", "UserAdmin", "Id", "IpAddress","RouteData"
        )
    ),
    new SerilogProvider(new QradarConfiguration("localhost", "514"))
});

// // Configura Swagger per i metodi API
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API di esempio v1");
    });
// }

app.UseAuthorization();

app.MapControllers();

app.Run();