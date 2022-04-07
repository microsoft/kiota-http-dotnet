// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
    public ParametersNameDecodingHandler(ParametersNameDecodingOption options = default)
    {
        EncodingOptions = options ?? new();
    }
    ///<inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        if(!httpRequestMessage.RequestUri.Query.Contains('%') ||
            EncodingOptions == null ||
            !EncodingOptions.Enabled ||
            !(EncodingOptions.ParametersToDecode?.Any() ?? false))
        {
            return base.SendAsync(httpRequestMessage, cancellationToken);
        }

        var originalUri = httpRequestMessage.RequestUri;
        var query = originalUri.Query;
        var symbolsToReplace = EncodingOptions.ParametersToDecode.Select(x => ($"%{Convert.ToInt32(x):X}", x.ToString())).ToArray();
        foreach(var symbolToReplace in symbolsToReplace.Where(x => query.Contains(x.Item1)))
        {
            query = query.Replace(symbolToReplace.Item1, symbolToReplace.Item2);
        }
        var decodedUri = new UriBuilder(originalUri.Scheme, originalUri.Host, originalUri.Port, originalUri.AbsolutePath, query).Uri;
        httpRequestMessage.RequestUri = decodedUri;
        return base.SendAsync(httpRequestMessage, cancellationToken);
    }
}
