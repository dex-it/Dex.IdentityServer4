using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.EntityFramework.Stores;

/// <inheritdoc />
public class PersistedKeyStore : IPersistedKeyStore
{
    private readonly KeyStoreDbContext _dbContext;
    
    public PersistedKeyStore(KeyStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    /// <inheritdoc />
    public async Task AddKeyMaterialAsync(KeyMaterial keyMaterial)
    {
        if (keyMaterial == null) throw new ArgumentNullException(nameof(keyMaterial));
        
        var keyEntity = new KeyEntity
        {
            KeyId = keyMaterial.KeyId,
            Algorithm = keyMaterial.Algorithm,
            KeyData = keyMaterial.KeyData,
            ExpiryDate = keyMaterial.ExpiryDate
        };
        
        _dbContext.Keys.Add(keyEntity);
        await _dbContext.SaveChangesAsync();
    }
    
    /// <inheritdoc />
    public async Task RemoveExpiredKeyMaterialAsync()
    {
        var now = DateTime.UtcNow;
        
        var expiredKeys = await _dbContext.Keys
            .Where(k => k.ExpiryDate < now)
            .ToListAsync();
        
        _dbContext.Keys.RemoveRange(expiredKeys);
        await _dbContext.SaveChangesAsync();
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<KeyMaterial>> LoadKeyMaterialsAsync()
    {
        var now = DateTime.UtcNow;
        
        var keys = await _dbContext.Keys
            .Where(k => k.ExpiryDate > now)
            .ToListAsync();
        
        return keys
            .Select(k => new KeyMaterial
            {
                KeyId = k.KeyId,
                Algorithm = k.Algorithm,
                KeyData = k.KeyData,
                ExpiryDate = k.ExpiryDate
            });
    }
}