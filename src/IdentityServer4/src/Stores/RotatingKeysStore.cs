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
    /// The RotatingKeysStore class is an implementation of the ISigningCredentialStore and IValidationKeysStore interface and the IHostedService interface for managing and rotating validation keys in IdentityServer4.
    /// This class ensures that validation keys are periodically rotated and expired keys are removed based on configurable options.
    /// </summary>
    public class RotatingKeysStore : ISigningCredentialStore, IValidationKeysStore, IHostedService, IDisposable
    {
        private readonly IPersistedKeyStore _persistedKeyStore;
        private readonly KeyRotationOptions _options;
        private readonly List<SecurityKeyInfo> _keys = new();
        private readonly object _lock = new();
        private Timer _timer;
        private bool _keysLoaded;
        private static readonly Predicate<SecurityKeyInfo> ExpiredKeyPredicate = k => k.ExpiryDate < DateTime.UtcNow;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="persistedKeyStore"></param>
        public RotatingKeysStore(IOptions<KeyRotationOptions> options, IPersistedKeyStore persistedKeyStore = null)
        {
            _options = options.Value;
            _persistedKeyStore = persistedKeyStore;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            await EnsureKeysLoadedAsync();
            lock (_lock)
            {
                _keys.RemoveAll(ExpiredKeyPredicate);
                return _keys.ToList();
            }
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            await EnsureKeysLoadedAsync();
            lock (_lock)
            {
                _keys.RemoveAll(ExpiredKeyPredicate);
                var currentKey = _keys.LastOrDefault();
                if (currentKey != null)
                {
                    return new SigningCredentials(currentKey.Key, currentKey.SigningAlgorithm);
                }

                return null;
            }
        }

        private async Task EnsureKeysLoadedAsync()
        {
            if (!_keysLoaded)
            {
                if (_persistedKeyStore != null)
                {
                    // load from DB
                    var loadedKeys = await _persistedKeyStore.LoadKeyMaterialsAsync();
                    lock (_lock)
                    {
                        _keys.AddRange(loadedKeys.Select(CreateSecurityKeyInfo));
                        _keys.RemoveAll(ExpiredKeyPredicate);
                        if (_keys.Count == 0)
                        {
                            RotateKeys(null);
                        }
                    }
                }
                else
                {
                    RotateKeys(null);
                }

                _keysLoaded = true;
            }
        }

        private async void RotateKeys(object state)
        {
            await AddNewKeyAsync();
            if (_persistedKeyStore != null)
            {
                await _persistedKeyStore.RemoveExpiredKeyMaterialAsync();
            }
        }

        private async Task AddNewKeyAsync()
        {
            var keyInfo = CreateSecurityKeyInfo();
            lock (_lock)
            {
                _keys.Add(keyInfo);
            }

            if (_persistedKeyStore != null)
            {
                var keyMaterial = new KeyMaterial
                {
                    KeyId = keyInfo.Key.KeyId,
                    Algorithm = keyInfo.SigningAlgorithm,
                    KeyData = ((RsaSecurityKey) keyInfo.Key).Rsa.ExportParameters(false).Modulus,
                    ExpiryDate = keyInfo.ExpiryDate
                };
                await _persistedKeyStore.AddKeyMaterialAsync(keyMaterial);
            }
        }

        /// <summary>
        /// Creates a new SecurityKeyInfo. If keyMaterial is provided, the SecurityKeyInfo will be created from the provided key material.
        /// </summary>
        /// <param name="keyMaterial">Optional key material to create the security key from.</param>
        /// <returns>A new SecurityKeyInfo instance.</returns>
        protected virtual SecurityKeyInfo CreateSecurityKeyInfo(KeyMaterial keyMaterial = null)
        {
            if (keyMaterial != null)
            {
                var rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = keyMaterial.KeyData,
                    Exponent = new byte[] { 1, 0, 1 } // Commonly used exponent
                });

                return new SecurityKeyInfo
                {
                    Key = new RsaSecurityKey(rsa) { KeyId = keyMaterial.KeyId },
                    SigningAlgorithm = keyMaterial.Algorithm,
                    ExpiryDate = keyMaterial.ExpiryDate
                };
            }

            var securityKey = new RsaSecurityKey(RSA.Create()) { KeyId = DateTime.UtcNow.Ticks.ToString() };
            return new SecurityKeyInfo
            {
                Key = securityKey,
                SigningAlgorithm = SecurityAlgorithms.RsaSha256,
                ExpiryDate = DateTime.UtcNow.Add(_options.KeyLifetime)
            };
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RotateKeys, null, TimeSpan.Zero, _options.KeyRotationInterval);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}