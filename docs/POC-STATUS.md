# PoC Status

## Purpose

This document provides a concise boundary between what currently exists in the public TraceClaim proof of concept and what remains future work.

This distinction is intentional and important for technical review, research planning and funding discussions.

## Implemented

### Core domain

- `SoftwareClaim`
- `IssuedAt` / `ObservedAt` distinction
- `ClaimRelation`
- `Supports`, `ConflictsWith`, `Supersedes`

### Domain services

- simple `affected-by` vs `not-affected-by` conflict detection
- issuer-side temporal lineage

### Claim graph

- in-memory claims
- in-memory relations
- relation endpoint validation
- duplicate edge avoidance
- subject queries
- conflict queries
- current-claim query based on supersession

### Trust policies

- conservative security demonstration policy
- vendor-preferred demonstration policy

### OSV

- limited source DTOs
- explicit affected versions
- package PURL use/building for selected ecosystems
- mapping to `affected-by`
- source publication time to `IssuedAt`

### OpenVEX

- limited source DTOs
- product/vulnerability/status mapping
- selected status mapping
- document/statement timestamp handling
- author mapped to logical issuer

### Tests

- unit tests for core behavior
- adapter tests
- synthetic JSON parsing tests
- cross-source conflict test

## Partially implemented / intentionally simplified

### Software identity

PURL is currently treated largely as a string.

### Vulnerability identity

No alias resolution.

### Conflict semantics

Only a small demonstration rule set.

### Lineage

Only straightforward same-issuer subject/object sequencing.

### Trust policies

Hard-coded demonstration implementations; no policy language or configuration model.

### Graph

In-memory only.

## Not implemented

- complete OSV specification support;
- OSV range evaluation;
- complete OpenVEX validation;
- CSAF;
- SPDX;
- CycloneDX;
- SLSA ingestion;
- in-toto attestations;
- canonical claim schema;
- deterministic hashing;
- cryptographic signing;
- signature verification;
- issuer identity verification;
- Sigstore;
- Rekor;
- Merkle trees;
- blockchain/DLT;
- scalable storage;
- graph database;
- REST API;
- CLI;
- authentication;
- production deployment;
- security audit.

## Interpretation

The PoC demonstrates that the architectural concept is implementable.

It does not demonstrate that the full system is complete, scalable, secure or standards-conformant.

That distinction should be preserved in all public descriptions of the project.
