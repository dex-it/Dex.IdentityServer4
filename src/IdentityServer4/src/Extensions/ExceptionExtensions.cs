using System;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace IdentityServer4.Extensions;

internal static class ExceptionExtensions
{
    public static IEnumerable<Exception> GetInnerExceptions(this Exception exception, int count = 5)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var innerException = exception;
        do
        {
            yield return innerException;

            innerException = innerException.InnerException;
            count--;
        } while (innerException is not null && count > 0);
    }
}