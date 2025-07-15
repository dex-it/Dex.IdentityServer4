using Microsoft.AspNetCore.Authentication;
using System;

namespace IdentityServer.UnitTests.Common;

internal class MockSystemClock : ISystemClock
{
    public DateTimeOffset Now { get; set; }

    public DateTimeOffset UtcNow
    {
        get
        {
            return Now;
        }
    }
}