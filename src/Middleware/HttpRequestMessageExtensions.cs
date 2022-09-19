// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net.Http;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

/// <summary>
/// Extension methods for <see cref="HttpRequestMessage"/>
/// </summary>
public static class HttpRequestMessageExtensions {
    /// <summary>
    /// Gets the <see cref="ObservabilityOptions"/> for the current request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to get the <see cref="ObservabilityOptions"/> from.</param>
    /// <returns>The <see cref="ObservabilityOptions"/> for the current request or null.</returns>
    public static ObservabilityOptions GetObservabilityOptionsForRequest(this HttpRequestMessage request) {
        return request?.Properties.TryGetValue(typeof(ObservabilityOptions).FullName, out var value) ?? false ?
            value as ObservabilityOptions :
            null;
    }
}
