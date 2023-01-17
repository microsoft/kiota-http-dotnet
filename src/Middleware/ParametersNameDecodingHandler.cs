// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        var options = httpRequestMessage.GetRequestOption<ParametersNameDecodingOption>() ?? EncodingOptions;
        ActivitySource? activitySource;
        Activity? activity;
        if (httpRequestMessage.GetRequestOption<ObservabilityOptions>() is ObservabilityOptions obsOptions) {
            activitySource = new ActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource?.StartActivity($"{nameof(ParametersNameDecodingHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("com.microsoft.kiota.handler.parameters_name_decoding.enable", true);
        } else {
            activity = null;
            activitySource = null;
        }
        try {
            if(!httpRequestMessage.RequestUri!.Query.Contains('%') ||
                options == null ||
                !options.Enabled ||
                !(options.ParametersToDecode?.Any() ?? false))
            {
                return base.SendAsync(httpRequestMessage, cancellationToken);
            }

            var originalUri = httpRequestMessage.RequestUri;
            var query = DecodeUriEncodedString(originalUri.Query, EncodingOptions.ParametersToDecode);
            var decodedUri = new UriBuilder(originalUri.Scheme, originalUri.Host, originalUri.Port, originalUri.AbsolutePath, query).Uri;
            httpRequestMessage.RequestUri = decodedUri;
            return base.SendAsync(httpRequestMessage, cancellationToken);
        } finally {
            activity?.Dispose();
            activitySource?.Dispose();
        }
    }
    internal static string? DecodeUriEncodedString(string? original, IEnumerable<char> charactersToDecode) {
        if (string.IsNullOrEmpty(original) || !(charactersToDecode?.Any() ?? false))
            return original;
        var symbolsToReplace = charactersToDecode.Select(static x => ($"%{Convert.ToInt32(x):X}", x.ToString())).ToArray();
        foreach(var symbolToReplace in symbolsToReplace.Where(x => original!.Contains(x.Item1)))
        {
            original = original!.Replace(symbolToReplace.Item1, symbolToReplace.Item2);
        }
        return original;
    }
}
