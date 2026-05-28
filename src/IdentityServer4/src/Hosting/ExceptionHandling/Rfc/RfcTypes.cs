namespace IdentityServer4.Hosting.ExceptionHandling.Rfc;

/// <summary>
/// RFC 7807 problem type identifiers.
/// </summary>
internal static class RfcTypes
{
    // 4xx
    public const string BadRequest = "/problems/bad-request";
    public const string Unauthorized = "/problems/unauthorized";
    public const string Forbidden = "/problems/forbidden";
    public const string NotFound = "/problems/not-found";
    public const string MethodNotAllowed = "/problems/method-not-allowed";
    public const string NotAcceptable = "/problems/not-acceptable";
    public const string Conflict = "/problems/conflict";
    public const string Gone = "/problems/gone";
    public const string LengthRequired = "/problems/length-required";
    public const string PreconditionFailed = "/problems/precondition-failed";
    public const string PayloadTooLarge = "/problems/payload-too-large";
    public const string UriTooLong = "/problems/uri-too-long";
    public const string UnsupportedMediaType = "/problems/unsupported-media-type";
    public const string RangeNotSatisfiable = "/problems/range-not-satisfiable";
    public const string ExpectationFailed = "/problems/expectation-failed";
    public const string MisdirectedRequest = "/problems/misdirected-request";
    public const string ValidationError = "/problems/validation-error";
    public const string Locked = "/problems/locked";
    public const string FailedDependency = "/problems/failed-dependency";
    public const string TooEarly = "/problems/too-early";
    public const string UpgradeRequired = "/problems/upgrade-required";
    public const string PreconditionRequired = "/problems/precondition-required";
    public const string TooManyRequests = "/problems/too-many-requests";
    public const string RequestHeaderTooLarge = "/problems/request-header-fields-too-large";
    public const string UnavailableForLegalReasons = "/problems/unavailable-for-legal-reasons";

    // 5xx
    public const string InternalServerError = "/problems/internal-server-error";
    public const string NotImplemented = "/problems/not-implemented";
    public const string BadGateway = "/problems/bad-gateway";
    public const string ServiceUnavailable = "/problems/service-unavailable";
    public const string GatewayTimeout = "/problems/gateway-timeout";
    public const string HttpVersionNotSupported = "/problems/http-version-not-supported";
    public const string VariantAlsoNegotiates = "/problems/variant-also-negotiates";
    public const string InsufficientStorage = "/problems/insufficient-storage";
    public const string LoopDetected = "/problems/loop-detected";
    public const string NotExtended = "/problems/not-extended";
    public const string NetworkAuthenticationRequired = "/problems/network-authentication-required";

    // Domain / business
    public const string PaymentError = "/problems/payment-error";
    public const string Timeout = "/problems/timeout";
}