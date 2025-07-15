// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4;
using Xunit;

namespace IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ProtocolValidation_Valid
{
    private const string Category = "AuthorizeRequest Protocol Validation - Valid";

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_OpenId_Code_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().Be(false);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Resource_Code_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "resource" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Mixed_Code_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid resource" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Resource_Token_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "resource" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_OpenId_IdToken_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdToken},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Mixed_IdTokenToken_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid resource" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_OpenId_IdToken_With_FormPost_ResponseMode_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, IdentityServerConstants.StandardScopes.OpenId },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdToken},
            {OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.FormPost},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_OpenId_IdToken_Token_With_FormPost_ResponseMode_Request()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid resource" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken},
            {OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.FormPost},
            {OidcConstants.AuthorizeRequest.Nonce, "abc"}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResponseMode_For_Code_ResponseType()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code},
            {OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Fragment}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task anonymous_user_should_produce_session_state_value()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code},
            {OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Fragment},
            {OidcConstants.AuthorizeRequest.Prompt, OidcConstants.PromptModes.None}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.ValidatedRequest.SessionId.Should().NotBeNull();
    }
        
    [Fact]
    [Trait("Category", Category)]
    public async Task multiple_prompt_values_should_be_accepted()
    {
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            {OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb"},
            {OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code},
            {OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Fragment},
            {OidcConstants.AuthorizeRequest.Prompt, OidcConstants.PromptModes.Consent.ToString() + " " + OidcConstants.PromptModes.Login.ToString()}
        };

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.ValidatedRequest.PromptModes.Count().Should().Be(2);
        result.ValidatedRequest.PromptModes.Should().Contain(OidcConstants.PromptModes.Login);
        result.ValidatedRequest.PromptModes.Should().Contain(OidcConstants.PromptModes.Consent);
    }
}