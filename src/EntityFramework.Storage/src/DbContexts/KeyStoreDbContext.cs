using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.EntityFramework.DbContexts;

/// <summary>
/// Context for keys
/// </summary>
public class KeyStoreDbContext : DbContext
{
    public DbSet<KeyEntity> Keys { get; set; }
    
    public KeyStoreDbContext(DbContextOptions<KeyStoreDbContext> options)
        : base(options)
    {
    }
}