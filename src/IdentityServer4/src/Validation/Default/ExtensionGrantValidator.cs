// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Validation;

/// <summary>
/// Validates an extension grant request using the registered validators
/// </summary>
public class ExtensionGrantValidator
{
    private readonly ILogger _logger;
    private readonly FrozenSet<string> _availableGrantTypes;
    private readonly FrozenDictionary<string, IExtensionGrantValidator> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionGrantValidator"/> class.
    /// </summary>
    /// <param name="validators">The validators.</param>
    /// <param name="logger">The logger.</param>
    public ExtensionGrantValidator(IEnumerable<IExtensionGrantValidator> validators, ILogger<ExtensionGrantValidator> logger)
    {
        _logger = logger;

        _validators = validators.ToFrozenDictionary(x => x.GrantType);
        _availableGrantTypes = _validators.Keys.ToFrozenSet();
    }

    /// <summary>
    /// Gets the available grant types.
    /// </summary>
    /// <returns></returns>
    public FrozenSet<string> GetAvailableGrantTypes() => _availableGrantTypes;

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public async Task<GrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
    {
        if (!_validators.TryGetValue(request.GrantType, out var validator))
        {
            _logger.LogError("No validator found for grant type");
            return new GrantValidationResult(TokenRequestErrors.UnsupportedGrantType);
        }

        try
        {
            _logger.LogTrace("Calling into custom grant validator: {Type}", validator.GetType().FullName);

            var context = new ExtensionGrantValidationContext
            {
                Request = request
            };

            await validator.ValidateAsync(context);
            return context.Result;
        }
        catch (Exception e)
        {
            _logger.LogError(1, e, "Grant validation error: {Message}", e.Message);
            return new GrantValidationResult(TokenRequestErrors.InvalidGrant);
        }
    }
}