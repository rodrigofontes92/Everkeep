using System.Security.Claims;
using System.Reflection;
using Everkeep.Controllers;
using Everkeep.Data;
using Everkeep.Models;
using Everkeep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Everkeep.Tests;

public sealed class ProfileAndJournalAuthorizationTests
{
    [Fact]
    public async Task Profile_hides_sensitive_fields_and_journal_from_non_connections()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        var viewer = CreateUser("viewer", "Viewer", "viewer@example.test");
        await using var testContext = await TestContext.CreateAsync(owner, viewer);
        testContext.Database.Diarios.AddRange(
            CreateEntry(owner.Id, "Public entry", isPrivate: false),
            CreateEntry(owner.Id, "Private entry", isPrivate: true));
        await testContext.Database.SaveChangesAsync();

        var controller = CreateProfileController(testContext, viewer.Id);
        var result = await controller.Index(owner.Id);

        var model = Assert.IsType<ProfileViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.False(model.PodeVerPerfil);
        Assert.Equal(string.Empty, model.Nome);
        Assert.Equal(string.Empty, model.Email);
        Assert.Equal(string.Empty, model.Bio);
        Assert.Equal(string.Empty, model.EstadoEmocional);
        Assert.Empty(model.DiariosPublicos);
    }

    [Theory]
    [InlineData("Pendente")]
    [InlineData("Rejeitado")]
    public async Task Profile_hides_sensitive_fields_when_connection_is_not_accepted(string state)
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        var viewer = CreateUser("viewer", "Viewer", "viewer@example.test");
        await using var testContext = await TestContext.CreateAsync(owner, viewer);
        testContext.Database.Conexoes.Add(new Conexao
        {
            User1Id = viewer.Id,
            User2Id = owner.Id,
            Estado = state
        });
        await testContext.Database.SaveChangesAsync();

        var controller = CreateProfileController(testContext, viewer.Id);
        var result = await controller.Index(owner.Id);

        var model = Assert.IsType<ProfileViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.False(model.PodeVerPerfil);
        Assert.Equal(string.Empty, model.Nome);
        Assert.Equal(string.Empty, model.Email);
        Assert.Equal(string.Empty, model.Bio);
        Assert.Equal(string.Empty, model.EstadoEmocional);
    }

    [Fact]
    public async Task Accepted_connections_see_profile_fields_and_public_journal_but_not_email()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        var viewer = CreateUser("viewer", "Viewer", "viewer@example.test");
        await using var testContext = await TestContext.CreateAsync(owner, viewer);
        testContext.Database.Conexoes.Add(new Conexao
        {
            User1Id = viewer.Id,
            User2Id = owner.Id,
            Estado = "Aceite"
        });
        testContext.Database.Diarios.AddRange(
            CreateEntry(owner.Id, "Public entry", isPrivate: false),
            CreateEntry(owner.Id, "Private entry", isPrivate: true));
        await testContext.Database.SaveChangesAsync();

        var controller = CreateProfileController(testContext, viewer.Id);
        var result = await controller.Index(owner.Id);

        var model = Assert.IsType<ProfileViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.True(model.PodeVerPerfil);
        Assert.Equal(owner.Nome, model.Nome);
        Assert.Equal(owner.Bio, model.Bio);
        Assert.Equal(owner.EstadoEmocional, model.EstadoEmocional);
        Assert.Equal(string.Empty, model.Email);
        Assert.Collection(model.DiariosPublicos, entry => Assert.Equal("Public entry", entry.Conteudo));
    }

    [Fact]
    public async Task Owner_can_see_own_email_and_profile_fields()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        await using var testContext = await TestContext.CreateAsync(owner);

        var controller = CreateProfileController(testContext, owner.Id);
        var result = await controller.Index(null);

        var model = Assert.IsType<ProfileViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.True(model.PodeVerPerfil);
        Assert.Equal(owner.Nome, model.Nome);
        Assert.Equal(owner.Email, model.Email);
        Assert.Equal(owner.Bio, model.Bio);
        Assert.Equal(owner.EstadoEmocional, model.EstadoEmocional);
    }

    [Fact]
    public async Task Profile_edit_updates_only_the_signed_in_user()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        var viewer = CreateUser("viewer", "Viewer", "viewer@example.test");
        await using var testContext = await TestContext.CreateAsync(owner, viewer);
        var controller = CreateProfileController(testContext, viewer.Id);

        var result = await controller.Edit(new ProfileViewModel
        {
            UserId = owner.Id,
            Nome = "Changed name",
            Bio = "Changed bio",
            EstadoEmocional = "Changed status"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Owner", owner.Nome);
        Assert.Equal("Owner bio", owner.Bio);
        Assert.Equal("Calm", owner.EstadoEmocional);
        Assert.Equal("Changed name", viewer.Nome);
        Assert.Equal("Changed bio", viewer.Bio);
        Assert.Equal("Changed status", viewer.EstadoEmocional);
    }

    [Fact]
    public async Task Journal_index_returns_only_the_signed_in_users_entries()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        var other = CreateUser("other", "Other", "other@example.test");
        await using var testContext = await TestContext.CreateAsync(owner, other);
        testContext.Database.Diarios.AddRange(
            CreateEntry(owner.Id, "Owner private", isPrivate: true),
            CreateEntry(owner.Id, "Owner public", isPrivate: false),
            CreateEntry(other.Id, "Other private", isPrivate: true));
        await testContext.Database.SaveChangesAsync();

        var controller = CreateJournalController(testContext, owner.Id);
        var result = await controller.Index();

        var model = Assert.IsAssignableFrom<IEnumerable<DiarioViewModel>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(2, model.Count());
        Assert.Contains(model, entry => entry.Conteudo == "Owner private");
        Assert.Contains(model, entry => entry.Conteudo == "Owner public");
        Assert.DoesNotContain(model, entry => entry.Conteudo == "Other private");
    }

    [Fact]
    public async Task Journal_create_assigns_the_owner_and_private_defaults()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        await using var testContext = await TestContext.CreateAsync(owner);
        var controller = CreateJournalController(testContext, owner.Id);

        var result = await controller.Criar(new DiarioViewModel
        {
            Conteudo = "New entry",
            IsPrivado = false,
            IsImportante = true
        });

        Assert.IsType<RedirectToActionResult>(result);
        var entry = await testContext.Database.Diarios.SingleAsync();
        Assert.Equal(owner.Id, entry.UserId);
        Assert.True(entry.IsPrivado);
        Assert.False(entry.IsImportante);
    }

    [Fact]
    public async Task Journal_mutations_reject_entries_owned_by_another_user()
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        var viewer = CreateUser("viewer", "Viewer", "viewer@example.test");
        await using var testContext = await TestContext.CreateAsync(owner, viewer);
        var entry = CreateEntry(owner.Id, "Original entry", isPrivate: true);
        testContext.Database.Diarios.Add(entry);
        await testContext.Database.SaveChangesAsync();
        var controller = CreateJournalController(testContext, viewer.Id);

        Assert.IsType<NotFoundResult>(await controller.Editar(entry.Id));
        Assert.IsType<NotFoundResult>(await controller.Editar(entry.Id, new DiarioViewModel { Conteudo = "Changed" }));
        Assert.IsType<NotFoundResult>(await controller.MarcarPrivado(entry.Id));
        Assert.IsType<NotFoundResult>(await controller.MarcarImportante(entry.Id));
        Assert.IsType<NotFoundResult>(await controller.Eliminar(entry.Id));

        Assert.Equal("Original entry", entry.Conteudo);
        Assert.True(entry.IsPrivado);
        Assert.False(entry.IsImportante);
        Assert.Equal(1, await testContext.Database.Diarios.CountAsync());
    }

    [Fact]
    public void Profile_and_journal_controllers_require_authenticated_users()
    {
        Assert.NotNull(typeof(ProfileController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(typeof(DiarioController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
    }

    private static ApplicationUser CreateUser(string id, string name, string email) => new()
    {
        Id = id,
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        Nome = name,
        Bio = $"{name} bio",
        EstadoEmocional = "Calm"
    };

    private static Diario CreateEntry(string userId, string content, bool isPrivate) => new()
    {
        UserId = userId,
        Conteudo = content,
        DataCriacao = DateTime.UtcNow,
        IsPrivado = isPrivate,
        IsImportante = false
    };

    private static ProfileController CreateProfileController(TestContext testContext, string userId) => new(testContext.Database, testContext.UserManager)
    {
        ControllerContext = CreateControllerContext(userId)
    };

    private static DiarioController CreateJournalController(TestContext testContext, string userId) => new(testContext.Database, testContext.UserManager)
    {
        ControllerContext = CreateControllerContext(userId)
    };

    private static ControllerContext CreateControllerContext(string userId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId)],
                authenticationType: "Test"))
        }
    };

    private sealed class TestContext : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestContext(SqliteConnection connection, ApplicationDbContext database, UserManager<ApplicationUser> userManager)
        {
            _connection = connection;
            Database = database;
            UserManager = userManager;
        }

        public ApplicationDbContext Database { get; }
        public UserManager<ApplicationUser> UserManager { get; }

        public static async Task<TestContext> CreateAsync(params ApplicationUser[] users)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;
            var database = new ApplicationDbContext(options);
            await database.Database.EnsureCreatedAsync();
            database.Users.AddRange(users);
            await database.SaveChangesAsync();

            return new TestContext(connection, database, CreateUserManager(users));
        }

        public async ValueTask DisposeAsync()
        {
            await Database.DisposeAsync();
            await _connection.DisposeAsync();
        }

        private static UserManager<ApplicationUser> CreateUserManager(IEnumerable<ApplicationUser> users)
        {
            var store = new TestUserStore(users);
            return new UserManager<ApplicationUser>(
                store,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new ServiceCollection().BuildServiceProvider(),
                NullLogger<UserManager<ApplicationUser>>.Instance);
        }
    }

    private sealed class TestUserStore : IUserStore<ApplicationUser>
    {
        private readonly Dictionary<string, ApplicationUser> _users;

        public TestUserStore(IEnumerable<ApplicationUser> users)
        {
            _users = users.ToDictionary(user => user.Id);
        }

        public void Dispose()
        {
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _users[user.Id] = user;
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _users[user.Id] = user;
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _users.Remove(user.Id);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Values.FirstOrDefault(user => user.NormalizedUserName == normalizedUserName));
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }
    }
}
