using System;

namespace IdentityServer4.Models;

/// <summary>
/// The KeyMaterial class represents the material of a cryptographic key, containing information such as the key identifier, algorithm, key data, and expiration date.
/// </summary>
public class KeyMaterial
{
    /// <summary>
    /// Gets or sets the unique identifier of the key.
    /// </summary>
    public string KeyId { get; set; }
    
    /// <summary>
    /// Gets or sets the algorithm used by the key.
    /// </summary>
    public string Algorithm { get; set; }
    
    /// <summary>
    /// Gets or sets the binary data of the key.
    /// </summary>
    public byte[] KeyData { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration date and time of the key.
    /// </summary>
    public DateTime ExpiryDate { get; set; }
}

