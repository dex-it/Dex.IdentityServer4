// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.IntegrationTests.Common;
using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityServer.IntegrationTests.Clients.Setup;

internal class Clients
{
    public static IEnumerable<Client> Get()
    {
        return new List<Client>
        {
            ///////////////////////////////////////////
            // Console Client Credentials Flow Sample
            //////////////////////////////////////////
            new()
            {
                ClientId = "client",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowOfflineAccess = true,

                AllowedScopes =
                {
                    "api1", "api2", "other_api"
                }
            },
            new()
            {
                ClientId = "client.cnf",
                ClientSecrets =
                {
                    new Secret
                    {
                        Type = "confirmation.test",
                        Description = "Test for cnf claim",
                        Value = "foo"
                    }
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowOfflineAccess = true,

                AllowedScopes =
                {
                    "api1", "api2"
                }
            },
            new()
            {
                ClientId = "client.and.ro",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,

                AllowedScopes =
                {
                    "openid",
                    "api1", "api2"
                }
            },
            new()
            {
                ClientId = "client.identityscopes",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,

                AllowedScopes =
                {
                    "openid", "profile",
                    "api1", "api2"
                }
            },
            new()
            {
                ClientId = "client.no_default_scopes",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials
            },
            new()
            {
                ClientId = "client.no_secret",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireClientSecret = false,
                AllowedScopes = { "api1" }
            },

            ///////////////////////////////////////////
            // Console Resource Owner Flow Sample
            //////////////////////////////////////////
            new()
            {
                ClientId = "roclient",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,

                AllowOfflineAccess = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Address,
                    "roles",
                    "api1", "api2", "api4.with.roles"
                }
            },
            new()
            {
                ClientId = "roclient.reuse",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                AllowOfflineAccess = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Address,
                    "roles",
                    "api1", "api2", "api4.with.roles"
                },

                RefreshTokenUsage = TokenUsage.ReUse
            },

            /////////////////////////////////////////
            // Console Custom Grant Flow Sample
            ////////////////////////////////////////
            new()
            {
                ClientId = "client.custom",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = { "custom", "custom.nosubject" },

                AllowedScopes =
                {
                    "api1", "api2"
                },

                AllowOfflineAccess = true
            },
            new()
            {
                ClientId = "client.dynamic",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = { "dynamic" },

                AllowedScopes =
                {
                    "api1", "api2"
                },

                AlwaysSendClientClaims = true
            },

            ///////////////////////////////////////////
            // Introspection Client Sample
            //////////////////////////////////////////
            new()
            {
                ClientId = "roclient.reference",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                AllowOfflineAccess = true,
                AllowedScopes =
                {
                    "api1", "api2"
                },

                AccessTokenType = AccessTokenType.Reference
            },

            new()
            {
                ClientName = "Client with Base64 encoded X509 Certificate",
                ClientId = "certificate_base64_valid",
                Enabled = true,

                ClientSecrets =
                {
                    new Secret
                    {
                        Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
                        Value = Convert.ToBase64String(TestCert.Load().Export(X509ContentType.Cert))
                    }
                },

                AllowedGrantTypes = GrantTypes.ClientCredentials,

                AllowedScopes = new List<string>
                {
                    "api1", "api2"
                }
            },

            new()
            {
                ClientId = "implicit",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = {"api1"},
                RedirectUris = { "http://implicit" }
            },
            new()
            {
                ClientId = "implicit_and_client_creds",
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                AllowedScopes = {"api1"},
                RedirectUris = { "http://implicit_and_client_creds" }
            }
        };
    }
}