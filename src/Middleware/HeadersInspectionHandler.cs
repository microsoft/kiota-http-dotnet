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
/// The Headers Inspection Handler allows the developer to inspect the headers of the request and response.
/// </summary>
public class HeadersInspectionHandler : DelegatingHandler
{
    private readonly HeadersInspectionHandlerOption _defaultOptions;
    /// <summary>
    /// Create a new instance of <see cref="HeadersInspectionHandler"/>
    /// </summary>
    /// <param name="defaultOptions">Default options to apply to the handler</param>
    public HeadersInspectionHandler(HeadersInspectionHandlerOption? defaultOptions = null)
    {
        _defaultOptions = defaultOptions ?? new HeadersInspectionHandlerOption();
    }
    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if(request == null) throw new ArgumentNullException(nameof(request));

        var options = request.GetRequestOption<HeadersInspectionHandlerOption>() ?? _defaultOptions;

        Activity? activity;
        if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource?.StartActivity($"{nameof(RedirectHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("com.microsoft.kiota.handler.headersInspection.enable", true);
        }
        else
        {
            activity = null;
        }
        try
        {
            if(options.InspectRequestHeaders)
            {
                foreach(var header in request.Headers)
                {
                    options.RequestHeaders[header.Key] = header.Value.ToArray();
                }
                if(request.Content != null)
                    foreach(var contentHeaders in request.Content.Headers)
                    {
                        options.RequestHeaders[contentHeaders.Key] = contentHeaders.Value.ToArray();
                    }
            }
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if(options.InspectResponseHeaders)
            {
                foreach(var header in response.Headers)
                {
                    options.ResponseHeaders[header.Key] = header.Value.ToArray();
                }
                if(response.Content != null)
                    foreach(var contentHeaders in response.Content.Headers)
                    {
                        options.ResponseHeaders[contentHeaders.Key] = contentHeaders.Value.ToArray();
                    }
            }
            return response;
        }
        finally
        {
            activity?.Dispose();
        }

    }

}
