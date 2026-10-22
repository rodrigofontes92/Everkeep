# Everkeep data audit and privacy policy

- **Last reviewed:** 2026-10-22
- **Policy status:** Approved for implementation in DEV-10

## Versioned database audit

The tracked `app.db` file was inspected read-only before reuse or publication. It is an empty SQLite/ASP.NET Identity schema scaffold, not a populated Everkeep development database:

- The Identity user, role, claim, login, role-membership, and token tables contain zero rows.
- `__EFMigrationsHistory` contains one schema-history row.
- There are no application tables for connections, journal entries, or messages.
- The file contains no account, email, profile, authentication, connection, journal, or message records.

The active application database is PostgreSQL through Npgsql, configured by `Program.cs` with `ConnectionStrings:DefaultConnection`; the local connection string is supplied through User Secrets. No application code uses the SQLite file. This change removes the obsolete database from the repository and removes its MSBuild copy-to-output rule. SQLite WAL/SHM sidecar files are ignored so local artifacts cannot be added accidentally.

The audited file contained no real or sensitive user data, so no credential rotation or history rewrite is required. The finding is recorded here before removing the empty scaffold.

## Approved visibility and write policy

An **accepted connection** is a relationship whose state is `Aceite`. Pending, rejected, and nonexistent relationships do not grant access. All routes in scope require authentication.

| Data | Owner can read | Accepted connection can read | Other authenticated user can read | Anonymous visitor can read |
| --- | --- | --- | --- | --- |
| Email address | Yes | No | No | No |
| Name, bio, and emotional state | Yes | Yes | No | No |
| Private journal entry and its flags | Yes | No | No | No |
| Public journal entry | Yes | Yes | No | No |

| Data | Who can create, edit, delete, or change visibility |
| --- | --- |
| Email address | The account owner, through authenticated account-management flows |
| Name, bio, and emotional state | The profile owner |
| Journal content, privacy, and importance flags | The journal owner |

New journal entries are private by default. The owner may make an entry public; this grants read access only to accepted connections. “Public” does not mean visible to every registered or anonymous visitor. No connection may alter another user's profile or journal.

## Current implementation findings

The intended policy above is the acceptance target for DEV-10. The current code has these gaps, which must be closed there before the policy is presented as enforced in the application:

- `ProfileController.Index` places the viewed user's name, email, bio, and emotional state in the view model for every authenticated viewer. The profile view renders name, bio, and emotional state without checking `PodeVerPerfil`.
- The profile view currently shows email to accepted connections. The approved policy allows only the owner to read it.
- The profile action queries public journal entries before the view checks whether the viewer is the owner or an accepted connection. The controller should apply that access rule before returning data.
- `DiarioController` already filters the journal page to the signed-in owner, defaults new entries to private, and checks ownership before editing, changing flags/privacy, or deleting an entry.

DEV-10 owns authorization changes and tests for the allowed and denied cases. Until those changes land, the first three findings remain implementation gaps even though the policy and database audit are documented here.

## Scope

This policy covers email, profile name, bio, emotional state, and journal entries. Message content, connection-management data, retention periods, and account deletion need their own explicit review before product behavior or promises are changed.
