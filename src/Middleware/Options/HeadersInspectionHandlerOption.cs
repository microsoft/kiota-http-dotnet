// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
/// <summary>
/// The Headers Inspection Option allows the developer to inspect the headers of the request and response.
/// </summary>
public class HeadersInspectionHandlerOption: IRequestOption
{
    /// <summary>
    /// Gets or sets a value indicating whether the request headers should be inspected.
    /// </summary>
    public bool InspectRequestHeaders { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the response headers should be inspected.
    /// </summary>
    public bool InspectResponseHeaders { get; set; }
    /// <summary>
    /// Gets the request headers to for the current request.
    /// </summary>
    public IDictionary<string, string> RequestHeaders { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Gets the response headers for the current request.
    /// </summary>
    public IDictionary<string, string> ResponseHeaders { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
