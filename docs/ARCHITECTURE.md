# Architecture

## 1. Scope of this document

This document describes both:

1. the architecture implemented by the current TraceClaim PoC;
2. the target architectural direction that motivates future work.

Sections explicitly marked **Planned** are not part of the current implementation.

## 2. Architectural overview

The current PoC separates source-specific metadata from the TraceClaim domain model.

```text
       +------------------+        +------------------+
       |     OSV JSON     |        |   OpenVEX JSON   |
       +---------+--------+        +---------+--------+
                 |                           |
                 v                           v
       +------------------+        +------------------+
       | OsvClaimAdapter  |        |OpenVexClaimAdapter|
       +---------+--------+        +---------+--------+
                 |                           |
                 +-------------+-------------+
                               |
                               v
                    +---------------------+
                    |    SoftwareClaim    |
                    +----------+----------+
                               |
                +--------------+--------------+
                |                             |
                v                             v
       +------------------+        +--------------------+
       | ConflictDetector |        | ClaimLineageBuilder|
       +---------+--------+        +---------+----------+
                 |                           |
                 +-------------+-------------+
                               |
                               v
                       +---------------+
                       |  ClaimGraph   |
                       +-------+-------+
                               |
                      +--------+--------+
                      |                 |
                      v                 v
           +------------------+ +--------------------+
           |ConservativePolicy| |VendorPreferredPolicy|
           +------------------+ +--------------------+
```

## 3. Projects

### TraceClaim.Core

Contains the source-independent domain model and domain services.

Responsibilities currently include:

- `SoftwareClaim`;
- claim relationships;
- temporal lineage;
- conflict detection;
- in-memory graph integrity;
- basic graph queries;
- trust-policy abstractions;
- simple demonstration policies.

`TraceClaim.Core` must not depend on OSV, OpenVEX, blockchain SDKs, cloud services, or persistence frameworks.

This dependency direction is intentional.

### TraceClaim.Adapters.Osv

Contains the source-specific representation and conversion logic for the supported OSV subset.

The adapter converts source semantics into normalized claims. It must not decide whether OSV is trustworthy.

### TraceClaim.Adapters.OpenVex

Contains the source-specific representation and conversion logic for the supported OpenVEX subset.

It preserves the document author as the current logical issuer and maps VEX impact status into TraceClaim predicates.

## 4. Dependency rule

The desired dependency direction is:

```text
Adapters -------> Core
Tests ----------> Adapters/Core
Core ------------> nothing source-specific
```

The following direction is prohibited:

```text
Core ---> OSV
Core ---> OpenVEX
Core ---> Polygon
Core ---> Azure
```

This allows TraceClaim to remain infrastructure-independent.

## 5. Core domain objects

### SoftwareClaim

Represents one assertion by one issuer about one software subject.

Conceptually:

```text
Claim =
    Subject
  + Predicate
  + Object
  + Issuer
  + IssuedAt
  + ObservedAt
  + Evidence reference
```

Examples:

```text
pkg:nuget/foo@1.2.3 affected-by CVE-2026-1234
pkg:nuget/foo@1.2.3 not-affected-by CVE-2026-1234
```

### ClaimRelation

Represents a semantic edge between claims.

Current types:

```text
Supports
ConflictsWith
Supersedes
```

A future model may need additional relationship metadata such as confidence, derivation evidence, algorithm version or source of inference.

## 6. Conflict detection

The PoC conflict detector intentionally implements a narrow rule:

```text
same Subject
+ same Object
+ opposite predicates affected-by / not-affected-by
= conflict
```

This is a proof of feasibility, not a comprehensive semantic conflict engine.

A production-quality implementation will need to account for:

- package aliases;
- version ranges;
- ecosystem-specific version semantics;
- vulnerability aliases;
- product/component relationships;
- statement scope;
- VEX status semantics;
- time validity;
- source-specific qualifiers.

## 7. Lineage

The lineage builder groups claims using:

- subject;
- object;
- issuer.

Predicate is deliberately excluded from the grouping key.

This allows an issuer to revise its statement:

```text
affected-by
    |
    | superseded by
    v
not-affected-by
```

