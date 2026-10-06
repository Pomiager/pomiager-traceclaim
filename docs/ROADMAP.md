# Roadmap

## 1. Purpose

This roadmap deliberately separates the **existing PoC** from the substantial work required for a reusable FOSS implementation.

The current repository should be read as evidence of feasibility, not as a completed product.

## 2. Phase 0 — Existing PoC

**Status: implemented**

Goals:

- validate the normalized claim concept;
- distinguish `IssuedAt` from `ObservedAt`;
- represent support/conflict/supersession relationships;
- demonstrate temporal lineage;
- demonstrate consumer-controlled resolution;
- normalize a limited subset of OSV;
- normalize a limited subset of OpenVEX;
- detect a simple cross-source vulnerability conflict;
- validate behavior through automated tests.

Explicit non-goals:

- production readiness;
- standards completeness;
- cryptographic verification;
- scalable persistence;
- public API;
- blockchain integration.

## 3. Phase 1 — Domain and canonical claim model

**Status: planned**

### Objectives

- formalize software identity;
- formalize vulnerability identity and aliases;
- define typed predicates;
- define claim schema versioning;
- define evidence representation;
- define canonical serialization;
- define deterministic claim hashing.

### Key research questions

- Which fields belong to canonical identity?
- How should optional evidence be represented?
- Can the model map naturally into an in-toto predicate?
- How should extension fields remain forward-compatible?

### Deliverables

- versioned claim specification;
- canonicalization specification;
- reference .NET implementation;
- conformance tests;
- published examples.

## 4. Phase 2 — Standards-complete interoperability

**Status: planned**

### OSV

- ranges;
- events such as introduced/fixed/last_affected;
- aliases;
- severity/evidence mapping where useful;
- ecosystem-correct version handling;
- withdrawal/update semantics.

### OpenVEX

- validation;
- complete inheritance semantics;
- richer justification/action mapping;
- statement identity;
- document version relationships;
- signature-related metadata where applicable.

### Additional sources

Candidate adapters:

- CSAF;
- SPDX;
- CycloneDX;
- SLSA provenance;
- vendor advisory formats.

### Deliverables

- adapters;
- interoperability test corpus;
- mapping documentation;
- unsupported-semantics diagnostics.

## 5. Phase 3 — Identity and alias resolution

**Status: planned**

### Objectives

- robust PURL parsing and canonicalization;
- ecosystem-aware package identity;
- vulnerability alias mapping;
- package aliases and namespaces;
- identity equivalence evidence.

### Research risk

Incorrect identity merging can create false conflicts or hide real ones. Identity resolution must therefore be evidence-based and auditable.

## 6. Phase 4 — Generalized lineage and conflict engine

**Status: planned**

The current conflict detector is intentionally simplistic.

Future work should support:

- version-range overlap;
- statement scopes;
- package/component hierarchy;
- different vulnerability identifiers referring to aliases;
- conflicting license claims;
- conflicting origin/provenance claims;
- support relationships;
- supersession chains;
- derived relationships with algorithm metadata.

### Deliverables

- relation-inference engine;
- deterministic relationship rules;
- explainable diagnostics;
- adversarial test cases.

## 7. Phase 5 — Configurable trust policy framework

**Status: planned**

### Objectives

- declarative policies;
- issuer roles;
- rule precedence;
- policy versioning;
- reproducible resolutions;
- unresolved/disputed states;
- optional weighting models;
- policy evaluation traces.

### Important constraint

Policy evaluation must remain separate from evidence storage.

## 8. Phase 6 — Cryptographic attestations

**Status: planned**

### Objectives

- authenticate canonical claims;
- bind issuer identity;
- preserve evidence digests;
- verify signatures;
- represent attestation receipts.

### Standards research

Evaluate:

- in-toto Attestation Framework;
- DSSE-compatible envelopes;
- Sigstore-compatible signing.

### Deliverables

- attestation profile;
- signer/verifier implementation;
- verification test suite;
- interoperability documentation.

## 9. Phase 7 — Transparency log integration

**Status: planned**

Rekor should be evaluated as a baseline for append-only public verifiability.

Research should cover:

- submission model;
- inclusion proofs;
- verification receipts;
- identity relationships;
- private/federated deployment scenarios;
- operational trust assumptions.

## 10. Phase 8 — Merkle batching

**Status: planned**

Large claim volumes may make one external anchor operation per claim inefficient.

Research should evaluate:

```text
Claim hashes
     |
     v
Merkle tree
     |
     v
Merkle root
     |
     v
External anchor
```

Requirements include:

- deterministic leaf encoding;
- proof generation;
- proof verification;
- batch identity;
- append/update semantics;
- storage of receipts.

## 11. Phase 9 — Experimental DLT anchoring

**Status: planned research**

A DLT adapter should be implemented only as a pluggable alternative.

The research question is:

> Does distributed-ledger anchoring provide measurable benefits over established transparency-log infrastructure for federated software metadata claims?

### Evaluation dimensions

- throughput;
- latency;
- transaction cost;
- proof size;
- verification complexity;
- node/operator trust;
- censorship resistance;
- federation/governance;
- availability;
- operational burden.

A result showing that a transparency log is preferable is considered a valid research outcome.

## 12. Phase 10 — Persistence and scalable query

**Status: planned**

The PoC graph is in-memory.

Future work should evaluate:

- relational persistence;
- graph persistence;
- hybrid models;
- event-oriented storage;
- temporal queries;
- large-scale indexing;
- incremental relation inference.

Potential queries include:

```text
What did source X assert about package Y at time T?
Which current claims conflict for package Y?
Which claim superseded claim Z?
What policy produced resolution R?
What evidence existed at historical time T?
```

## 13. Phase 11 — Public API and CLI

**Status: planned**

Potential tooling:

### CLI

```text
traceclaim ingest
traceclaim claims
traceclaim conflicts
traceclaim lineage
traceclaim resolve
traceclaim verify
traceclaim anchor
```

### REST API

Candidate capabilities:

- ingest metadata;
- query claims;
- query lineage;
- evaluate policies;
- verify attestations;
- retrieve anchor receipts.

API design should follow the finalized domain model, not precede it.

## 14. Phase 12 — Packaging and deployment

**Status: planned**

Goals:

- container images;
- reproducible builds;
- CI pipelines;
- deployment documentation;
- self-hosted reference configuration;
- no mandatory proprietary cloud dependency.

## 15. Phase 13 — Security and interoperability audit

**Status: planned**

Before production claims are made, TraceClaim should undergo:

- security code review;
- dependency review;
- canonicalization review;
- signature verification review;
- threat-model validation;
- interoperability testing against real implementations;
- fuzz/adversarial input testing.

## 16. Funding rationale

The existing PoC intentionally covers only Phase 0.

The main engineering and research value lies in the planned phases:

```text
PoC feasibility
      |
      v
standards completeness
      |
      v
canonicalization
      |
      v
cryptographic verification
      |
      v
scalable lineage
      |
      v
trust-policy framework
      |
      v
transparency / DLT evaluation
      |
      v
reusable FOSS tooling
```

This separation is deliberate so that public pre-existing work demonstrates capability without prematurely implementing the work intended for future funded development.
