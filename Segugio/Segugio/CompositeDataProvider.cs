using Audit.Core;

namespace Segugio;

internal class CompositeDataProvider : AuditDataProvider
{
    private readonly IList<AuditDataProvider> _providers;
    
    public CompositeDataProvider(IList<AuditDataProvider> providers)
    {
        _providers = providers;
    }
    
    public override object InsertEvent(AuditEvent auditEvent)
    {
        foreach (var provider in _providers)
        {
            provider.InsertEvent(auditEvent);
        }
        return null;
    }

    public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
    {
        foreach (var provider in _providers)
        {
            provider.ReplaceEvent(eventId, auditEvent);
        }
    }

    public override T GetEvent<T>(object eventId)
    {
        foreach (var provider in _providers)
        {
            var result = provider.GetEvent<T>(eventId);
            if (result != null)
            {
                return result;
            }
        }
        return default;
    }

    public override async Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var tasks = _providers.Select(provider => provider.InsertEventAsync(auditEvent)).ToList();
        return Task.WhenAll(tasks).ContinueWith(_ => (object)null);
    }

    public override async Task ReplaceEventAsync(object eventId, AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        var tasks = _providers.Select(provider => provider.ReplaceEventAsync(eventId, auditEvent)).ToList();
        Task.WhenAll(tasks);
    }

    public override async Task<T> GetEventAsync<T>(object eventId, CancellationToken cancellationToken)
    {
        foreach (var provider in _providers)
        {
            var result = await provider.GetEventAsync<T>(eventId);
            if (result != null)
            {
                return result;
            }
        }
        return default;
    }
}