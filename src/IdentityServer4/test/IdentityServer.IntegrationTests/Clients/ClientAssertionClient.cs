// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using IdentityServer.IntegrationTests.Clients.Setup;
using IdentityServer.IntegrationTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IdentityServer.IntegrationTests.Clients;

public class ClientAssertionClient
{
    private const string TokenEndpoint = "https://idsvr4/connect/token";
    private const string ClientId = "certificate_base64_valid";

    private readonly HttpClient _client;

    public ClientAssertionClient()
    {
        var builder = new WebHostBuilder()
            .UseStartup<Startup>();
        var server = new TestServer(builder);

        _client = server.CreateClient();
    }

    [Fact]
    public async Task Valid_client_with_manual_payload_should_succeed()
    {
        var token = CreateToken(ClientId);
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", ClientId },
            { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
            { "client_assertion", token },
            { "grant_type", "client_credentials" },
            { "scope", "api1" }
        });

        var response = await GetToken(requestBody);

        AssertValidToken(response);
    }

    [Fact]
    public async Task Valid_client_should_succeed()
    {
        var token = CreateToken(ClientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            // CredentialStyle.AuthorizationHeader and client assertions are not compatible
            // ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        AssertValidToken(response);
    }

    [Fact]
    public async Task Valid_client_with_implicit_clientId_should_succeed()
    {
        var token = CreateToken(ClientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            // CredentialStyle.AuthorizationHeader and client assertions are not compatible
            // ClientId = "client",

            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        AssertValidToken(response);
    }
        
    [Fact]
    public async Task Valid_client_with_token_replay_should_fail()
    {
        var token = CreateToken(ClientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            // CredentialStyle.AuthorizationHeader and client assertions are not compatible
            // ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        AssertValidToken(response);
            
        // replay
        response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            // CredentialStyle.AuthorizationHeader and client assertions are not compatible
            // ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = token
            },

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().BeTrue();
        response.Error.Should().Be("invalid_client");
    }

    [Fact]
    public async Task Client_with_invalid_secret_should_fail()
    {
        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,
            // CredentialStyle.AuthorizationHeader and client assertions are not compatible
            // ClientId = ClientId,
            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = "invalid"
            },

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
    }

    [Fact]
    public async Task Invalid_client_should_fail()
    {
        const string clientId = "certificate_base64_invalid";
        var token = CreateToken(clientId);

        var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = TokenEndpoint,

            ClientId = clientId,
            // CredentialStyle.AuthorizationHeader and client assertions are not compatible
            // ClientAssertion =
            // {
            //     Type = OidcConstants.ClientAssertionTypes.JwtBearer,
            //     Value = token
            // },

            Scope = "api1"
        }, TestContext.Current.CancellationToken);

        response.IsError.Should().Be(true);
        response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
        response.ErrorType.Should().Be(ResponseErrorType.Protocol);
    }

    private async Task<TokenResponse> GetToken(FormUrlEncodedContent body)
    {
        var response = await _client.PostAsync(TokenEndpoint, body);
        return await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(response);
    }

    private static void AssertValidToken(TokenResponse response)
    {
        response.IsError.Should().Be(false);
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
        response.IdentityToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();

        var payload = GetPayload(response);
            
        payload.Count.Should().Be(8);
        payload.Should().Contain("iss", "https://idsvr4");
        payload.Should().Contain("client_id", ClientId);
        payload.Keys.Should().Contain("iat");
            
        var scopes = payload["scope"] as JArray;
        scopes.First().ToString().Should().Be("api1");

        payload["aud"].Should().Be("api");
    }

    private static Dictionary<string, object> GetPayload(TokenResponse response)
    {
        var token = response.AccessToken.Split('.').Skip(1).Take(1).First();
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(
            Encoding.UTF8.GetString(Base64Url.Decode(token)));

        return dictionary;
    }

    private static string CreateToken(string clientId, DateTime? nowOverride = null)
    {
        var certificate = TestCert.Load();
        var now = nowOverride ?? DateTime.UtcNow;

        var token = new JwtSecurityToken(
            clientId,
            TokenEndpoint,
            new List<Claim>
            {
                new("jti", Guid.NewGuid().ToString()),
                new(JwtClaimTypes.Subject, clientId),
                new(JwtClaimTypes.IssuedAt, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            },
            now,
            now.AddMinutes(1),
            new SigningCredentials(
                new X509SecurityKey(certificate),
                SecurityAlgorithms.RsaSha256
            )
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}