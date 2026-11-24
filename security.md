# Security Audit

**Audit type:** AI-assisted repository review  
**Repository:** HazardDataApp  
**Audit date:** 2026-11-24  
**Scope:** Supabase configuration and access, database models and repositories,
desktop article/report workflows, shared configuration, and Git ignore rules

## AI security comments

1. **High — Article persistence may use an unapproved or mismatched schema.**
   The proposed `sql/create_articles.sql` creates `author` and
   `date_published` columns, while `ArticleRecord` and the repository use
   `is_published` and `created_at`. In addition, the branch's article code
   performs insert, update, and delete operations before a team-approved article
   policy exists. This can break deployments or create unintended database
   behavior. The team should finalize the table contract and RLS policies before
   these operations are enabled for users.

2. **High — Authorization is enforced only by server-side Supabase RLS.**
   The app has no authentication session in the reviewed code. Public report and
   article operations therefore depend entirely on existing Supabase policies.
   Anonymous users must not receive read or write access to private reports or
   administrative article operations.

3. **Medium — No explicit maximum lengths or content limits are enforced in the app.**
   Article title and body are checked only for whitespace. Very large input can
   consume device resources or fail only when the database rejects it. Report
   text and article text may also require validation based on the team's defined
   data requirements.

4. **Medium — Repository operations assume every mutation returns exactly one row.**
   `Insert(...).Models.Single()` and update `.Models.Single()` can leak internal
   state through exceptions or fail when RLS prevents the return or when no row
   is affected. Mutations should check for an empty or unexpected result and
   return a clear domain error.

5. **Low — Download and sharing operations should isolate and clean up temporary files.**
   The article and report download workflows write files to the cache and share
   them. Cache retention and sharing permissions are platform-managed, but
   failure handling and cleanup should be explicit. The app should not write
   sensitive data outside the cache without a team requirement.

6. **Low — Exception details can be displayed to end users.**
   Several pages pass raw `Exception.Message` to alerts. This is generally safe
   for the MAUI UI, but it can expose implementation details or backend error
   text. Use stable user-facing messages while logging or reporting technical
   details through an approved diagnostics mechanism.

7. **Approved control — Public Supabase publishable-key handling is appropriate.**
   The URL and publishable key are public client settings, not a server secret.
   The repository explicitly avoids `service_role` and `sb_secret_` keys, and the
   README states that access must come from RLS. The key is not a credential for
   unrestricted administrative access.

8. **Approved control — Shared `.gitignore` rules and SQL defaults reduce secret exposure.**
   Local `SupabaseConfig.cs` overrides are ignored, build artifacts are ignored,
   and the proposed SQL files deny article access until policies are supplied.
   These controls should be preserved.

## Response report

| Finding | Decision | Action taken |
| --- | --- | --- |
| 1. Mismatched article schema | **Rejected ** | AI does not understand different methods can use different values from table and not need all of them. |
| 2. Authorization is RLS-dependent | **Approved — remains a deployment requirement** | No authentication or RLS bypass was introduced. Still needs to be worked on a bit |
| 3. Missing input limits | **Modified** | This is mostly addressed in the UI by trimming and rejecting empty article titles and bodies.  |
| 4. Mutation result assumptions | **Approved — needs implementation follow-up** | No repository behavior was changed during this documentation-only audit. The team should add guarded `SingleOrDefault` checks and domain-specific not-found/permission errors after the Supabase contract is finalized. |
| 5. Temporary file handling | **Modified / rejected for scope** | Cache-directory sharing is retained because it is appropriate for MAUI export workflows and requires no secret storage. New cleanup behavior was rejected for this audit because the team did not request download behavior changes and platform sharing may still need the file. Download/file-write exception handling remains a follow-up. |
| 6. Raw exception text | **Rejected** | The repository's existing alert behavior was left unchanged because the audit found no evidence of secrets or credentials being emitted. If backend logging is added, it should use sanitized errors rather than raw response bodies. |
| 7. Public publishable key | **Approved** | No configuration changes were made. The publishable key remains in shared source, as documented in the README, with service-role and secret-key usage prohibited. |
| 8. Git ignore and RLS safeguards | **Approved** | Existing `.gitignore` and SQL safeguards were preserved; no broader database privileges or secret-key handling were introduced. |

## Required follow-up

Before article persistence is enabled in production:

- finalize and match the `articles` table schema;
- define explicit public read and authenticated/admin write policies; 
- test those policies with anonymous, student, and administrator roles;
- add input limits and mutation-result checks
- verify with database integration tests and a running desktop/Android build.
