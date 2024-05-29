using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// In-memory implementation of the <see cref="IKeyStore"/> interface.
    /// </summary>
    public class InMemoryKeyStore : IKeyStore
    {
        private readonly List<KeyInfo> _keys = new();
        private readonly object _lock = new();

        /// <summary>
        /// Gets the current validation keys.
        /// </summary>
        /// <returns>An enumerable of <see cref="SecurityKeyInfo"/> representing the validation keys.</returns>
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_keys.Select(k => k.SecurityKeyInfo).AsEnumerable());
            }
        }

        /// <summary>
        /// Gets the current signing credentials.
        /// </summary>
        /// <returns>The current <see cref="SigningCredentials"/>.</returns>
        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            lock (_lock)
            {
                var currentKey = _keys.LastOrDefault();
                if (currentKey != null)
                {
                    return Task.FromResult(new SigningCredentials(currentKey.SecurityKeyInfo.Key, currentKey.SecurityKeyInfo.SigningAlgorithm));
                }

                return Task.FromResult<SigningCredentials>(null);
            }
        }

        /// <summary>
        /// Adds a new key to the store.
        /// </summary>
        /// <param name="keyInfo">The <see cref="SecurityKeyInfo"/> to add.</param>
        /// <param name="expiryDate">The expiration date of the key.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task AddKeyAsync(SecurityKeyInfo keyInfo, DateTime expiryDate)
        {
            lock (_lock)
            {
                _keys.Add(new KeyInfo
                {
                    SecurityKeyInfo = keyInfo,
                    ExpiryDate = expiryDate
                });
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes expired keys from the store.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RemoveExpiredKeysAsync()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                _keys.RemoveAll(k => k.ExpiryDate < now);
            }
            return Task.CompletedTask;
        }

        private record KeyInfo
        {
            public SecurityKeyInfo SecurityKeyInfo { get; init; }
            public DateTime ExpiryDate { get; init; }
        }
    }
}
