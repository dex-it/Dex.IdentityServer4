using System;
using System.Collections.Generic;
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
    /// The RotatingKeysStore class is an implementation of the ISigningCredentialStore and IValidationKeysStore interface and the IHostedService interface for managing and rotating validation keys in IdentityServer4.
    /// This class ensures that validation keys are periodically rotated and expired keys are removed based on configurable options. 
    /// </summary>
    public class RotatingKeysStore : ISigningCredentialStore, IValidationKeysStore, IHostedService, IDisposable
    {
        private readonly IKeyStore _keyStore;
        private readonly KeyRotationOptions _options;
        private Timer _timer;
        
        public RotatingKeysStore(IKeyStore keyStore, IOptions<KeyRotationOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _keyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));
            _options = options.Value;
        }
        
        /// <inheritdoc />
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            return _keyStore.GetValidationKeysAsync();
        }
        
        /// <inheritdoc />
        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            return _keyStore.GetSigningCredentialsAsync();
        }
        
        private async void RotateKeys(object state)
        {
            var keyInfo = CreateKeyInfo();
            await _keyStore.AddKeyAsync(keyInfo, DateTime.UtcNow.Add(_options.KeyLifetime));
            await _keyStore.RemoveExpiredKeysAsync();
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
    }
}