The relation direction is:

```text
NewClaim --Supersedes--> OldClaim
```

The old claim is retained.

## 8. ClaimGraph

The current graph is an in-memory implementation.

Its purpose is to demonstrate graph semantics and integrity, not scale.

Current invariants include:

- claim IDs are unique;
- relation endpoints must exist;
- duplicate semantic edges are ignored;
- superseded claims can be excluded from current-state queries.

Current state does not mean truth.

A current claim is merely a claim that has not been superseded in the graph.

## 9. Trust policies

Trust policies consume claims and produce resolutions.

They must not mutate the graph.

Conceptually:

```text
Claims + Policy -> Resolution
```

rather than:

```text
Claims -> Truth
```

This permits reproducible organization-specific decisions.

## 10. Planned architecture

The intended future architecture introduces several new layers.

```text
                     External metadata sources
          +----------+----------+----------+----------+
          |          |          |          |          |
         OSV        VEX       SPDX/CDX    SLSA      CSAF
          |          |          |          |          |
          +----------+----------+----------+----------+
                                |
                                v
                      Source adapters/validators
                                |
                                v
                       Identity normalization
                         PURL / aliases / CVE
                                |
                                v
                         Canonical claims
                                |
                     +----------+----------+
                     |                     |
                     v                     v
              Semantic lineage       Evidence metadata
                     |                     |
                     +----------+----------+
                                |
                                v
                        Attestation layer
                                |
                     canonicalization + hash
                                |
                     digital signature/identity
                                |
               +----------------+----------------+
               |                                 |
               v                                 v
       Transparency-log anchor           Experimental DLT anchor
               |                                 |
               +----------------+----------------+
                                |
                                v
                      Verification service
                                |
                                v
                        Policy evaluation
                                |
                                v
                            Resolution
```

## 11. Planned canonicalization layer

Cryptographic hashing requires deterministic serialization.

Naively hashing ordinary JSON serialization is unsafe because logically equivalent data can serialize differently due to:

- property ordering;
- whitespace;
- optional values;
- encoding choices;
- normalization differences.

A future canonical representation must define exactly which fields are signed and hashed and how they are serialized.

The current `SoftwareClaim.Hash` property is therefore intentionally not populated by the PoC.

## 12. Planned attestation layer

The project intends to investigate reuse of the in-toto Attestation Framework rather than inventing an unnecessary custom envelope.

A possible mapping is:

```text
in-toto Statement
  subject       -> software subject identity
  predicateType -> TraceClaim claim predicate type
  predicate     -> issuer/evidence/lineage metadata
```

This is a design direction, not yet implemented.

## 13. Planned transparency integration

Sigstore Rekor is a natural baseline for tamper-evident logging because it already provides append-only transparency and verification mechanisms for signed software supply-chain metadata.

TraceClaim should therefore avoid implementing its own transparency log unless a concrete requirement cannot be satisfied by existing infrastructure.

## 14. Planned DLT experimentation

Distributed-ledger anchoring should remain pluggable.

A future abstraction may resemble:

```text
IAttestationAnchor
    |
    +-- RekorAnchor
    +-- DltAnchor
    +-- LocalResearchAnchor
```

The purpose of the DLT implementation is experimental comparison, not technological preference.

Relevant evaluation dimensions include:

- throughput;
- latency;
- proof size;
- verification cost;
- storage cost;
- operational complexity;
- governance;
- trust assumptions;
- resilience to operator compromise;
- federation properties.

## 15. Persistence

The PoC uses in-memory storage only.

Future persistence must support:

- immutable claim history;
- efficient subject/object queries;
- lineage traversal;
- conflict queries;
- temporal queries;
- source provenance;
- policy reproducibility;
- potentially large metadata volumes.

The project should evaluate whether a relational model, graph database, hybrid approach, or event-oriented persistence provides the best trade-off.

No database technology is selected by the current PoC.

## 16. Infrastructure independence

TraceClaim should remain deployable without mandatory proprietary services.

Cloud platforms may be used for development, demonstration or hosting, but the FOSS core should not require a specific commercial cloud provider.
