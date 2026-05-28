using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting.ExceptionHandling.Rfc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace IdentityServer4.Hosting.ExceptionHandling;

public sealed class IdentityExceptionHandleMiddleware(ILogger<IdentityExceptionHandleMiddleware> logger, IWebHostEnvironment environment) : IMiddleware
{
    private const int Status499ClientClosedRequest = 499;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cToken = context.RequestAborted;

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            // rfc problem
            var rfcProblem = exception.ToProblemDetails();

            // rfc code
            rfcProblem.Status ??= ResolveHttpStatusCode(exception);

            // rfc type
            rfcProblem.Type ??= ResolveRfcTypeByHttpStatusCode(rfcProblem.Status.Value);

            // rfc stackTrace
            if (string.IsNullOrEmpty(exception.StackTrace) is false && environment.IsProduction() is false)
                rfcProblem.Extensions[RfcExtensions.StackTrace] = string.Join(Environment.NewLine,
                    exception.GetInnerExceptions().Select(x => x.StackTrace));

            // rfc traceId
            rfcProblem.Extensions[RfcExtensions.TraceId] = Activity.Current?.Id ?? context.TraceIdentifier;

            // rfc Instance
            rfcProblem.Instance = GetRfcInstance(context.Request);

            var responseJson = JsonSerializer.Serialize(rfcProblem);
            if (rfcProblem.Status >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Request failed");
            else
                logger.LogWarning(exception, "Request complete with warning. {MiddlewareResponse}", responseJson);

            try
            {
                context.Response.StatusCode = rfcProblem.Status.Value;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(responseJson, cToken);
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (InvalidOperationException e) when (e.Message.Contains("response has already started", StringComparison.CurrentCultureIgnoreCase))
            {
                // ignore
            }
        }
    }

    private static int ResolveHttpStatusCode(Exception exception) => exception switch
    {
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        ArgumentException or ValidationException => StatusCodes.Status400BadRequest,
        OperationCanceledException => Status499ClientClosedRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string ResolveRfcTypeByHttpStatusCode(int statusCode) => statusCode switch
    {
        // 4xx Client Errors
        StatusCodes.Status400BadRequest => RfcTypes.BadRequest,
        StatusCodes.Status401Unauthorized => RfcTypes.Unauthorized,
        StatusCodes.Status402PaymentRequired => RfcTypes.PaymentError,
        StatusCodes.Status403Forbidden => RfcTypes.Forbidden,
        StatusCodes.Status404NotFound => RfcTypes.NotFound,
        StatusCodes.Status405MethodNotAllowed => RfcTypes.MethodNotAllowed,
        StatusCodes.Status406NotAcceptable => RfcTypes.NotAcceptable,
        StatusCodes.Status407ProxyAuthenticationRequired => RfcTypes.Unauthorized,
        StatusCodes.Status408RequestTimeout => RfcTypes.Timeout,
        StatusCodes.Status409Conflict => RfcTypes.Conflict,
        StatusCodes.Status410Gone => RfcTypes.Gone,
        StatusCodes.Status411LengthRequired => RfcTypes.LengthRequired,
        StatusCodes.Status412PreconditionFailed => RfcTypes.PreconditionFailed,
        StatusCodes.Status413PayloadTooLarge => RfcTypes.PayloadTooLarge,
        StatusCodes.Status414UriTooLong => RfcTypes.UriTooLong,
        StatusCodes.Status415UnsupportedMediaType => RfcTypes.UnsupportedMediaType,
        StatusCodes.Status416RangeNotSatisfiable => RfcTypes.RangeNotSatisfiable,
        StatusCodes.Status417ExpectationFailed => RfcTypes.ExpectationFailed,
        StatusCodes.Status418ImATeapot => "☕🤦‍♂️",
        StatusCodes.Status421MisdirectedRequest => RfcTypes.MisdirectedRequest,
        StatusCodes.Status422UnprocessableEntity => RfcTypes.ValidationError,
        StatusCodes.Status423Locked => RfcTypes.Locked,
        StatusCodes.Status424FailedDependency => RfcTypes.FailedDependency,
        425 => RfcTypes.TooEarly,
        StatusCodes.Status426UpgradeRequired => RfcTypes.UpgradeRequired,
        StatusCodes.Status428PreconditionRequired => RfcTypes.PreconditionRequired,
        StatusCodes.Status429TooManyRequests => RfcTypes.TooManyRequests,
        StatusCodes.Status431RequestHeaderFieldsTooLarge => RfcTypes.RequestHeaderTooLarge,
        StatusCodes.Status451UnavailableForLegalReasons => RfcTypes.UnavailableForLegalReasons,

        // 5xx Server Errors
        StatusCodes.Status500InternalServerError => RfcTypes.InternalServerError,
        StatusCodes.Status501NotImplemented => RfcTypes.NotImplemented,
        StatusCodes.Status502BadGateway => RfcTypes.BadGateway,
        StatusCodes.Status503ServiceUnavailable => RfcTypes.ServiceUnavailable,
        StatusCodes.Status504GatewayTimeout => RfcTypes.GatewayTimeout,
        StatusCodes.Status505HttpVersionNotsupported => RfcTypes.HttpVersionNotSupported,
        StatusCodes.Status506VariantAlsoNegotiates => RfcTypes.VariantAlsoNegotiates,
        StatusCodes.Status507InsufficientStorage => RfcTypes.InsufficientStorage,
        StatusCodes.Status508LoopDetected => RfcTypes.LoopDetected,
        StatusCodes.Status510NotExtended => RfcTypes.NotExtended,
        StatusCodes.Status511NetworkAuthenticationRequired => RfcTypes.NetworkAuthenticationRequired,

        _ => "unknown"
    };

    private static string GetRfcInstance(HttpRequest contextRequest)
    {
        var instance = new
        {
            Path = contextRequest.Path.ToString(),
            Query = contextRequest.QueryString.ToString()
        };

        return JsonSerializer.Serialize(instance);
    }
}

public static class IdentityExceptionHandleMiddlewareExtensions
{
    public static IIdentityServerBuilder AddIdentityExceptionHandling(this IIdentityServerBuilder identityServer)
    {
        identityServer.Services.AddSingleton<IdentityExceptionHandleMiddleware>();
        return identityServer;
    }

    public static IApplicationBuilder UseIdentityExceptionHandling(this IApplicationBuilder app) => app.UseMiddleware<IdentityExceptionHandleMiddleware>();
}