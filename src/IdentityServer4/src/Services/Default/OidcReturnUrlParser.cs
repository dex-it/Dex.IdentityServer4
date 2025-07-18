﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using IdentityServer4.Stores;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace IdentityServer4.Services;

internal class OidcReturnUrlParser : IReturnUrlParser
{
    private readonly IAuthorizeRequestValidator _validator;
    private readonly IUserSession _userSession;
    private readonly ILogger _logger;
    private readonly IAuthorizationParametersMessageStore? _authorizationParametersMessageStore;

    public OidcReturnUrlParser(
        IAuthorizeRequestValidator validator,
        IUserSession userSession,
        ILogger<OidcReturnUrlParser> logger,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
    {
        _validator = validator;
        _userSession = userSession;
        _logger = logger;
        _authorizationParametersMessageStore = authorizationParametersMessageStore;
    }


    public async Task<AuthorizationRequest?> ParseAsync(string returnUrl)
    {
        if (IsValidReturnUrl(returnUrl))
        {
            var parameters = returnUrl.ReadQueryStringAsNameValueCollection();

            if (_authorizationParametersMessageStore is not null)
            {
                var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                parameters = entry.Data.FromFullDictionary();
            }

            // Добавлено после обновления до net8
            // распарсить JWT из параметра "request" и добавить кастомные claims в parameters
            // чтоб соответствовать логике автотеста authorize_should_accept_complex_objects_in_request_object
            var requestJwt = parameters["request"];
            if (string.IsNullOrEmpty(requestJwt) is false)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(requestJwt))
                    {
                        var jwt = handler.ReadJwtToken(requestJwt);
                        var claims = jwt.Claims.ToArray();
                        
                        foreach (var claim in claims)
                            if (parameters[claim.Type] is null)
                                parameters.Add(claim.Type, claim.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to parse 'request' JWT: {Exception}", ex);
                }
            }

            var user = await _userSession.GetUserAsync();
            var result = await _validator.ValidateAsync(parameters, user);
            if (!result.IsError)
            {
                _logger.LogTrace("AuthorizationRequest being returned");
                return new AuthorizationRequest(result.ValidatedRequest);
            }
        }

        _logger.LogTrace("No AuthorizationRequest being returned");
        return null;
    }

    public bool IsValidReturnUrl(string returnUrl)
    {
        if (returnUrl.IsLocalUrl())
        {
            var index = returnUrl.IndexOf('?');
            if (index >= 0)
            {
                returnUrl = returnUrl[..index];
            }

            if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
            {
                _logger.LogTrace("returnUrl is valid");
                return true;
            }
        }

        _logger.LogTrace("returnUrl is not valid");
        return false;
    }
}