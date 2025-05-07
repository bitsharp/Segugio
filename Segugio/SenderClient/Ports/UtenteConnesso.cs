using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Segugio.Ports;

namespace SenderClient.Ports;

public class UtenteConnesso : IUtenteAudit
{
    public UtenteConnesso()
    {
    }

    public string GetNetworkAccount()
    {
        return "mrossi";
    }

    public string GetImpersonatedAccount()
    {
        return "";
    }

    public string GetRoles()
    {
        return "UtenteGenerico";
    }
}