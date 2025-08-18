// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer.IntegrationTests.Clients.Setup;

public class ExtensionGrantValidator : IExtensionGrantValidator
{
    public Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var credential = context.Request.Raw.Get("custom_credential");
        var extraClaim = context.Request.Raw.Get("extra_claim");

        if (credential is null)
        {
            // custom error message
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid_custom_credential");
            return Task.CompletedTask;
        }

        context.Result = context.Result = new GrantValidationResult(
            "818727",
            claims: extraClaim is not null ? [new Claim("extra_claim", extraClaim)] : null,
            authenticationMethod: GrantType);

        return Task.CompletedTask;
    }

    public string GrantType { get; } = "custom";
}