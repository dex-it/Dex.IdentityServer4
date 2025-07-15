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

public class Authorize_ClientValidation_Token
{
    private const string Category = "AuthorizeRequest Client Validation - Token";

    private IdentityServerOptions _options = TestIdentityServerOptions.Create();

    [Fact]
    [Trait("Category", Category)]
    public async Task Mixed_Token_Request_Without_OpenId_Scope()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "resource profile" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);
            
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidScope);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task IdTokenToken_Request_with_no_AAVB()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient_no_aavb" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }


    [Fact]
    [Trait("Category", Category)]
    public async Task CodeIdTokenToken_Request_with_no_AAVB()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "hybridclient_no_aavb" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.Nonce, "nonce"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.CodeIdTokenToken}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }
}