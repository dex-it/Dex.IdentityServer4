using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Interface for managing security keys.
    /// </summary>
    public interface IKeyStore
    {
        /// <summary>
        /// Gets the current validation keys.
        /// </summary>
        /// <returns>An enumerable of <see cref="SecurityKeyInfo"/> representing the validation keys.</returns>
        Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync();
        
        /// <summary>
        /// Gets the current signing credentials.
        /// </summary>
        /// <returns>The current <see cref="SigningCredentials"/>.</returns>
        Task<SigningCredentials> GetSigningCredentialsAsync();
        
        /// <summary>
        /// Adds a new key to the store.
        /// </summary>
        /// <param name="keyInfo">The <see cref="SecurityKeyInfo"/> to add.</param>
        /// <param name="expiryDate">The expiration date of the key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddKeyAsync(SecurityKeyInfo keyInfo, DateTime expiryDate);
        
        /// <summary>
        /// Removes expired keys from the store.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveExpiredKeysAsync();
    }
}