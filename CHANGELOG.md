# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.5] - 2023-06-29

- Fixes regression in request building when the passed httpClient base address ends with a `\`

## [1.0.4] - 2023-06-15

- Fixes a bug where NullReference Exception is thrown if a requestInformation is sent without providing UriTemplate
- RequestAdapter passes `HttpCompletionOption.ResponseHeadersRead` to HttpClient for Stream responses to avoid memory consumption for large payloads.

## [1.0.3] - 2023-06-09

- Added propagating the HttpClientRequestAdapter's supplied HttpClient BaseAddress as the adapter's initial BaseUrl

### Added

## [1.0.2] - 2023-04-06

### Changed

- Includes Response headers in APIException for failed requests.

## [1.0.1] - 2023-03-10

### Changed

- Update minimum version of [`System.Diagnostics.DiagnosticSource`](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource) to `6.0.0`.
- Update minimum version of [`System.Text.Json`](https://www.nuget.org/packages/System.Text.Json) to `6.0.0`.

## [1.0.0] - 2023-02-27

### Added

- GA release

## [1.0.0-rc.6] - 2023-02-03

### Added

- Added the HTTP response status code on API exception.

## [1.0.0-rc.5] - 2023-01-23

### Changed

- Aligns the HttpClientRequestAdapter with other langugages to use the BaseUrl from the RequestAdapter as the baseUrl for making requests.

## [1.0.0-rc.4] - 2023-01-09

### Added

- Adds support for nullalbe reference types.

## [1.0.0-rc.3] - 2023-01-09

### Added

- Added a method to convert abstract requests to native requests in the request adapter interface.

## [1.0.0-rc.2] - 2023-01-05

### Added

- Adds this library version as a product in the user-agent

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

### Changed

## [1.0.0-preview.13] - 2022-12-14

### Changed

- Added multi-value headers support.

## [1.0.0-preview.12] - 2022-12-01

### Changed

- Fixes RetryHandler to return the real wait time

## [1.0.0-preview.11] - 2022-10-17

### Changed

- Changes the ResponseHandler parameter in IRequestAdapter to be a RequestOption

## [1.0.0-preview.10] - 2022-09-19

### Added

- Added tracing support through OpenTelemetry.

## [1.0.0-preview.9] - 2022-09-07

### Added

- Added support for additional status codes.

## [1.0.0-preview.8] - 2022-05-19

### Changed

- Fixed a bug where CAE support would keep connections open when retrying.

## [1.0.0-preview.7] - 2022-05-13

### Added

- Added support for continuous access evaluation.

## [1.0.0-preview.6] - 2022-04-12

### Changed

- Breaking: Changes target runtime to netstandard2.0

## [1.0.0-preview.5] - 2022-04-07

### Added

- Added supports for decoding parameter names.

## [1.0.0-preview.4] - 2022-04-06

### Changed

- Fix issue with `HttpRequestAdapter` returning disposed streams when the requested return type is a Stream [#10](https://github.com/microsoft/kiota-http-dotnet/issues/10)

## [1.0.0-preview.3] - 2022-03-28

### Added

- Added support for 204 no content responses

### Changed

- Fixed a bug where BaseUrl would not be set in some scenarios

## [1.0.0-preview.2] - 2022-03-18

### Changed

- Fixed a bug where scalar request would not deserialize correctly.

## [1.0.0-preview.1] - 2022-03-18

### Added

- Initial Nuget release