# Standards Mapping

## 1. Purpose

TraceClaim is designed to reuse existing software supply-chain standards rather than replace them.

This document records the intended role of each relevant standard or ecosystem and clearly distinguishes what is implemented in the PoC from what remains planned.

## 2. Summary table

| Standard / project | Role in TraceClaim | PoC status |
|---|---|---|
| Package URL (PURL) | Common software package identity | Partial string-level use |
| OSV | Vulnerability source claims | Partial adapter implemented |
| OpenVEX | Product vulnerability-impact claims | Partial adapter implemented |
| CSAF | Security advisory information | Planned |
| SPDX | SBOM / software metadata | Planned |
| CycloneDX | SBOM / VEX / software metadata | Planned |
| in-toto Attestation Framework | Candidate attestation envelope/model | Planned research |
| SLSA | Build provenance | Planned |
| Sigstore | Signing identity and verification ecosystem | Planned |
| Rekor | Transparency-log baseline | Planned research |
| W3C PROV | General provenance concepts | Conceptual reference |

## 3. Package URL (PURL)

TraceClaim uses PURL where possible to identify concrete package versions.

Example:

```text
pkg:nuget/Acme.Security@2.1.0
```

Current PoC limitations:

- PURLs are stored as strings;
- there is no formal parser in Core;
- canonicalization is not implemented;
- aliases are not resolved;
- ecosystem-specific case rules are not fully addressed.

Planned work should introduce proper identity handling rather than manual string concatenation.

## 4. OSV

OSV provides machine-readable vulnerability metadata with package-level affected information.

The current TraceClaim OSV adapter consumes only a subset:

```text
id
published
modified
affected[].package
affected[].versions
```

Current mapping:

```text
OSV advisory ID -> SoftwareClaim.Object
OSV package/version -> SoftwareClaim.Subject
OSV affected entry -> Predicate = affected-by
OSV published -> IssuedAt
TraceClaim ingestion time -> ObservedAt
```

### Explicit versions

Implemented.

Example source semantics:

```text
package X version 2.1.0 is affected by advisory Y
```

becomes:

```text
X@2.1.0 --affected-by--> Y
```

### Version ranges

Not implemented.

OSV ranges require correct version semantics for each ecosystem. TraceClaim should not implement unsafe comparisons using lexical string ordering.

Future work must use ecosystem-correct version comparison and range expansion/evaluation.

## 5. OpenVEX

OpenVEX models vulnerability impact through statements that associate products, vulnerabilities and status.

The PoC currently maps:

| OpenVEX status | TraceClaim predicate |
|---|---|
| `affected` | `affected-by` |
| `not_affected` | `not-affected-by` |
| `fixed` | `fixed-for` |
| `under_investigation` | `under-investigation-for` |

Current mapping:

```text
product @id -> Subject
vulnerability.name -> Object
author -> Issuer
statement timestamp -> IssuedAt
fallback document timestamp -> IssuedAt
```

OpenVEX v0.2.0 describes a statement as the intersection of product(s), a vulnerability and an impact status. The PoC aligns directly with that structure.

Current limitations include:

- incomplete validation;
- incomplete inheritance handling;
- no signature validation;
- no formal issuer identity model;
- no complete justification/action semantics in the common claim model.

## 6. SPDX

SPDX is a major standard for software bills of materials and software package metadata.

Potential TraceClaim use cases include:

- package identity assertions;
- license assertions;
- dependency relationships;
- external references;
- provenance of SBOM metadata.

No SPDX adapter exists in the current PoC.

## 7. CycloneDX

CycloneDX can represent SBOM and related software supply-chain information, including vulnerability and VEX-style metadata.

Potential TraceClaim use cases include:

- component identity;
- dependency relationships;
- vulnerability analysis;
- VEX statements;
- evidence mapping.

No CycloneDX adapter exists in the current PoC.

## 8. CSAF

Common Security Advisory Framework (CSAF) provides structured security advisory information.

A future CSAF adapter could provide another independent source of vulnerability claims and therefore create useful multi-authority conflict scenarios.

Not implemented.

## 9. in-toto Attestation Framework

The in-toto Attestation Framework defines authenticated metadata around software artifacts using layers including Statement, Predicate, Envelope and Bundle.

This makes it a strong candidate for representing future cryptographically authenticated TraceClaim assertions without inventing a proprietary signing envelope.

Possible conceptual mapping:

```text
TraceClaim software identity -> in-toto subject
TraceClaim claim metadata    -> predicate
TraceClaim predicate schema  -> predicateType
signed serialization         -> envelope
```

This remains a design hypothesis and requires interoperability evaluation.

No in-toto output is implemented in the current PoC.

## 10. SLSA

SLSA provenance describes how software artifacts were produced.

TraceClaim does not intend to replace SLSA provenance.

A future adapter may translate selected provenance assertions into claims or relate SLSA attestations to package identities and other metadata.

Not implemented.

## 11. Sigstore

Sigstore provides tooling and infrastructure for signing and verifying software artifacts and related metadata.

TraceClaim may use Sigstore to authenticate future claim attestations.

Important architectural principle:

```text
Signature validity != semantic truth
```

Sigstore can help verify who signed an assertion and that the signed content was not altered. Trust policy remains a separate layer.

Not implemented.

## 12. Rekor

Rekor is Sigstore's transparency-log component and provides an append-only, tamper-resistant log for signed software supply-chain metadata.

TraceClaim considers Rekor the natural baseline against which any DLT-based anchoring experiment should be compared.

The project should not build a custom transparency log unless existing infrastructure cannot satisfy a concrete requirement.

Not implemented.

## 13. W3C PROV

W3C PROV provides general concepts for representing provenance.

TraceClaim's distinction between assertion, issuer, evidence and derivation is conceptually aligned with provenance-oriented systems.

The PoC does not currently serialize data as PROV.

## 14. Standards strategy

TraceClaim should follow these rules:

1. Prefer existing identifiers and formats.
2. Preserve source semantics.
3. Avoid converting unsupported constructs into guessed meanings.
4. Keep source adapters isolated from Core.
5. Introduce new schemas only where an actual interoperability gap remains.
6. Version any TraceClaim-specific predicate or canonical format explicitly.
