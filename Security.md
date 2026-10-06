Security Policy
Project status
TraceClaim is currently an early research proof of concept.
It has not undergone an independent security audit and must not currently be treated as a production security decision engine.
Current security limitations
The PoC does not yet provide:
cryptographic signature verification;
verified issuer identity;
canonical claim hashing;
signed attestations;
tamper-evident persistent storage;
authentication or authorization;
hardened input validation;
rate limiting;
production persistence;
threat-model-complete handling of untrusted metadata;
supply-chain security guarantees for TraceClaim itself.
Reporting a vulnerability
Please report security issues privately to the project maintainers rather than publishing exploit details in a public issue.
A useful report should include:
affected component;
reproduction steps;
expected behavior;
observed behavior;
impact assessment;
suggested mitigation, if known.
Threat categories of particular interest
Future TraceClaim work should explicitly consider:
Malicious metadata
An attacker may supply structurally valid but intentionally misleading claims.
Identity spoofing
A source identifier such as a URI is not by itself proof of issuer identity.
Signature confusion
Signed content may be validly signed by an identity that is not authorized for the relevant claim type.
Canonicalization attacks
Ambiguous serialization may cause signature or hash mismatches, duplicate identities or semantic confusion.
Replay
Old valid claims may be replayed after they have been superseded.
Equivocation
A source may issue conflicting claims to different consumers.
Poisoned relationship inference
Incorrect identity mapping can create false conflicts or false supersession.
Policy manipulation
A valid claim graph can still produce unsafe results if policy configuration is compromised.
Log or anchor compromise
Transparency and DLT mechanisms have different operator and governance trust assumptions that must be modeled explicitly.
Security philosophy
TraceClaim should never present cryptographic validity as semantic truth.
The intended verification pipeline is conceptually:
```text
Structural validation
       |
       v
Cryptographic verification
       |
       v
Issuer identity / authorization
       |
       v
Evidence and lineage analysis
       |
       v
Consumer trust policy
       |
       v
Resolution
```
Each stage answers a different security question.