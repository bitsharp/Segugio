using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Segugio.Context;

public class SegugioDbContext : AuditDbContext
{
    public SegugioDbContext(DbContextOptions options) : base(options)
    {
    }
}