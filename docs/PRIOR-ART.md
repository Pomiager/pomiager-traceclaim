# Prior Art and Positioning

## 1. Purpose

TraceClaim is not proposed as a replacement for existing software supply-chain standards, SBOM systems, provenance frameworks, signing infrastructure, transparency logs or blockchain registries.

This document summarizes the relevant technical landscape and explains the narrower gap TraceClaim explores.

## 2. The wrong positioning

TraceClaim should **not** be described as:

- a new SBOM format;
- a new vulnerability database;
- a new provenance standard;
- a new transparency log;
- a blockchain for SBOMs;
- a generic knowledge graph;
- a replacement for in-toto, SLSA, Sigstore, GUAC or OSV.

Those areas already contain substantial standards and open-source infrastructure.

## 3. GUAC

GUAC aggregates heterogeneous software supply-chain metadata into a queryable graph.

It is highly relevant because it demonstrates the value of connecting SBOM, provenance, vulnerability and other metadata around common software identities.

TraceClaim's intended distinction is narrower:

```text
GUAC focus:
aggregation + synthesis + query

TraceClaim research focus:
verifiable temporal claim lineage + explicit conflict + consumer resolution
```

The projects may be complementary rather than competitive.

## 4. Grafeas

Grafeas provides a common metadata API for software supply-chain information and introduced useful separation between generalized metadata descriptions and occurrences tied to concrete artifacts.

This demonstrates that common representations for heterogeneous supply-chain metadata are not a new idea.

TraceClaim therefore should not claim novelty merely from normalization.

## 5. in-toto

in-toto provides a framework for verifiable claims and authenticated metadata concerning software production.

This is highly relevant prior art for TraceClaim's planned attestation work.

Rather than designing a proprietary signed envelope, TraceClaim should evaluate whether an in-toto predicate can represent claim-lineage metadata.

## 6. SLSA

SLSA provides a framework for software supply-chain integrity and provenance.

It addresses questions such as where and how software artifacts were built.

TraceClaim focuses on a different but adjacent question:

> What happens when independent metadata authorities make different claims about the same software object?

SLSA provenance may become one input to TraceClaim rather than a competing model.

## 7. Sigstore and Rekor

Sigstore provides software signing infrastructure. Rekor provides an append-only transparency log for signed software supply-chain metadata.

These systems already solve substantial parts of:

- signing;
- identity binding;
- tamper evidence;
- inclusion proofs;
- public auditability.

Therefore TraceClaim should not claim that it needs blockchain simply to make metadata immutable.

Rekor is a strong baseline for future TraceClaim experimentation.

## 8. Blockchain-based SBOM and provenance approaches

Several projects and research efforts have already explored:

- SBOM hashes on blockchain;
- dependency graphs anchored to Ethereum-like networks;
- permissioned blockchain for software asset management;
- distributed lifecycle tracking;
- tamper-resistant SBOM registries.

Therefore:

```text
SBOM -> hash -> blockchain
```

is not a sufficient innovation claim for TraceClaim.

## 9. W3C PROV and provenance research

General provenance systems have long recognized that different agents can produce different descriptions of the same events or entities.

TraceClaim's claim-versus-truth distinction is therefore grounded in established provenance thinking rather than being a new theoretical invention.

## 10. Nanopublications and provenance-driven knowledge claims

Nanopublication systems demonstrate decentralized publication of small assertions with provenance and publication information.

Research such as provenance-driven nanopublication approaches also demonstrates multi-source evidence, supporting and conflicting assertions, and trust relationships.

Therefore TraceClaim should not claim invention of "conflicting claims" as a generic concept.

## 11. VEX conflict handling

VEX ecosystems already recognize that different statements can exist for the same product/vulnerability relationship.

Different tools may resolve disagreement conservatively or according to their own policies.

This provides evidence that conflicting software metadata is a practical problem, not merely a theoretical scenario.

## 12. TraceClaim's narrower research gap

The intended gap is the **integration** of existing ideas in a software-specific federated metadata context.

A cautious positioning statement is:

> Existing standards and tools address individual aspects of software metadata trust: package identity, SBOMs, vulnerability data, VEX, provenance attestations, signing, transparency and general-purpose provenance. These capabilities remain fragmented. TraceClaim explores a standards-oriented layer that preserves heterogeneous software claims as a verifiable temporal lineage, makes contradictions and supersession explicit, and keeps final trust resolution under consumer control.

This is deliberately narrower than claiming that no prior system can represent conflicting assertions.

## 13. Proposed differentiators

Potential differentiators to validate through future work include:

### Software-specific normalization

Common representation across OSV, VEX, SBOM, advisory and provenance ecosystems while preserving original evidence.

### Temporal claim lineage

Explicit issuer-side history of corrections and supersession.

### Conflict as first-class metadata

Contradictions are preserved rather than silently overwritten during aggregation.

### Consumer-controlled resolution

Different organizations can reproduce different decisions from the same underlying evidence.

### Cryptographically verifiable history

Future work should bind canonical claims and their provenance to verifiable attestations.

### Federated suitability

The model should support metadata produced and governed by independent organizations.

## 14. Research humility

TraceClaim should avoid absolute statements such as:

> No existing system supports conflicting metadata.

A more defensible statement is:

> We have not identified a broadly adopted FOSS layer that combines software-specific cross-format normalization, temporal claim lineage, explicit contradictions and consumer-specific resolution with verifiable provenance for federated metadata catalogues.

This is a research hypothesis to validate, not a marketing claim to assume.
