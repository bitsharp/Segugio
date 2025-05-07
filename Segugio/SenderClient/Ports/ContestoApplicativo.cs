using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Segugio.Ports;

namespace SenderClient.Ports;

public class ContestoApplicativo : IContestoAudit
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ContestoApplicativo(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetRemoteIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
    }

    public string GetSessionId()
    {
        return "sessionId"; //_httpContextAccessor.HttpContext?.Session.Id ?? "";
    }

    public string GetTerminalId()
    {
        return "terminalId"; //_httpContextAccessor.HttpContext?.Session.Id ?? "";
    }

    public RouteData GetHttpRouteData()
    {
        return _httpContextAccessor.HttpContext?.GetRouteData();
    }
}