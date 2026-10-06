# Trust Policies

## 1. Why policies exist

TraceClaim intentionally refuses to define a universal trust order among software metadata authorities.

Different consumers have different risk models.

For example:

- an operations team may prefer conservative vulnerability handling;
- a vendor may consider its own exploitability analysis authoritative;
- a regulated organization may privilege a national CERT;
- a research system may retain all conflicting statuses without resolving them.

Therefore TraceClaim separates:

```text
Evidence
    from
Resolution
```

## 2. Policy invariant

A trust policy must not mutate, delete or rewrite evidence.

The expected relationship is:

```text
Claims + Policy -> Resolution
```

The evidence graph remains unchanged.

This enables:

- reproducibility;
- auditability;
- comparison of policies;
- retrospective policy changes;
- organization-specific security posture.

## 3. ITrustPolicy

The PoC exposes a generic policy abstraction conceptually equivalent to:

```csharp
public interface ITrustPolicy
{
    string Name { get; }

    TrustResolution Resolve(
        IReadOnlyCollection<SoftwareClaim> claims);
}
```

The policy receives claims and produces a `TrustResolution`.

## 4. TrustResolution

The current PoC resolution contains:

- status;
- selected claim, when applicable;
- policy name;
- human-readable reason.

A future resolution should probably also contain:

- evaluated claim IDs;
- policy version;
- timestamp;
- confidence or assurance level;
- decision evidence;
- policy parameters;
- cryptographic identity of the policy configuration;
- reproducibility metadata.

## 5. ConservativeSecurityPolicy

Current demonstration rule:

```text
if any current claim says affected-by
    -> resolve affected
else if a current claim says not-affected-by
    -> resolve not-affected
else
    -> unresolved
```

This is intentionally simple.

It demonstrates a security-first policy where uncertainty is treated conservatively.

It must not be interpreted as a recommended universal VEX policy.

## 6. VendorPreferredPolicy

The vendor-preferred demonstration policy is configured with a preferred issuer.

If claims from that issuer exist, the policy selects the latest claim according to `IssuedAt`.

Example:

```text
Vendor T1: affected-by
Vendor T2: not-affected-by
```

The policy chooses T2.

If no preferred-vendor claim exists, the current PoC returns `unresolved`.

The implementation does not automatically fall back to OSV or another authority.

This makes the policy behavior explicit and predictable.

## 7. Same evidence, different resolution

Consider:

```text
OSV    -> affected-by
Vendor -> not-affected-by
```

The evidence is identical for both consumers.

Consumer A:

```text
Policy = conservative-security
Result = affected
```

Consumer B:

```text
Policy = vendor-preferred
Result = not-affected
```

This is not inconsistency in TraceClaim.

It is the intended representation of different trust assumptions.

## 8. Policy versus cryptographic verification

A future signed claim could have states such as:

```text
Signature valid
Issuer recognized
Evidence intact
```

and still be rejected by a policy.

Conversely, a policy may decide not to use an unsigned claim at all.

Therefore the architecture should eventually evaluate at least four independent questions:

```text
1. Is the claim structurally valid?
2. Is its cryptographic evidence valid?
3. Who is the verified issuer?
4. Does this consumer trust that issuer for this predicate and scope?
```

## 9. Planned policy framework

The funded/research evolution may investigate declarative policies such as:

```text
for vulnerability-impact claims:
    trust Vendor at weight 100
    trust CERT-EU at weight 90
    trust OSV at weight 80

if any authority with weight >= 90 says affected:
    affected

if Vendor says not_affected and no higher authority conflicts:
    not_affected

otherwise:
    disputed
```

This is illustrative only. No weighting model is implemented today.

## 10. Potential policy dimensions

Future policies may consider:

- issuer identity;
- issuer role;
- claim type;
- software ecosystem;
- package namespace;
- vulnerability source;
- statement age;
- signature verification;
- evidence availability;
- source confidence;
- consensus among sources;
- explicit organizational allow/deny lists;
- jurisdiction or regulatory context.

## 11. Reproducibility requirement

A future production policy engine should make a resolution reproducible from:

```text
Claim set
+ policy definition
+ policy version
+ evaluation time
+ verified identities
= resolution
```

This is important for audits and historical explanations.
