# Local PostgreSQL setup for Everkeep

This guide prepares a local PostgreSQL 18 server and the `everkeep_dev` database on Windows. It does not change or remove existing SQL Server installations or databases.

## 1. Install PostgreSQL

1. Open the official [PostgreSQL Windows download page](https://www.postgresql.org/download/windows/) and choose the interactive installer by EDB. The installer includes the PostgreSQL server, pgAdmin, and optional command-line tools.
2. Select the latest stable PostgreSQL 18.x release for 64-bit Windows. Do not select a beta or release candidate.
3. Keep **PostgreSQL Server**, **pgAdmin 4**, and **Command Line Tools** selected. Stack Builder is optional.
4. Keep the default installation and data directories unless you have a specific reason to move them.
5. Set the server port to `5432`, unless another PostgreSQL server already uses it. If you choose another port, use that same port in the pgAdmin registration and the application's User Secret.
6. Set a strong local password for the database administrator account `postgres`. Store it in a password manager; it is separate from the Everkeep application role password.
7. Finish the installer. It registers PostgreSQL as a Windows service. See the [EDB Windows installation guide](https://www.enterprisedb.com/docs/supported-open-source/postgresql/installing/windows/) for installer details.

## 2. Confirm the Windows service is running

1. Press **Win+R**, enter `services.msc`, and press **Enter**.
2. Find the PostgreSQL 18 service. The service name commonly resembles `postgresql-x64-18`; use the name shown on this computer if it differs.
3. Confirm its status is **Running** and its startup type is **Automatic**. If it is stopped, right-click it and select **Start**. To recover a stopped server later, use the same screen to start or restart the service. Restarting disconnects active database clients.
4. Open PowerShell and check that port `5432` accepts connections. With the default installer location, run:

   ```powershell
   & 'C:\Program Files\PostgreSQL\18\bin\pg_isready.exe' -h localhost -p 5432
   ```

   If PostgreSQL is installed elsewhere or uses another port, adjust the executable path or port. An `accepting connections` response confirms that the server is ready. `pg_isready` is one of the PostgreSQL command-line tools included by the installer.

## 3. Register the server in pgAdmin

1. Open pgAdmin 4. In the browser tree, right-click **Servers** and choose **Register → Server**.
2. On **General**, enter a name such as `Everkeep Local — PostgreSQL 18`.
3. On **Connection**, use:

   | Setting | Value |
   | --- | --- |
   | Host name/address | `localhost` |
   | Port | `5432` |
   | Maintenance database | `postgres` |
   | Username | `postgres` |
   | Password | The local password chosen in the installer |

4. Save. Saving the administrator password in pgAdmin is optional.

## 4. Create the Everkeep application role

Create a dedicated login role so the application does not use the database administrator account:

1. Expand the registered server, right-click **Login/Group Roles**, and choose **Create → Login/Group Role**.
2. On **General**, set **Name** to `everkeep_app`.
3. On **Definition**, enter a strong local password. Do not paste that password into a tracked SQL file or repository document.
4. On **Privileges**, enable **Can login**. Keep **Superuser**, **Create roles**, **Create databases**, **Can initiate streaming replication and backups**, and **Bypass RLS** disabled. The default **Inherit rights from parent roles** setting may remain enabled.
5. Save the role.

## 5. Create the development database

1. Right-click **Databases** and choose **Create → Database**.
2. Set **Database** to `everkeep_dev` and **Owner** to `everkeep_app`.
3. Leave the default encoding as UTF8 and save.

## 6. Verify the application connection

Register a second server in pgAdmin for the application role, or open a new connection using:

| Setting | Value |
| --- | --- |
| Host | `localhost` |
| Port | `5432` |
| Database | `everkeep_dev` |
| Username | `everkeep_app` |
| Password | The local password set for `everkeep_app` |

Open Query Tool for that connection and run:

```sql
SELECT current_database(), current_user, version();
```

The first two values should be `everkeep_dev` and `everkeep_app`; the server version should be PostgreSQL 18.x. The `everkeep_app` role should not be a superuser or have permission to create roles or databases.

## 7. Configure Everkeep without committing credentials

Use Visual Studio **Manage User Secrets** for the Everkeep project and set `ConnectionStrings:DefaultConnection` to the local connection string. The password belongs only in User Secrets, never in `appsettings.json`, a SQL script, or a commit. The repository README contains a placeholder format and the application runs EF Core migrations with:

```powershell
dotnet tool restore
dotnet ef database update
dotnet run
```

## Troubleshooting

- **No response from `pg_isready`:** Check the PostgreSQL service in `services.msc`, then confirm the host and port. If another PostgreSQL version uses port `5432`, either stop the unused service or assign a different port and update the connection settings consistently.
- **Password authentication failed:** Check that the connection uses the matching role password. The administrator account `postgres` and application account `everkeep_app` have separate passwords.
- **Everkeep cannot find `DefaultConnection`:** Confirm the secret was added to the Everkeep project, not a different Visual Studio project, and that the application is running in the Development environment.

Do not delete PostgreSQL data directories or change SQL Server services as a troubleshooting step for this setup.
