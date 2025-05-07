using Microsoft.AspNetCore.Routing;

namespace Segugio.Extensions;

internal static class RouteDataExtensions
{
    public static string GetApiRoutePath(this RouteData apiPath)
    {
        return $"{apiPath.GetApiController()}.{apiPath.GetApiAction()}";
    }
    
    public static string GetApiController(this RouteData apiPath)
    {
        return $"{apiPath?.Values["Controller"]?.ToString()??""}";
    }
    public static string GetApiAction(this RouteData apiPath)
    {
        return $"{apiPath?.Values["Action"]?.ToString()??""}";
    }
}