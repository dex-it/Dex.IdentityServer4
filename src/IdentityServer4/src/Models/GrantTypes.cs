// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


#pragma warning disable 1591

namespace IdentityServer4.Models;

public static class GrantTypes
{
    public static string[] Implicit => [GrantType.Implicit];

    public static string[] ImplicitAndClientCredentials => [GrantType.Implicit, GrantType.ClientCredentials];

    public static string[] Code => [GrantType.AuthorizationCode];

    public static string[] CodeAndClientCredentials => [GrantType.AuthorizationCode, GrantType.ClientCredentials];

    public static string[] Hybrid => [GrantType.Hybrid];

    public static string[] HybridAndClientCredentials => [GrantType.Hybrid, GrantType.ClientCredentials];

    public static string[] ClientCredentials => [GrantType.ClientCredentials];

    public static string[] ResourceOwnerPassword => [GrantType.ResourceOwnerPassword];

    public static string[] ExternalUserCredentials => [GrantType.ExternalUserCredentials];

    public static string[] ResourceOwnerPasswordAndClientCredentials => [GrantType.ResourceOwnerPassword, GrantType.ClientCredentials];

    public static string[] DeviceFlow => [GrantType.DeviceFlow];
}