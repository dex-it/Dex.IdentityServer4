// ReSharper properties

using System;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Options to control key rotations
    /// </summary>
    public class KeyRotationOptions
    {
        public TimeSpan KeyLifetime { get; set; } = TimeSpan.FromHours(24); // Время жизни ключа
        public TimeSpan KeyRotationInterval { get; set; } = TimeSpan.FromHours(12); // Интервал ротации ключа
    }
    
    /// <summary>
    /// Key rotation options validator
    /// </summary>
    public class KeyRotationOptionsValidator : IValidateOptions<KeyRotationOptions>
    {
        private static readonly TimeSpan MinKeyLifetime = TimeSpan.FromHours(1);
        private static readonly TimeSpan MinKeyRotationInterval = TimeSpan.FromMinutes(30);
        
        public ValidateOptionsResult Validate(string name, KeyRotationOptions options)
        {
            if (options.KeyLifetime < MinKeyLifetime)
            {
                return ValidateOptionsResult.Fail($"KeyLifetime must be at least {MinKeyLifetime.TotalHours} hours.");
            }
            
            if (options.KeyRotationInterval < MinKeyRotationInterval)
            {
                return ValidateOptionsResult.Fail(
                    $"KeyRotationInterval must be at least {MinKeyRotationInterval.TotalMinutes} minutes.");
            }
            
            return ValidateOptionsResult.Success;
        }
    }
}