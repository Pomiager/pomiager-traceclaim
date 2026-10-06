# Motivation

## 1. The problem

Software supply-chain security depends on metadata produced by many independent actors.

For a single package version, consumers may need to combine information from:

- package registries;
- source repositories;
- vulnerability databases;
- software vendors;
- SBOM generators;
- VEX producers;
- build systems;
- CI/CD pipelines;
- security scanners;
- provenance services;
- transparency logs;
- compliance tooling.

These sources are not guaranteed to agree.

A vulnerability database may report a package as affected while the vendor publishes a VEX statement declaring that the vulnerable code path is not reachable in its product. A later advisory may revise an earlier statement. A metadata collector may discover an older assertion after a newer one. Multiple identifiers may refer to the same logical software object. Different formats may express similar semantics in incompatible ways.

The problem is therefore not only data collection.

It is also:

```text
Who asserted what?
When was it asserted?
When was it observed?
What evidence supported it?
Did the assertion change?
What superseded it?
Which independent sources agree?
Which sources conflict?
How should a particular consumer resolve the conflict?
Can the history be independently verified?
```

## 2. A key distinction: provenance is not truth

TraceClaim is built around an explicit distinction between **verifiability** and **truth**.

A digital signature can provide evidence that:

- a particular identity signed a payload;
- the payload has not changed since signing;
- the signature can be validated against a trust root.

A transparency log can additionally provide evidence that:

- a signed object was recorded;
- the record is part of an append-only history;
- the object existed no later than a particular point represented by the log.

A blockchain or distributed ledger can provide different forms of replicated, tamper-evident state and governance.

None of these mechanisms can automatically prove that the semantic content of a claim is correct.

For example:

```text
Vendor A signs:
"Package X is not affected by CVE-Y"
```

A valid signature proves that Vendor A signed the assertion. It does not prove that the vulnerability assessment is correct.

This leads to a central TraceClaim rule:

> **Integrity and provenance must remain separate from semantic trust.**

## 3. Why multiple claims must be preserved

A common metadata architecture tends to converge toward a single current value:

```text
Package X
Vulnerability status = affected
```

This is convenient for queries but can hide important information.

Suppose the history is:

```text
T1 OSV     : affected
T2 Vendor  : not affected
T3 CERT    : affected
T4 Vendor  : fixed in version 2.1.1
```

Collapsing this into one field loses:

- provenance;
- disagreement;
- temporal evolution;
- source authority;
- the ability to reproduce a past decision.

TraceClaim instead treats each statement as an independently preserved claim.

## 4. Claims versus resolutions

TraceClaim separates two different objects:

### Claim

A claim records what a source asserted.

```text
Issuer    : OSV
Subject   : package X
Predicate : affected-by
Object    : CVE-Y
IssuedAt  : T1
```

### Resolution

A resolution is a consumer-specific interpretation of a set of claims.

For the same evidence:

```text
Conservative security policy -> affected
Vendor-preferred policy      -> not affected
```

Neither resolution changes the underlying claims.

This makes decisions reproducible:

```text
Evidence + Policy = Resolution
```

rather than:

```text
Database field = Truth
```

## 5. Temporal lineage

Metadata evolves.

A vendor may first report:

```text
Package X affected by CVE-Y
```

and later publish:

```text
Package X not affected by CVE-Y
```

TraceClaim represents this as lineage:

```text
NewClaim --Supersedes--> OldClaim
```

The old claim remains part of the historical record.

This is important for audit scenarios. A consumer should be able to reconstruct what information was available and which policy was applied at a particular time.

## 6. Issued time versus observed time

Distributed metadata collection creates another subtle problem.

A source can publish a statement at T1 while TraceClaim discovers it only at T3.

Therefore:

```text
IssuedAt != ObservedAt
```

TraceClaim stores both.

This allows two independent timelines to be reconstructed:

### Issuer timeline

When the authority says its statements were issued.

### Observation timeline

When TraceClaim actually acquired those statements.

This distinction is critical when ingestion occurs out of order.

## 7. Why a common claim layer is useful

OSV, VEX, SBOM formats, provenance attestations, vendor advisories and other metadata systems each solve different parts of the software supply-chain problem.

TraceClaim does not aim to replace them.

Instead, it investigates whether their relevant assertions can be mapped into a common, verifiable claim layer while preserving source semantics.

The long-term conceptual pipeline is:

```text
External standards
        |
        v
Source-specific adapters
        |
        v
Normalized claims
        |
        v
Claim lineage + relationships
        |
        v
Cryptographic attestations
        |
        v
Consumer trust policy
        |
        v
Resolution
```

## 8. Why TraceClaim is currently a PoC

The existing implementation deliberately validates only the central architectural hypothesis:

- multiple formats can be normalized;
- provenance can be preserved;
- temporal lineage can be modeled;
- conflicts can remain explicit;
- resolution can remain consumer-controlled.

It does not yet claim to solve the full interoperability problem.

Important unresolved research and engineering work includes:

- complete semantics for source standards;
- package identity canonicalization;
- version-range interpretation;
- claim canonicalization;
- cryptographic signing and verification;
- scalable persistence;
- policy configuration;
- federated operation;
- tamper-evident anchoring;
- transparency-log versus DLT evaluation.

See `ROADMAP.md` for the proposed evolution.
