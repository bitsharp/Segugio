using Audit.Core;
using Segugio.Providers;

namespace Segugio;

internal class SegugioDataProvider
{
    public SegugioDataProvider(string providerType, AuditDataProvider provider, ISegugioProvider.LogTypes logTypes)
    {
        ProviderType = providerType;
        Provider = provider;
        LogTypes = logTypes;
    }

    public string ProviderType { get; }
    public AuditDataProvider Provider { get; }
    public ISegugioProvider.LogTypes LogTypes { get; }
}

internal class CompositeDataProvider : AuditDataProvider
{
    private readonly IList<SegugioDataProvider> _providers;
    
    public CompositeDataProvider(IList<SegugioDataProvider> providers)
    {
        _providers = providers;
    }
    
    public override object InsertEvent(AuditEvent auditEvent)
    {
        foreach (var provider in _providers)
        {
            try
            {
                provider.Provider.InsertEvent(auditEvent);
                LogMessage("Event inserted", provider);
            }
            catch (Exception e)
            {
                LogError("Error inserting event", e, provider);
            }
        }
        return null;
    }

    public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
    {
        foreach (var provider in _providers)
        {
            try
            {
                provider.Provider.ReplaceEvent(eventId, auditEvent);
                LogMessage("Event replaced", provider);
            }
            catch (Exception e)
            {
                LogError("Error replacing event", e, provider);
            }
        }
    }

    public override T GetEvent<T>(object eventId)
    {
        foreach (var provider in _providers)
        {
            try
            {
                var result = provider.Provider.GetEvent<T>(eventId);
                LogMessage("Event retrieved", provider);
                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                LogError("Error getting event", e, provider);
            }
        }
        return default;
    }

    public override async Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var tasks = _providers.Select(provider =>
        {
            try
            {
                var result = provider.Provider.InsertEventAsync(auditEvent);
                LogMessage("Event inserted", provider);
                return result;
            }
            catch (Exception e)
            {
                LogError("Error inserting event", e, provider);
            }

            return default;
        }).ToList();
        return Task.WhenAll(tasks).ContinueWith(_ => (object)null);
    }

    public override async Task ReplaceEventAsync(object eventId, AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var tasks = _providers.Select(provider => 
        {
            try
            {
                var result = provider.Provider.ReplaceEventAsync(eventId, auditEvent);
                LogMessage("Event replaced", provider);
                return result;
            }
            catch (Exception e)
            {
                LogError("Error replacing event", e, provider);
            }

            return default;
        }).ToList();
        Task.WhenAll(tasks);
    }

    public override async Task<T> GetEventAsync<T>(object eventId, CancellationToken cancellationToken)
    {
        foreach (var provider in _providers)
        {
            try
            {
                var result = await provider.Provider.GetEventAsync<T>(eventId);
                LogMessage("Event retrieved", provider);
                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                LogError("Error getting event", e, provider);
            }
        }
        return default;
    }

    private void LogMessage(string message, SegugioDataProvider provider)
    {
        switch (provider.LogTypes) 
        {
            case ISegugioProvider.LogTypes.Console:
                Console.WriteLine($"{message} on {provider.Provider.GetType().Name}");
                break;
            default:
                break;
        };
    }

    private void LogError(string message, Exception e, SegugioDataProvider provider)
    {
        switch (provider.LogTypes) 
        {
            case ISegugioProvider.LogTypes.Console:
                Console.WriteLine(e);
                break;
            case ISegugioProvider.LogTypes.Exception:
                throw new SegugioException($"{message} on {provider.Provider.GetType().Name}", e);
            default:
                break;
        };
    }
}