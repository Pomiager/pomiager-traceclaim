Contributing to TraceClaim
Thank you for your interest in TraceClaim.
TraceClaim is currently an early research proof of concept. Contributions are welcome, but the project intentionally prioritizes architectural clarity, standards interoperability and correctness over feature volume.
Before contributing
Please read:
`README.md`
`docs/ARCHITECTURE.md`
`docs/CLAIM-MODEL.md`
`docs/STANDARDS-MAPPING.md`
`docs/ROADMAP.md`
This is important because some functionality is deliberately not implemented yet.
Contribution principles
Preserve source semantics
Adapters must not silently reinterpret source metadata.
If a source construct cannot be represented correctly, prefer an explicit unsupported/diagnostic path over a guessed conversion.
Do not mix trust policy with normalization
An adapter answers:
> What did this source assert?
A trust policy answers:
> How does this consumer interpret the available assertions?
These concerns must remain separate.
Do not equate signatures with truth
Cryptographic verification can prove integrity and provenance. It does not prove semantic correctness.
Avoid unnecessary new standards
Before introducing a TraceClaim-specific format, evaluate whether an existing standard such as PURL, OSV, OpenVEX, SPDX, CycloneDX, in-toto, SLSA or Sigstore already solves the problem.
Keep Core infrastructure-independent
`TraceClaim.Core` must not depend directly on:
cloud SDKs;
blockchain SDKs;
OSV/OpenVEX models;
database frameworks;
web frameworks.
Development requirements
.NET 10 SDK
Git
Build:
```bash
dotnet restore
dotnet build TraceClaim.slnx
```
Run tests:
```bash
dotnet test TraceClaim.slnx
```
All changes should keep the test suite green.
Code style
Use modern C# and nullable reference types.
Prefer small, explicit domain types.
Public APIs should include XML documentation.
Non-obvious logic should include English inline comments explaining why, not merely restating the code.
Tests should describe behavior in their names.
Prefer deterministic dates in tests over `DateTimeOffset.UtcNow` when chronology is part of the behavior.
Pull requests
A useful pull request should include:
the problem being solved;
the standards or specifications involved;
why the change belongs in TraceClaim;
tests;
documentation updates when semantics change.
Adding an adapter
A new adapter should:
live outside `TraceClaim.Core`;
model only required source fields where practical;
document which parts of the upstream specification are supported;
preserve issuer and timestamps;
avoid trust decisions;
contain conversion tests;
include realistic synthetic sample documents;
document unsupported semantics.
Reporting standards issues
Standards corrections are especially welcome.
When reporting an issue, please include:
standard name and version;
relevant specification section;
example input;
expected TraceClaim behavior;
current behavior.
Security issues
Do not publish sensitive security vulnerabilities as ordinary public issues.
See `SECURITY.md`.