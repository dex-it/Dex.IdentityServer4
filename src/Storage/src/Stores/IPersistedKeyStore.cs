using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for a persisted key store used to save and load key materials.
    /// </summary>
    public interface IPersistedKeyStore
    {
        /// <summary>
        /// Adds a new key material to the store.
        /// </summary>
        /// <param name="keyMaterial">The key material to add.</param>
        Task AddKeyMaterialAsync(KeyMaterial keyMaterial);
        
        /// <summary>
        /// Removes expired key materials from the store.
        /// </summary>
        Task RemoveExpiredKeyMaterialAsync();
        
        /// <summary>
        /// Loads key materials from the store.
        /// </summary>
        /// <returns>A collection of key materials.</returns>
        Task<IEnumerable<KeyMaterial>> LoadKeyMaterialsAsync();
    }
}