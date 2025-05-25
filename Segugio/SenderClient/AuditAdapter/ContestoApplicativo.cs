using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Segugio.Ports;

namespace SenderClient.AuditAdapter;

public class ContestoApplicativo : IContestoAudit
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ContestoApplicativo(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCustomAttribute(string attributeName)
    {
        return attributeName switch
        {
            "IpAddress" => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "",
            "RoutePath" => JsonSerializer.Serialize(_httpContextAccessor.HttpContext?.GetRouteData()??new RouteData()),
            "UserName" => "MROSSI",
            "QueryPath" => $"{_httpContextAccessor.HttpContext?.GetRouteData().Values["controller"]}/{_httpContextAccessor.HttpContext?.GetRouteData().Values["action"]}",
            "Role" => "UtenteGenerico",
            _ => ""
        };
    }
}