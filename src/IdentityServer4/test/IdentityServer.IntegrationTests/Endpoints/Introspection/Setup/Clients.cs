// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer.IntegrationTests.Endpoints.Introspection.Setup;

internal class Clients
{
    public static IEnumerable<Client> Get()
    {
        return new List<Client>
        {
            new()
            {
                ClientId = "client1",
                ClientSecrets = new List<Secret>
                {
                    new("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api1", "api2", "api3-a", "api3-b" },
                AccessTokenType = AccessTokenType.Reference
            },
            new()
            {
                ClientId = "client2",
                ClientSecrets = new List<Secret>
                {
                    new("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api1", "api2", "api3-a", "api3-b" },
                AccessTokenType = AccessTokenType.Reference
            },
            new()
            {
                ClientId = "client3",
                ClientSecrets = new List<Secret>
                {
                    new("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api1", "api2", "api3-a", "api3-b" },
                AccessTokenType = AccessTokenType.Reference
            },
            new()
            {
                ClientId = "ro.client",
                ClientSecrets = new List<Secret>
                {
                    new("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = { "api1", "api2", "api3-a", "api3-b" },
                AccessTokenType = AccessTokenType.Reference
            }
        };
    }
}