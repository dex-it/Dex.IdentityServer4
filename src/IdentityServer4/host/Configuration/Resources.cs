// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityServerHost.Configuration;

public class Resources
{
    // identity resources represent identity data about a user that can be requested via the scope parameter (OpenID Connect)
    public static readonly IEnumerable<IdentityResource> IdentityResources =
    [
        // some standard scopes from the OIDC spec
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),

            // custom identity resource with some consolidated claims
            new("custom.profile", [JwtClaimTypes.Name, JwtClaimTypes.Email, "location", JwtClaimTypes.Address
        ])
    ];

    // API scopes represent values that describe scope of access and can be requested by the scope parameter (OAuth)
    public static readonly IEnumerable<ApiScope> ApiScopes =
    [
        // local API scope
            new(LocalApi.ScopeName),

            // resource specific scopes
            new("resource1.scope1"),
            new("resource2.scope1"), 
                
            // a scope without resource association
            new("scope3"),
                
            // a scope shared by multiple resources
            new("shared.scope"),

            // a parameterized scope
            new("transaction", "Transaction")
            {
                Description = "Some Transaction"
            }
    ];

    // API resources are more formal representation of a resource with processing rules and their scopes (if any)
    public static readonly IEnumerable<ApiResource> ApiResources =
    [
        new("resource1", "Resource 1")
            {
                ApiSecrets = { new Secret("secret".Sha256()) },

                Scopes = { "resource1.scope1", "shared.scope" }
            },
                
            new("resource2", "Resource 2")
            {
                ApiSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // additional claims to put into access token
                UserClaims =
                {
                    JwtClaimTypes.Name,
                    JwtClaimTypes.Email
                },

                Scopes = { "resource2.scope1", "shared.scope" }
            }
    ];
}