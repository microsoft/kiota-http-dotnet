using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

/// <summary>
/// Replaces a portion of the URL.
/// </summary>
/// <typeparam name="TUriReplacementHandlerOption">A type with the rules used to perform a URI replacement.</typeparam>
public class UriReplacementHandler<TUriReplacementHandlerOption> : DelegatingHandler where TUriReplacementHandlerOption : IUriReplacementHandlerOption
{
    private readonly TUriReplacementHandlerOption uriReplacement;

    /// <summary>
    /// Creates a new UriReplacementHandler.
    /// </summary>
    /// <param name="uriReplacement">An object with the URI replacement rules.</param>
    public UriReplacementHandler(TUriReplacementHandlerOption uriReplacement)
    {
        this.uriReplacement = uriReplacement;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        ActivitySource? activitySource;
        Activity? activity;
        if(request.GetRequestOption<ObservabilityOptions>() is ObservabilityOptions obsOptions)
        {
            activitySource = new ActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource.StartActivity($"{nameof(UriReplacementHandler<TUriReplacementHandlerOption>)}_{nameof(SendAsync)}");
            activity?.SetTag("com.microsoft.kiota.handler.uri_replacement.enable", uriReplacement.IsEnabled());
        }
        else
        {
            activity = null;
            activitySource = null;
        }

        try
        {
            var uriReplacementHandlerOption = request.GetRequestOption<IUriReplacementHandlerOption>() ?? uriReplacement;
            if (uriReplacement.IsEnabled()) {
                request.RequestUri = uriReplacementHandlerOption.Replace(request.RequestUri);
            }
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            activity?.Dispose();
            activitySource?.Dispose();
        }
    }
}
