using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// The RotatingValidationKeysStore class is an implementation of the IValidationKeysStore interface and the IHostedService interface for managing and rotating validation keys in IdentityServer4.
    /// This class ensures that validation keys are periodically rotated and expired keys are removed based on configurable options. 
    /// </summary>
    public class RotatingKeysStore : ISigningCredentialStore, IValidationKeysStore,
        IHostedService, IDisposable
    {
        private readonly List<KeyInfo> _keys;
        private readonly KeyRotationOptions _options;
        private readonly object _lock = new();
        private Timer _timer;
        
        internal RotatingKeysStore(IOptions<KeyRotationOptions> options)
        {
            _options = options.Value;
            _keys = new List<KeyInfo>();
        }
        
        /// <inheritdoc />
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            lock (_lock)
            {
                RemoveExpiredKeys();
                return Task.FromResult(_keys.Select(k => k.SecurityKeyInfo));
            }
        }
        
        /// <inheritdoc />
        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            lock (_lock)
            {
                RemoveExpiredKeys();
                var currentKey = _keys.LastOrDefault();
                if (currentKey != null)
                {
                    return Task.FromResult(new SigningCredentials(currentKey.SecurityKeyInfo.Key,
                        currentKey.SecurityKeyInfo.SigningAlgorithm));
                }
                
                return Task.FromResult<SigningCredentials>(null);
            }
        }
        
        private void RemoveExpiredKeys()
        {
            var now = DateTime.UtcNow;
            _keys.RemoveAll(k => k.ExpiryDate < now);
        }
        
        private void RotateKeys(object state)
        {
            lock (_lock)
            {
                AddNewKey();
                RemoveExpiredKeys();
            }
        }
        
        private void AddNewKey()
        {
            var keyInfo = CreateKeyInfo();
            
            _keys.Add(new KeyInfo
            {
                SecurityKeyInfo = keyInfo,
                ExpiryDate = DateTime.UtcNow.Add(_options.KeyLifetime)
            });
        }
        
        /// <summary>
        /// Override your creation keys logic.
        /// Default: RSA key be creating
        /// </summary>
        /// <returns></returns>
        protected virtual SecurityKeyInfo CreateKeyInfo()
        {
            return new SecurityKeyInfo
            {
                Key = new RsaSecurityKey(RSA.Create()) { KeyId = DateTime.UtcNow.Ticks.ToString() },
                SigningAlgorithm = SecurityAlgorithms.RsaSha256
            };
        }
        
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RotateKeys, null, TimeSpan.Zero, _options.KeyRotationInterval);
            return Task.CompletedTask;
        }
        
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Clear all resources
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }
        
        private class KeyInfo
        {
            public SecurityKeyInfo SecurityKeyInfo { get; init; }
            public DateTime ExpiryDate { get; init; }
        }
    }
}