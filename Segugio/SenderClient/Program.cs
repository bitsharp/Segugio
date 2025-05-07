using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Segugio;
using Segugio.Ports;
using Segugio.Providers;
using SenderClient.Data;
using SenderClient.Ports;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SegugioConnection");

// Aggiungi il DbContext usando SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(connectionString));

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
builder.Services.AddScoped<IUtenteAudit, UtenteConnesso>();
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
    new SerilogProvider("localhost", "514")
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