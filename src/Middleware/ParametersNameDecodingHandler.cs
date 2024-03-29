// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

/// <summary>
/// This handlers decodes special characters in the request query parameters that had to be encoded due to RFC 6570 restrictions names before executing the request.
/// </summary>
public class ParametersNameDecodingHandler: DelegatingHandler
{
    /// <summary>
    /// The options to use when decoding parameters names in URLs
    /// </summary>
    internal ParametersNameDecodingOption EncodingOptions { get; set; }
    /// <summary>
    /// Constructs a new <see cref="ParametersNameDecodingHandler"/>
    /// </summary>
    /// <param name="options">An OPTIONAL <see cref="ParametersNameDecodingOption"/> to configure <see cref="ParametersNameDecodingHandler"/></param>
    public ParametersNameDecodingHandler(ParametersNameDecodingOption? options = default)
    {
        EncodingOptions = options ?? new();
    }
    ///<inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = request.GetRequestOption<ParametersNameDecodingOption>() ?? EncodingOptions;
        Activity? activity;
        if (request.GetRequestOption<ObservabilityOptions>() is { } obsOptions) {
            var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource.StartActivity($"{nameof(ParametersNameDecodingHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("com.microsoft.kiota.handler.parameters_name_decoding.enable", true);
        } else {
            activity = null;
        }
        try {
            if(!request.RequestUri!.Query.Contains('%') ||
                !options.Enabled ||
                !(options.ParametersToDecode?.Any() ?? false))
            {
                return base.SendAsync(request, cancellationToken);
            }

            var originalUri = request.RequestUri;
            var query = DecodeUriEncodedString(originalUri.Query, options.ParametersToDecode.ToArray());
            var decodedUri = new UriBuilder(originalUri.Scheme, originalUri.Host, originalUri.Port, originalUri.AbsolutePath, query).Uri;
            request.RequestUri = decodedUri;
            return base.SendAsync(request, cancellationToken);
        } finally {
            activity?.Dispose();
        }
    }
    internal static string? DecodeUriEncodedString(string? original, char[] charactersToDecode) {
        if (string.IsNullOrEmpty(original) || !(charactersToDecode?.Any() ?? false))
            return original;
        var symbolsToReplace = charactersToDecode.Select(static x => ($"%{Convert.ToInt32(x):X}", x.ToString())).Where(x => original!.Contains(x.Item1)).ToArray();

        var encodedParameterValues = original!.TrimStart('?')
            .Split(new []{'&'}, StringSplitOptions.RemoveEmptyEntries)
            .Select(static part => part.Split(new []{'='}, StringSplitOptions.RemoveEmptyEntries)[0])
            .Where(static x => x.Contains('%'))// only pull out params with `%` (encoded)
            .ToArray();

        foreach(var parameter in encodedParameterValues)
        {
            var updatedParameterName = symbolsToReplace.Where(x => parameter!.Contains(x.Item1))
                .Aggregate(parameter, (current, symbolToReplace) => current!.Replace(symbolToReplace.Item1, symbolToReplace.Item2));
            original = original.Replace(parameter, updatedParameterName);
        }

        return original;
    }
}
