// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace IdentityServer4.Validation;

/// <summary>
/// Validates JWT authorization request objects
/// </summary>
public class JwtRequestValidator
{
    private readonly string _audienceUri;
    private readonly IHttpContextAccessor _httpContextAccessor;
        
    /// <summary>
    /// JWT handler
    /// </summary>
    protected readonly JwtSecurityTokenHandler Handler = new()
    {
        MapInboundClaims = false
    };

    /// <summary>
    /// The audience URI to use
    /// </summary>
    protected string AudienceUri
    {
        get
        {
            if (_audienceUri.IsPresent())
            {
                return _audienceUri;
            }

            return _httpContextAccessor.HttpContext.GetIdentityServerIssuerUri();
        }
    }

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;
        
    /// <summary>
    /// The optione
    /// </summary>
    protected readonly IdentityServerOptions Options;

    /// <summary>
    /// Instantiates an instance of private_key_jwt secret validator
    /// </summary>
    public JwtRequestValidator(IHttpContextAccessor contextAccessor, IdentityServerOptions options, ILogger<JwtRequestValidator> logger)
    {
        _httpContextAccessor = contextAccessor;
            
        Options = options;
        Logger = logger;
    }

    /// <summary>
    /// Instantiates an instance of private_key_jwt secret validator (used for testing)
    /// </summary>
    internal JwtRequestValidator(string audience, ILogger<JwtRequestValidator> logger)
    {
        _audienceUri = audience;
        Logger = logger;
    }

    /// <summary>
    /// Validates a JWT request object
    /// </summary>
    /// <param name="client">The client</param>
    /// <param name="jwtTokenString">The JWT</param>
    /// <returns></returns>
    public virtual async Task<JwtRequestValidationResult> ValidateAsync(Client client, string jwtTokenString)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(jwtTokenString)) throw new ArgumentNullException(nameof(jwtTokenString));

        var fail = new JwtRequestValidationResult { IsError = true };

        List<SecurityKey> trustedKeys;
        try
        {
            trustedKeys = await GetKeysAsync(client);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not parse client secrets");
            return fail;
        }

        if (trustedKeys.Count is 0)
        {
            Logger.LogError("There are no keys available to validate JWT");
            return fail;
        }

        JwtSecurityToken jwtSecurityToken;
        try
        {
            jwtSecurityToken = await ValidateJwtAsync(jwtTokenString, trustedKeys, client);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "JWT token validation error");
            return fail;
        }

        if (jwtSecurityToken.Payload.ContainsKey(OidcConstants.AuthorizeRequest.Request) ||
            jwtSecurityToken.Payload.ContainsKey(OidcConstants.AuthorizeRequest.RequestUri))
        {
            Logger.LogError("JWT payload must not contain request or request_uri");
            return fail;
        }

        var payload = await ProcessPayloadAsync(jwtSecurityToken);

        var result = new JwtRequestValidationResult
        {
            IsError = false,
            Payload = payload
        };

        Logger.LogDebug("JWT request object validation success");
        return result;
    }

    /// <summary>
    /// Retrieves keys for a given client
    /// </summary>
    /// <param name="client">The client</param>
    /// <returns></returns>
    protected virtual Task<List<SecurityKey>> GetKeysAsync(Client client)
    {
        return client.ClientSecrets.GetKeysAsync();
    }

    /// <summary>
    /// Validates the JWT token
    /// </summary>
    /// <param name="jwtTokenString">JWT as a string</param>
    /// <param name="keys">The keys</param>
    /// <param name="client">The client</param>
    /// <returns></returns>
    protected virtual Task<JwtSecurityToken> ValidateJwtAsync(string jwtTokenString, IEnumerable<SecurityKey> keys, Client client)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = keys,
            ValidateIssuerSigningKey = true,

            ValidIssuer = client.ClientId,
            ValidateIssuer = true,

            ValidAudience = AudienceUri,
            ValidateAudience = true,

            RequireSignedTokens = true,
            RequireExpirationTime = true
        };

        if (Options.StrictJarValidation)
        {
            tokenValidationParameters.ValidTypes = [JwtClaimTypes.JwtTypes.AuthorizationRequest];
        }

        Handler.ValidateToken(jwtTokenString, tokenValidationParameters, out var token);
            
        return Task.FromResult((JwtSecurityToken)token);
    }

    /// <summary>
    /// Processes the JWT contents
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns></returns>
    protected virtual Task<Dictionary<string, string>> ProcessPayloadAsync(JwtSecurityToken token)
    {
        // filter JWT validation values
        var payload = new Dictionary<string, string>();
        foreach (var key in token.Payload.Keys)
        {
            if (!Constants.Filters.JwtRequestClaimTypesFilter.Contains(key))
            {
                var value = token.Payload[key];

                //https://stackoverflow.com/questions/62398659/why-does-my-code-think-a-value-is-microsoft-identitymodel-json-linq-jobject-when
                switch (value) // сюда приходит internal Microsoft.IdentityModel.Json.Linq.JObject / JArray
                {
                    case string s:
                        payload.Add(key, s);
                        break;
                    case JObject jobj: // здесь ожидается NewtonSoft.Json.Linq.JObject
                        payload.Add(key, jobj.ToString(Formatting.None));
                        break;
                    case JArray jarr: // здесь ожидается NewtonSoft.Json.Linq.JArray
                        payload.Add(key, jarr.ToString(Formatting.None));
                        break;
                    case INotifyPropertyChanged: // public Microsoft.IdentityModel.Json.Linq.JObject parent
                        payload.Add(key, value.ToString());
                        break;
                    case IList: // public Microsoft.IdentityModel.Json.Linq.JArray parent
                        payload.Add(key, value.ToString());
                        break;
                }
            }
        }

        return Task.FromResult(payload);
    }
}