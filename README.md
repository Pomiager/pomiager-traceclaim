TraceClaim
Verifiable claim lineage and trust resolution for federated software supply-chain metadata.
> **Project status:** early research proof of concept (PoC).  
> TraceClaim is **not production-ready** and intentionally implements only a narrow vertical slice of the proposed architecture.
TraceClaim explores how software metadata coming from independent and potentially conflicting authorities can be normalized, preserved, related over time, and evaluated through explicit consumer-controlled trust policies without collapsing multiple assertions into a single global "truth".
The current PoC focuses on a concrete software supply-chain scenario: an OSV advisory reports a package version as affected by a vulnerability while a vendor-issued OpenVEX statement reports the same package version as not affected. TraceClaim normalizes both statements into a common claim model, preserves their provenance and timestamps, detects the conflict, stores the relationship in a claim graph, and allows different trust policies to derive different resolutions from the same evidence.
Why TraceClaim?
Modern software supply-chain metadata is distributed across many independent sources and standards. A package can simultaneously be described by:
package registries and Package URLs (PURL);
SBOM documents such as SPDX and CycloneDX;
vulnerability databases such as OSV;
VEX documents describing exploitability or impact status;
build provenance such as SLSA and in-toto attestations;
vendor advisories and security bulletins;
transparency logs and signed metadata.
These sources may be incomplete, delayed, corrected, superseded, or contradictory.
TraceClaim starts from a simple premise:
> **A cryptographically valid assertion is not necessarily semantically true.**
A system can prove who issued a statement, when it was issued, and whether it was modified. It cannot automatically prove that the statement itself is correct. TraceClaim therefore separates three concerns:
Evidence and claims — what each source asserted.
Lineage and relationships — how claims support, conflict with, or supersede one another.
Resolution — how a particular consumer chooses to interpret the available evidence.
This separation is the core architectural idea of the project.
---
Current PoC capabilities
The current repository demonstrates the following capabilities:
Normalized software claims
A `SoftwareClaim` represents a normalized assertion about a software subject:
```text
Subject   : pkg:nuget/Acme.Security@2.1.0
Predicate : affected-by
Object    : CVE-2026-1234
Issuer    : https://osv.dev
IssuedAt  : 2026-09-01T10:00:00Z
ObservedAt: 2026-09-02T08:30:00Z
```
The model deliberately distinguishes:
`IssuedAt`: when the source states that the assertion was issued;
`ObservedAt`: when TraceClaim ingested or observed the assertion.
This makes it possible to reconstruct both the issuer-side timeline and TraceClaim's own ingestion history.
Claim relationships
The PoC currently models three semantic relationships:
```text
Supports
ConflictsWith
Supersedes
```
For example:
```text
Vendor claim v2 --Supersedes--> Vendor claim v1

OSV claim --------ConflictsWith-------- Vendor claim
```
A superseded claim remains part of the historical record. Supersession does not delete evidence.
Temporal lineage
`ClaimLineageBuilder` reconstructs the sequence of assertions issued by the same authority about the same software subject and object.
The lineage uses `IssuedAt`, not ingestion order:
```text
Issuer timeline

T1  affected-by
       |
       | superseded by
       v
T2  not-affected-by
```
Even if TraceClaim observes `T2` before `T1`, the issuer-side lineage remains correct.
Conflict detection
`ClaimConflictDetector` currently recognizes a deliberately small first rule:
```text
affected-by
     vs.
not-affected-by
```
for the same software subject and vulnerability.
This is intentionally narrow. The PoC demonstrates feasibility without pretending to solve the full semantic interoperability problem.
Consumer-controlled trust policies
The PoC includes two simple policies:
ConservativeSecurityPolicy
If at least one current source reports the package as affected, the resolution is:
```text
affected
```
VendorPreferredPolicy
If the configured vendor has issued a claim, the most recent vendor-side assertion takes precedence.
This can produce:
```text
not-affected
```
from the same evidence used by the conservative policy.
This is intentional:
> **The evidence is stable; the decision is policy-dependent.**
Claim graph
`ClaimGraph` provides an in-memory representation of claims and relationships with basic integrity checks and queries, including:
add and retrieve claims;
add relationships only between existing claims;
avoid duplicate semantic edges;
query claims by subject;
retrieve conflicts;
retrieve current claims by excluding superseded assertions.
The graph is deliberately in-memory in the PoC. Scalable persistence and query infrastructure are planned work.
Initial OSV adapter
`TraceClaim.Adapters.Osv` normalizes a limited subset of OSV metadata into `SoftwareClaim` instances.
The current implementation supports:
OSV advisory identifier;
publication timestamp;
package ecosystem and name;
optional PURL;
explicitly listed affected versions.
Each explicit affected version becomes a version-specific claim:
```text
pkg:nuget/Acme.Security@2.1.0
    affected-by
CVE-2026-1234
```
The adapter intentionally does not yet implement OSV version ranges.
Initial OpenVEX adapter
`TraceClaim.Adapters.OpenVex` normalizes a limited subset of OpenVEX statements.
The current mapping includes:
```text
OpenVEX status          TraceClaim predicate
------------------------------------------------
affected                affected-by
not_affected            not-affected-by
fixed                   fixed-for
under_investigation     under-investigation-for
```
The adapter preserves the document author as the current logical issuer and supports statement-level timestamps overriding the enclosing document timestamp.
Automated tests
The PoC includes unit and integration-oriented tests covering the domain model, temporal lineage, conflicts, trust policies, graph integrity, OSV conversion, OpenVEX conversion, and cross-standard conflict detection.
The exact test count may evolve as the PoC changes; the repository history and CI status are the authoritative source.
---
Example scenario
The central PoC scenario is intentionally simple.
OSV says
```text
Subject   : pkg:nuget/Acme.Security@2.1.0
Predicate : affected-by
Object    : CVE-2026-1234
Issuer    : https://osv.dev
```
Vendor OpenVEX says
```text
Subject   : pkg:nuget/Acme.Security@2.1.0
Predicate : not-affected-by
Object    : CVE-2026-1234
Issuer    : https://vendor.example
```
TraceClaim preserves both assertions:
```text
              CVE-2026-1234
                    |
          +---------+---------+
          |                   |
         OSV                Vendor
          |                   |
      affected            not affected
          |                   |
          +---------+---------+
                    |
                 conflict
```
A conservative policy may resolve the evidence to `affected`, while a vendor-preferred policy may resolve it to `not-affected`.
Neither policy deletes, rewrites, or invalidates the original claims.
---
Repository structure
The PoC is organized as a small .NET solution:
```text
pomiager-traceclaim/
|
|-- TraceClaim.slnx
|-- README.md
|-- LICENSE
|-- CONTRIBUTING.md
|-- SECURITY.md
|
|-- docs/
|   |-- ARCHITECTURE.md
|   |-- MOTIVATION.md
|   |-- CLAIM-MODEL.md
|   |-- TRUST-POLICIES.md
|   |-- STANDARDS-MAPPING.md
|   |-- PRIOR-ART.md
|   `-- ROADMAP.md
|
|-- src/
|   |-- TraceClaim.Core/
|   |-- TraceClaim.Adapters.Osv/
|   `-- TraceClaim.Adapters.OpenVex/
|
`-- tests/
    |-- TraceClaim.Core.Tests/
    |-- TraceClaim.Adapters.Osv.Tests/
    `-- TraceClaim.Adapters.OpenVex.Tests/
```
---
Build and test
Requirements
.NET 10 SDK
Git
Build
```bash
dotnet restore
dotnet build TraceClaim.slnx
```
Run tests
```bash
dotnet test TraceClaim.slnx
```
No proprietary cloud service is required to build or test the current PoC.
---
Architecture principles
TraceClaim follows several principles that are expected to remain stable even as the implementation evolves.
1. Preserve source assertions
Adapters normalize metadata but should not silently change its meaning.
2. Do not equate signatures with truth
Cryptographic verification can establish integrity and provenance, not semantic correctness.
3. Keep claims separate from resolution
A claim records an assertion. A policy derives a decision.
4. Preserve history
Superseded information remains auditable.
5. Prefer existing standards
TraceClaim is intended to interoperate with existing software supply-chain standards rather than create unnecessary replacements.
6. Keep anchoring pluggable
Future tamper-evident storage or anchoring should not make the core domain dependent on one blockchain, transparency log, or proprietary platform.
7. Make unsupported semantics explicit
When TraceClaim cannot safely interpret a source construct, failing explicitly is preferable to producing silently incorrect normalized metadata.
---
What is deliberately NOT implemented yet
The current PoC is intentionally incomplete. In particular, it does not yet provide:
standards-complete OSV parsing;
OSV ecosystem-specific range evaluation;
complete OpenVEX validation or inheritance semantics;
SPDX or CycloneDX ingestion;
CSAF ingestion;
SLSA provenance ingestion;
in-toto attestation output;
canonical claim serialization;
deterministic claim hashing;
digital signatures;
issuer identity verification;
Sigstore integration;
Rekor integration;
Merkle batching;
blockchain/DLT anchoring;
scalable persistence;
graph database integration;
REST API;
CLI;
authentication/authorization;
policy configuration language;
trust weighting;
distributed federation protocol;
production security hardening;
security audit;
production deployment packaging.
These are not accidental omissions. They define a substantial part of the research and engineering roadmap beyond the initial PoC.
---
Planned research and development
The broader TraceClaim direction is described in ROADMAP.md. Major areas include:
standards-complete normalization and validation;
canonical claim representation;
cryptographic attestations and issuer verification;
scalable claim lineage and conflict queries;
configurable trust-policy evaluation;
transparency-log integration;
experimental distributed-ledger anchoring;
benchmarking transparency-log versus DLT approaches;
public API and CLI tooling;
interoperability and conformance test suites.
The project explicitly does not assume that blockchain is always the correct trust mechanism. One research objective is to compare transparency-log and distributed-ledger approaches using measurable criteria such as throughput, latency, verification cost, governance, operational complexity, and failure assumptions.
---
Standards and ecosystems
TraceClaim currently studies or plans interoperability with:
Package URL (PURL)
OSV
OpenVEX
CSAF
SPDX
CycloneDX
in-toto Attestation Framework
SLSA provenance
Sigstore
Rekor
See STANDARDS-MAPPING.md for details.
---
Prior art
TraceClaim is not intended to replace existing software supply-chain infrastructure. Relevant projects and research include GUAC, Grafeas, in-toto, SLSA, Sigstore/Rekor, blockchain-based SBOM/provenance projects, W3C PROV, nanopublications, and provenance-oriented research on multi-source assertions.
The intended research gap is not merely "conflicting claims" or "blockchain for SBOMs". It is the integration of software-specific heterogeneous claims into a verifiable temporal lineage that preserves issuer provenance, contradictions, supersession, and consumer-controlled resolution in a federated metadata environment.
See PRIOR-ART.md.
---
Security
TraceClaim is a research PoC and should not currently be used as a production security decision engine.
Please read SECURITY.md before reporting vulnerabilities or evaluating the PoC for operational use.
---
Contributing
Contributions, technical discussion, interoperability feedback, and standards corrections are welcome.
See CONTRIBUTING.md.
---
License
TraceClaim is released under the Apache License 2.0.
See LICENSE.
---
Project origin
TraceClaim is an independent open-source research project initiated by Pomiager to explore verifiable lineage and trust resolution for federated software supply-chain metadata.
The repository is intentionally maintained as a clean, standalone project. It does not incorporate proprietary source code, customer data, or confidential implementation material from other Pomiager projects.