// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer.UnitTests.Common;

public class MockUserSession : IUserSession
{
    public string[] Clients = [];

    public bool EnsureSessionIdCookieWasCalled { get; set; }
    public bool RemoveSessionIdCookieWasCalled { get; set; }
    public bool CreateSessionIdWasCalled { get; set; }

    public ClaimsPrincipal User { get; set; }
    public string SessionId { get; set; }
    public AuthenticationProperties Properties { get; set; }


    public Task<string> CreateSessionIdAsync(ClaimsPrincipal principal, AuthenticationProperties properties)
    {
        CreateSessionIdWasCalled = true;
        User = principal;
        SessionId = Guid.NewGuid().ToString();
        return Task.FromResult(SessionId);
    }

    public Task<ClaimsPrincipal> GetUserAsync()
    {
        return Task.FromResult(User);
    }

    Task<string> IUserSession.GetSessionIdAsync()
    {
        return Task.FromResult(SessionId);
    }

    public Task EnsureSessionIdCookieAsync()
    {
        EnsureSessionIdCookieWasCalled = true;
        return Task.CompletedTask;
    }

    public Task RemoveSessionIdCookieAsync()
    {
        RemoveSessionIdCookieWasCalled = true;
        return Task.CompletedTask;
    }

    public Task<string[]> GetClientListAsync()
    {
        return Task.FromResult(Clients);
    }

    public Task AddClientIdAsync(string clientId)
    {
        Clients = Clients.Append(clientId).ToArray();
        return Task.CompletedTask;
    }
}