// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

#pragma warning disable 1591

namespace IdentityServer4.Models;

public class GrantTypes
{
    public static ICollection<string> Implicit =>
        [GrantType.Implicit];

    public static ICollection<string> ImplicitAndClientCredentials =>
        [GrantType.Implicit, GrantType.ClientCredentials];

    public static ICollection<string> Code =>
        [GrantType.AuthorizationCode];

    public static ICollection<string> CodeAndClientCredentials =>
        [GrantType.AuthorizationCode, GrantType.ClientCredentials];

    public static ICollection<string> Hybrid =>
        [GrantType.Hybrid];

    public static ICollection<string> HybridAndClientCredentials =>
        [GrantType.Hybrid, GrantType.ClientCredentials];

    public static ICollection<string> ClientCredentials =>
        [GrantType.ClientCredentials];

    public static ICollection<string> ResourceOwnerPassword =>
        [GrantType.ResourceOwnerPassword];

    public static ICollection<string> ResourceOwnerPasswordAndClientCredentials =>
        [GrantType.ResourceOwnerPassword, GrantType.ClientCredentials];

    public static ICollection<string> DeviceFlow =>
        [GrantType.DeviceFlow];
}