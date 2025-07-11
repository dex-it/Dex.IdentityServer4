// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Common;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4.Configuration;
using Xunit;

namespace IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ClientValidation_Code
{
    private IdentityServerOptions _options = TestIdentityServerOptions.Create();

    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - Code")]
    public async Task Code_Request_Unknown_Scope()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "unknown" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidScope);
    }

    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - Code")]
    public async Task OpenId_Code_Request_Invalid_RedirectUri()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://invalid"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }

    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - Code")]
    public async Task OpenId_Code_Request_Invalid_IdToken_ResponseType()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdToken},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
    }

    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - Code")]
    public async Task OpenId_Code_Request_Invalid_IdTokenToken_ResponseType()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
    }

    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - Code")]
    public async Task OpenId_Code_Request_With_Unknown_Client()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "unknown" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.UnauthorizedClient);
    }

    [Fact]
    [Trait("Category", "AuthorizeRequest Client Validation - Code")]
    public async Task OpenId_Code_Request_With_Restricted_Scope()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient_restricted" },
            { OidcConstants.AuthorizeRequest.Scope, "openid profile" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidScope);
    }
}