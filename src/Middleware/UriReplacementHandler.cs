using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

/// <summary>
/// Interface for making URI replacements.
/// </summary>
public interface IUriReplacementHandlerOption : IRequestOption
{
    /// <summary>
    /// Check if URI replacement is enabled for the option.
    /// </summary>
    /// <returns>true if replacement is enabled or false otherwise.</returns>
    bool IsEnabled();

    /// <summary>
    /// Accepts a URI and returns a new URI with all replacements applied.
    /// </summary>
    /// <param name="original">The URI to apply replacements to</param>
    /// <returns>A new URI with all replacements applied.</returns>
    Uri? Replace(Uri? original);
}

/// <summary>
/// Url replacement options.
/// </summary>
public class UriReplacementHandlerOption : IUriReplacementHandlerOption
{
    private readonly bool isEnabled = false;

    private readonly IEnumerable<KeyValuePair<string, string>> replacementPairs = new Dictionary<string, string>();

    /// <summary>
    /// Creates a new instance of UriReplacementOption.
    /// </summary>
    /// <param name="isEnabled">Whether replacement is enabled.</param>
    /// <param name="replacementPairs">Replacements with the key being a string to match against and the value being the replacement.</param>
    public UriReplacementHandlerOption(bool isEnabled, IEnumerable<KeyValuePair<string, string>> replacementPairs)
    {
        this.isEnabled = isEnabled;
        this.replacementPairs = replacementPairs;

    }

    /// <inheritdoc/>
    public bool IsEnabled()
    {
        return isEnabled;
    }

    /// <inheritdoc/>
    public Uri? Replace(Uri? original)
    {
        if(original is null) return null;

        if(!isEnabled)
        {
            return original;
        }

        var newUrl = new UriBuilder(original);
        foreach(var pair in replacementPairs)
        {
            newUrl.Path = newUrl.Path.Replace(pair.Key, pair.Value);
        }

        return newUrl.Uri;
    }
}

/// <summary>
/// Replaces a portion of the URL.
/// </summary>
/// <typeparam name="TUriReplacementHandlerOption">A type with the rules used to perform a URI replacement.</typeparam>
public class UriReplacementHandler<TUriReplacementHandlerOption> : DelegatingHandler where TUriReplacementHandlerOption : IUriReplacementHandlerOption, IRequestOption
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
            var uriReplacementHandlerOption = request.GetRequestOption<TUriReplacementHandlerOption>() ?? uriReplacement;
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
