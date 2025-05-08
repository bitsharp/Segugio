using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Segugio.Ports;

namespace SenderClient.Ports;

public class UtenteConnesso : IUtenteAudit
{
    public UtenteConnesso()
    {
    }

    public string GetUserAccount()
    {
        return "mrossi";
    }

    public string GetRealAccount()
    {
        return "";
    }

    public string GetRoles()
    {
        return "UtenteGenerico";
    }
}