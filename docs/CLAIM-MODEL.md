# Claim Model

## 1. Purpose

The TraceClaim claim model is a deliberately small common representation used to normalize assertions from heterogeneous software supply-chain metadata sources.

It is not intended to replace the source standards.

The model should preserve enough semantic information to:

- identify what software object a claim concerns;
- identify what is being asserted;
- identify who made the assertion;
- reconstruct issuer and ingestion timelines;
- relate claims through support, conflict and supersession;
- later attach cryptographic evidence.

## 2. SoftwareClaim

Current conceptual model:

```text
SoftwareClaim
|
|-- Id
|-- Subject
|-- Predicate
|-- Object
|-- Issuer
|-- IssuedAt
|-- ObservedAt
|-- EvidenceUri?
`-- Hash?
```

## 3. Id

`Id` is the TraceClaim-local identity of a claim instance.

Current PoC implementation uses `Guid`.

This identifier should not be confused with:

- the identity of the software package;
- an upstream advisory ID;
- an in-toto subject digest;
- a future canonical claim digest.

A future implementation may derive stable identifiers from canonical content, but that decision has not yet been made.

## 4. Subject

The subject identifies the software object about which the assertion is made.

Where possible, TraceClaim uses Package URL (PURL).

Example:

```text
pkg:nuget/Acme.Security@2.1.0
```

The PoC currently stores the subject as a string.

This is intentionally temporary. Robust subject handling requires:

- PURL parsing;
- canonicalization;
- percent-encoding rules;
- namespaces;
- qualifiers;
- subpaths;
- ecosystem-specific case semantics;
- aliases;
- version normalization.

A future `SoftwareIdentity` value object is likely.

## 5. Predicate

The predicate expresses the semantic relation being asserted.

Examples currently used:

```text
affected-by
not-affected-by
fixed-for
under-investigation-for
licensed-under
```

Predicates are currently represented as strings to keep the PoC flexible while source mappings are explored.

A future implementation may use typed predicates or registered predicate URIs.

## 6. Object

The object is the target of the predicate.

Examples:

```text
CVE-2026-1234
MIT
pkg:nuget/foo@2.0.0
```

Therefore a claim can be read as a simple semantic triple plus provenance:

```text
Subject --Predicate--> Object
```

Example:

```text
pkg:nuget/Acme.Security@2.1.0
          --affected-by-->
CVE-2026-1234
```

## 7. Issuer

The issuer represents the authority responsible for the assertion.

Current examples:

```text
https://osv.dev
https://vendor.example
```

The PoC uses a string because issuer identity is not yet cryptographically modeled.

Future work must distinguish concepts such as:

```text
Logical issuer
Source system
Signing identity
Certificate identity
Organization identity
Collector identity
```

These are not necessarily the same entity.

## 8. IssuedAt

`IssuedAt` represents source-side chronology.

It answers:

> When does the source state that this assertion was issued or became available?

This timestamp is required in the current model.

Adapters map source-specific timestamps to `IssuedAt`.

Example:

```text
OSV published -> IssuedAt
OpenVEX statement timestamp -> IssuedAt
OpenVEX document timestamp -> fallback IssuedAt
```

The precise semantics depend on the source standard.

## 9. ObservedAt

`ObservedAt` belongs to TraceClaim.

It answers:

> When did this TraceClaim instance ingest or observe the assertion?

This distinction supports delayed and out-of-order ingestion.

Example:

```text
Claim A issued     Sep 1
Claim B issued     Sep 5

TraceClaim observes B Sep 6
TraceClaim observes A Sep 10
```

Issuer lineage still follows:

```text
A -> B
```

not ingestion order.

## 10. EvidenceUri

Optional reference to source evidence.

It may point to:

- an advisory;
- a VEX document;
- a source-controlled artifact;
- another retrievable evidence object.

The PoC does not download, preserve or cryptographically bind the referenced document.

Future provenance work must define stronger evidence semantics.

## 11. Hash

The model currently contains an optional `Hash` field, but the PoC does not calculate it.

This is intentional.

A meaningful content digest requires a canonical representation.

Before hashing is implemented, TraceClaim must define:

- fields included in the digest;
- canonical field ordering;
- string normalization;
- timestamp representation;
- URI normalization;
- encoding;
- extension-field handling;
- versioning of the canonical format.

Without this, the same semantic claim could generate different hashes.

## 12. Claim relations

Current relations are:

### Supports

One claim provides support for another claim.

Not yet inferred automatically by the PoC.

### ConflictsWith

Two claims make semantically incompatible assertions.

Current PoC example:

```text
same subject
same vulnerability

affected-by
    vs
not-affected-by
```

### Supersedes

A newer claim from the same issuer replaces an older issuer-side assertion in the current logical timeline.

```text
NewClaim --Supersedes--> OldClaim
```

Supersession does not delete the old claim.

## 13. Claim identity versus semantic identity

Two claim objects may be different records but semantically equivalent.

Example:

```text
Claim #A
Issuer: OSV
Subject: X
Predicate: affected-by
Object: Y

Claim #B
Issuer: OSV
Subject: X
Predicate: affected-by
Object: Y
```

Whether these should be deduplicated depends on timestamps, source evidence, signatures and canonical identity rules.

The PoC intentionally avoids premature semantic deduplication.

## 14. Current-state claims

A claim is currently considered non-current if another claim has a `Supersedes` relationship targeting it.

This is only temporal state.

It does **not** imply:

- truth;
- higher trust;
- cryptographic validity;
- source authority.

## 15. Future extensions

Likely future extensions include:

- typed software identity;
- canonical vulnerability identity and aliases;
- explicit evidence objects;
- signature metadata;
- signing identity;
- source collector identity;
- confidence;
- validity interval;
- statement scope;
- derivation metadata;
- canonical digest;
- attestation ID;
- policy-evaluation metadata;
- schema version.

These extensions should be introduced only after interoperability requirements are validated against real source standards.
