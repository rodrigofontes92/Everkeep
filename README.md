# Everkeep

Everkeep is a web application designed to strengthen meaningful human connections through emotional communication, personal reflection, and memory preservation.

Unlike traditional messaging platforms that prioritize volume and speed, Everkeep focuses on the emotional value of interactions, allowing users to save, organize, and revisit meaningful moments over time.

---

## Project Purpose

Everkeep was created as an academic software engineering project with the goal of providing a safe and welcoming digital space where users can:

- Communicate through private messages
- Build trusted connections
- Record personal thoughts and reflections
- Preserve emotionally significant memories
- Revisit important moments through curated content

The platform is particularly inspired by situations involving emotional vulnerability, where meaningful communication becomes especially important.

---

## Key Features

### User Authentication

- Registration and login
- ASP.NET Identity integration
- Secure password hashing
- Session management

### User Profiles

- Custom profile information
- Biography
- Emotional status indicator
- Profile editing

### Connections

- Send connection requests
- Accept or reject invitations
- Manage personal network
- Remove existing connections

### Private Messaging

- One-to-one conversations
- Conversation history
- Favorite messages
- Message deletion

### Personal Journal

- Create journal entries
- Edit entries
- Private/public visibility
- Important content marking

### Memory Panel

A dedicated area that automatically gathers:

- Favorite messages
- Important journal entries

Creating a personal collection of meaningful memories.

### Dashboard

- Personalized greeting
- Emotional support messages
- Recent activity overview
- Quick access to core features

---

## Technologies Used

- ASP.NET Core MVC
- C#
- Entity Framework Core
- ASP.NET Identity
- PostgreSQL 18 or a newer stable release
- Razor Pages
- Bootstrap 5
- HTML5
- CSS3
- JavaScript

---

## Architecture

Everkeep is an ASP.NET Core MVC application targeting .NET 10. It uses ASP.NET Identity for authentication and account management, Entity Framework Core for data access, and Razor views for server-rendered pages. PostgreSQL is the active application database, accessed through the Npgsql EF Core provider.

```text
Everkeep
│
├── Controllers
├── Models
├── ViewModels
├── Views
├── Data
├── Migrations
├── Areas
│   └── Identity
├── tests
│   └── Everkeep.Tests
└── wwwroot
```

---

## Core Entities

- ApplicationUser
- Conexao (Connection)
- Mensagem (Message)
- Diario (Journal Entry)

---

## Database

The application uses Entity Framework Core with the Code First approach.

Main relationships include:

- User ↔ Connections
- User ↔ Messages
- User ↔ Journal Entries

Database schema evolution is managed through EF Core Migrations.

The application uses PostgreSQL through the Npgsql EF Core provider. The older SQL Server migration files are retained under `Migrations/SqlServerLegacy` for reference and are excluded from the active PostgreSQL migration assembly. Existing SQL Server data is not converted by this setup.

---

## Security

- ASP.NET Identity authentication
- Authorization using `[Authorize]`
- Ownership validation
- Protected private content
- Secure password storage

Data visibility rules and the repository database audit are documented in the [Data and Privacy Policy](docs/privacy-and-data-policy.md).

---

## Running the Project

### Prerequisites

- .NET SDK 10
- PostgreSQL 18 ([Windows installation and local database setup](docs/postgresql-local-setup.md))
- Visual Studio 2026 is optional; the setup and run commands use the .NET CLI.

### Setup

- Clone the repository:

```powershell
git clone https://github.com/rodrigofontes92/Everkeep.git
```

- Navigate to the project:

```powershell
cd Everkeep
```

- Install PostgreSQL, create the local `everkeep_dev` database and the `everkeep_app` role by following the [local PostgreSQL setup guide](docs/postgresql-local-setup.md).

- Store the local connection string in User Secrets for the Everkeep project. From the repository root, run the following command and replace the example password. Alternatively, use Visual Studio's **Manage User Secrets** command for the project.

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=everkeep_dev;Username=everkeep_app;Password=your-local-password"
```

The equivalent User Secrets JSON structure is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=everkeep_dev;Username=everkeep_app;Password=<your-local-password>"
  }
}
```

Do not commit the password or put it in `appsettings.json`. The PostgreSQL role must own the local `everkeep_dev` database.

- Restore dependencies and the repository's local EF Core command-line tool:

```powershell
dotnet restore
dotnet tool restore
```

- Apply database migrations and run the application:

```powershell
dotnet ef database update -- --environment Development
dotnet run
```

The migration command selects the Development environment so EF Core can load the local connection string from User Secrets. The `http` launch profile also runs in Development at `http://localhost:5071`.

---

## Tests

Run the profile and journal authorization suite with:

```powershell
dotnet test --project tests/Everkeep.Tests/Everkeep.Tests.csproj
```

The tests use an in-memory SQLite database. They do not connect to the local PostgreSQL server. The current suite focuses on profile visibility, journal access, and ownership checks.

---

## Continuous Integration

GitHub Actions runs the **Build and test** workflow defined in `.github/workflows/build-and-test.yml`. It restores dependencies, builds the web application in Release, and runs the test suite for pull requests targeting `develop` or `main`, pushes to those branches, and manual workflow dispatches.

The workflow verifies build and test results. It does not deploy the application because a hosting provider has not been selected.

---

## Current Limitations

- Everkeep is an academic MVP and does not have a production hosting environment or managed database configured.
- Automated coverage currently focuses on profile and journal authorization. End-to-end UI tests and integration tests against PostgreSQL have not been added.
- Timeline controller, view model, and view files are present in the repository but excluded from the current application build.
- The legacy SQL Server migration files are retained for reference. Converting existing SQL Server data to PostgreSQL is not implemented.

---

## Future Improvements

- Real-time messaging (SignalR)
- Mobile application
- Media attachments
- Advanced memory organization
- Notifications
- Emotional analysis features
- AI-powered recommendations

---

## Author

Rodrigo Fontes
Software Developer | Digital Marketing Specialist
Portugal

---

## License

This project was developed for educational and portfolio purposes.
