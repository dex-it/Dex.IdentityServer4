using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common;

internal class MockKeyMaterialService : IKeyMaterialService
{
    public readonly List<SigningCredentials> SigningCredentials = [];
    public readonly List<SecurityKeyInfo> ValidationKeys = [];

    public Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
    {
        return Task.FromResult(SigningCredentials.AsEnumerable());
    }

    public Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null)
    {
        return Task.FromResult(SigningCredentials.FirstOrDefault());
    }

    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
        return Task.FromResult(ValidationKeys.AsEnumerable());
    }
}