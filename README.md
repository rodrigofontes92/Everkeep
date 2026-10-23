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

The project follows the MVC (Model-View-Controller) pattern.

```text
Everkeep
│
├── Controllers
├── Models
├── ViewModels
├── Views
├── Data
├── Areas
│   └── Identity
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
- Authorization using ```[Authorize]```
- Ownership validation
- Protected private content
- Secure password storage

Data visibility rules and the repository database audit are documented in the [Data and Privacy Policy](docs/privacy-and-data-policy.md).

---

## Running the Project

### Prerequisites

- Visual Studio 2026
- .NET SDK 10
- PostgreSQL 18 ([Windows installation and local database setup](docs/postgresql-local-setup.md))

### Setup

- Clone the repository:

```git clone <repository-url>```

- Navigate to the project:

```cd Everkeep```

- Store the local connection string with Visual Studio's **Manage User Secrets** command for the Everkeep project. Use this key and format:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=everkeep_dev;Username=everkeep_app;Password=<your-local-password>"
  }
}
```

Do not commit the password or put it in `appsettings.json`. The PostgreSQL role must own the local `everkeep_dev` database.

- Restore project dependencies:

```dotnet restore```

- Restore the repository's local EF Core command-line tool:

```dotnet tool restore```

- Apply migrations:

```dotnet ef database update```

- Run the application:

```dotnet run```

---

## Tests

Run the profile and journal authorization suite with:

```powershell
dotnet test --project tests/Everkeep.Tests/Everkeep.Tests.csproj
```

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
