// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using Microsoft.AspNetCore.Authentication;

using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace IdentityServerHost.Quickstart.UI;

public class DiagnosticsViewModel
{
    public DiagnosticsViewModel(AuthenticateResult result)
    {
        AuthenticateResult = result;

        if (result.Properties.Items.TryGetValue("client_list", out var encoded))
        {
            var bytes = Base64Url.Decode(encoded);
            var value = Encoding.UTF8.GetString(bytes);

            Clients = JsonSerializer.Deserialize<string[]>(value);
        }
    }

    public AuthenticateResult AuthenticateResult { get; }
    public IEnumerable<string> Clients { get; } = new List<string>();
}