using System;

namespace IdentityServer4.EntityFramework.Entities;

#pragma warning disable 1591
/// <summary>
/// The KeyEntity model represents an entity for storing information about key material in a database.
/// </summary>
public class KeyEntity
{
    /// <summary>
    /// The unique identifier of the key.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The unique identifier of the key.
    /// </summary>
    public string KeyId { get; set; }
    
    /// <summary>
    /// The encryption algorithm of the key.
    /// </summary>
    public string Algorithm { get; set; }
    
    /// <summary>
    /// The key data as a byte array.
    /// </summary>
    public byte[] KeyData { get; set; }
    
    /// <summary>
    /// The expiration date and time of the key.
    /// </summary>
    public DateTime ExpiryDate { get; set; }
}