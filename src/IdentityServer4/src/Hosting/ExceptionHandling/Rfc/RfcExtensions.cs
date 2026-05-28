namespace IdentityServer4.Hosting.ExceptionHandling.Rfc;

#pragma warning disable 1591

internal static class RfcExtensions
{
    public const int DefaultExtensionsCapacity = 4;

    // default
    public const string ExceptionType = "exceptionType";
    public const string ExceptionData = "exceptionData";
    public const string StackTrace = "stackTrace";
    public const string TraceId = "traceId";
}