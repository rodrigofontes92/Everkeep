# Everkeep data audit and privacy policy

- **Last reviewed:** 2026-10-23
- **Policy status:** Implemented and covered by DEV-10 authorization tests

## Versioned database audit

The tracked `app.db` file was inspected read-only before reuse or publication. It is an empty SQLite/ASP.NET Identity schema scaffold, not a populated Everkeep development database:

- The Identity user, role, claim, login, role-membership, and token tables contain zero rows.
- `__EFMigrationsHistory` contains one schema-history row.
- There are no application tables for connections, journal entries, or messages.
- The file contains no account, email, profile, authentication, connection, journal, or message records.

The active application database is PostgreSQL through Npgsql, configured by `Program.cs` with `ConnectionStrings:DefaultConnection`; the local connection string is supplied through User Secrets. Production application code does not use the SQLite file. DEV-9 removed the obsolete database from version control and its MSBuild copy-to-output rule. SQLite WAL/SHM sidecar files are ignored so local artifacts cannot be added accidentally.

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

## Implementation status

DEV-10 implemented the visibility rules and added controller-level authorization tests:

- `ProfileController.Index` returns profile fields only to the owner and accepted connections, returns email only to the owner, and queries public journal entries only when the viewer is allowed to see the profile.
- `DiarioController` lists only the signed-in user's journal entries, defaults new entries to private, and checks ownership before editing, changing privacy or importance, and deleting entries.
- `tests/Everkeep.Tests/ProfileAndJournalAuthorizationTests.cs` covers allowed and denied profile and journal access, including cross-user mutation attempts.

Run the suite with `dotnet test --project tests/Everkeep.Tests/Everkeep.Tests.csproj`. The current policy tests use SQLite in memory and do not exercise a live PostgreSQL server or full browser flows.

## Scope

This policy covers email, profile name, bio, emotional state, and journal entries. Message content, connection-management data, retention periods, and account deletion need their own explicit review before product behavior or promises are changed.
