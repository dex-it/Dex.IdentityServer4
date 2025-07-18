// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling;

/// <summary>
/// The userinfo response generator
/// </summary>
/// <seealso cref="IdentityServer4.ResponseHandling.IUserInfoResponseGenerator" />
public class UserInfoResponseGenerator : IUserInfoResponseGenerator
{
    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The profile service
    /// </summary>
    protected readonly IProfileService Profile;

    /// <summary>
    /// The resource store
    /// </summary>
    protected readonly IResourceStore Resources;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoResponseGenerator"/> class.
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <param name="resourceStore">The resource store.</param>
    /// <param name="logger">The logger.</param>
    public UserInfoResponseGenerator(IProfileService profile, IResourceStore resourceStore, ILogger<UserInfoResponseGenerator> logger)
    {
        Profile = profile;
        Resources = resourceStore;
        Logger = logger;
    }

    /// <summary>
    /// Creates the response.
    /// </summary>
    /// <param name="validationResult">The userinfo request validation result.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">Profile service returned incorrect subject value</exception>
    public virtual async Task<Dictionary<string, object>> ProcessAsync(UserInfoRequestValidationResult validationResult)
    {
        Logger.LogDebug("Creating userinfo response");

        // extract scopes and turn into requested claim types
        var scopes = validationResult.TokenValidationResult.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(c => c.Value);

        var validatedResources = await GetRequestedResourcesAsync(scopes);
        var requestedClaimTypes = await GetRequestedClaimTypesAsync(validatedResources);

        Logger.LogDebug("Requested claim types: {claimTypes}", requestedClaimTypes.ToSpaceSeparatedString());

        // call profile service
        var context = new ProfileDataRequestContext(
            validationResult.Subject,
            validationResult.TokenValidationResult.Client,
            IdentityServerConstants.ProfileDataCallers.UserInfoEndpoint,
            requestedClaimTypes);
        context.RequestedResources = validatedResources;

        await Profile.GetProfileDataAsync(context);
        var profileClaims = context.IssuedClaims;

        // construct outgoing claims
        var outgoingClaims = new List<Claim>();

        if (profileClaims == null)
        {
            Logger.LogInformation("Profile service returned no claims (null)");
        }
        else
        {
            outgoingClaims.AddRange(profileClaims);
            Logger.LogInformation("Profile service returned the following claim types: {types}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
        }

        var subClaim = outgoingClaims.SingleOrDefault(x => x.Type == JwtClaimTypes.Subject);
        if (subClaim == null)
        {
            outgoingClaims.Add(new Claim(JwtClaimTypes.Subject, validationResult.Subject.GetSubjectId()));
        }
        else if (subClaim.Value != validationResult.Subject.GetSubjectId())
        {
            Logger.LogError("Profile service returned incorrect subject value: {sub}", subClaim);
            throw new InvalidOperationException("Profile service returned incorrect subject value");
        }

        return outgoingClaims.ToClaimsDictionary();
    }

    /// <summary>
    ///  Gets the identity resources from the scopes.
    /// </summary>
    /// <param name="scopes"></param>
    /// <returns></returns>
    protected internal virtual async Task<ResourceValidationResult> GetRequestedResourcesAsync(IEnumerable<string> scopes)
    {
        if (scopes == null || !scopes.Any())
        {
            return null;
        }

        var scopeString = string.Join(" ", scopes);
        Logger.LogDebug("Scopes in access token: {scopes}", scopeString);

        // if we ever parameterize identity scopes, then we would need to invoke the resource validator's parse API here
        var identityResources = await Resources.FindEnabledIdentityResourcesByScopeAsync(scopes);
            
        var resources = new Resources(identityResources, [], []);
        var result = new ResourceValidationResult(resources);
            
        return result;
    }

    /// <summary>
    /// Gets the requested claim types.
    /// </summary>
    /// <param name="resourceValidationResult"></param>
    /// <returns></returns>
    protected internal virtual Task<IEnumerable<string>> GetRequestedClaimTypesAsync(ResourceValidationResult resourceValidationResult)
    {
        IEnumerable<string> result = null;

        if (resourceValidationResult == null)
        {
            result = [];
        }
        else
        {
            var identityResources = resourceValidationResult.Resources.IdentityResources;
            result = identityResources.SelectMany(x => x.UserClaims).Distinct();
        }

        return Task.FromResult(result);
    }
}