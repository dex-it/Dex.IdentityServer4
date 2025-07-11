using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityServer.UnitTests.Validation;

public class RotationKeysTokenValidation
{
    private const string Category = "Rotation kes, identity token validation";
    
    static RotationKeysTokenValidation()
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_IdentityToken_DefaultKeyType()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var token = TokenFactory.CreateIdentityToken("roclient", "valid");
        var jwt = await creator.CreateTokenAsync(token);
        
        var validKey = new SecurityKeyInfo
        {
            Key = TestCert.LoadSigningCredentials().Key,
            SigningAlgorithm = "RS256"
        };
        var validator = Factory.CreateTokenValidator(keys: [CreateKey(), CreateKey(), validKey]);
        var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
        
        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task InValid_IdentityToken_DefaultKeyType()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var token = TokenFactory.CreateIdentityToken("roclient", "valid");
        var jwt = await creator.CreateTokenAsync(token);
        
        var validator = Factory.CreateTokenValidator(keys: [CreateKey(), CreateKey()]);
        var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
        
        result.IsError.Should().BeTrue();
    }
    
    private static SecurityKeyInfo CreateKey()
    {
        return new SecurityKeyInfo
        {
            Key = new RsaSecurityKey(RSA.Create()) { KeyId = DateTime.UtcNow.Ticks.ToString() },
            SigningAlgorithm = SecurityAlgorithms.RsaSha256
        };
    }
}