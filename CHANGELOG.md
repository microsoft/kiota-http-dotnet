# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

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