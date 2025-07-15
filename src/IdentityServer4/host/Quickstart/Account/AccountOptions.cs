// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServerHost.Quickstart.UI;

public class AccountOptions
{
    public static readonly bool AllowLocalLogin = true;
    public static readonly bool AllowRememberLogin = true;
    public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

    public static readonly bool ShowLogoutPrompt = true;
    public static readonly bool AutomaticRedirectAfterSignOut = false;

    public static readonly string InvalidCredentialsErrorMessage = "Invalid username or password";
}