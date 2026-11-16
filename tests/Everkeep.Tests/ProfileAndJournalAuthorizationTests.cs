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
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

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
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

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
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

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
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

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
        var entry = await testContext.Database.Diarios.SingleAsync(Xunit.TestContext.Current.CancellationToken);
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
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);
        var controller = CreateJournalController(testContext, viewer.Id);

        Assert.IsType<NotFoundResult>(await controller.Editar(entry.Id));
        Assert.IsType<NotFoundResult>(await controller.Editar(entry.Id, new DiarioViewModel { Conteudo = "Changed" }));
        Assert.IsType<NotFoundResult>(await controller.MarcarPrivado(entry.Id));
        Assert.IsType<NotFoundResult>(await controller.MarcarImportante(entry.Id));
        Assert.IsType<NotFoundResult>(await controller.Eliminar(entry.Id));

        Assert.Equal("Original entry", entry.Conteudo);
        Assert.True(entry.IsPrivado);
        Assert.False(entry.IsImportante);
        Assert.Equal(1, await testContext.Database.Diarios.CountAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("Pendente")]
    [InlineData("Rejeitado")]
    public async Task Message_send_is_blocked_when_connection_is_not_accepted(string state)
    {
        var sender = CreateUser("sender", "Sender", "sender@example.test");
        var recipient = CreateUser("recipient", "Recipient", "recipient@example.test");
        await using var testContext = await TestContext.CreateAsync(sender, recipient);
        testContext.Database.Conexoes.Add(new Conexao
        {
            User1Id = sender.Id,
            User2Id = recipient.Id,
            Estado = state
        });
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);
        var controller = CreateMessageController(testContext, sender.Id);

        var result = await controller.Enviar(recipient.Id, "Hello");

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(await testContext.Database.Mensagens.ToListAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Accepted_connections_can_exchange_messages()
    {
        var sender = CreateUser("sender", "Sender", "sender@example.test");
        var recipient = CreateUser("recipient", "Recipient", "recipient@example.test");
        await using var testContext = await TestContext.CreateAsync(sender, recipient);
        testContext.Database.Conexoes.Add(new Conexao
        {
            User1Id = sender.Id,
            User2Id = recipient.Id,
            Estado = "Aceite"
        });
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);
        var controller = CreateMessageController(testContext, sender.Id);

        var result = await controller.Enviar(recipient.Id, "Hello");

        Assert.IsType<RedirectToActionResult>(result);
        var message = await testContext.Database.Mensagens.SingleAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(sender.Id, message.RemetenteId);
        Assert.Equal(recipient.Id, message.DestinatarioId);
        Assert.Equal("Hello", message.Conteudo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task Message_send_rejects_empty_or_whitespace_content(string content)
    {
        var sender = CreateUser("sender", "Sender", "sender@example.test");
        var recipient = CreateUser("recipient", "Recipient", "recipient@example.test");
        await using var testContext = await TestContext.CreateAsync(sender, recipient);
        var controller = CreateMessageController(testContext, sender.Id);

        var result = await controller.Enviar(recipient.Id, content);

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(await testContext.Database.Mensagens.ToListAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Only_message_participants_can_favorite_and_only_sender_can_delete()
    {
        var sender = CreateUser("sender", "Sender", "sender@example.test");
        var recipient = CreateUser("recipient", "Recipient", "recipient@example.test");
        var outsider = CreateUser("outsider", "Outsider", "outsider@example.test");
        await using var testContext = await TestContext.CreateAsync(sender, recipient, outsider);
        var message = CreateMessage(sender.Id, recipient.Id, "Hello");
        testContext.Database.Mensagens.Add(message);
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

        var outsiderController = CreateMessageController(testContext, outsider.Id);
        Assert.IsType<NotFoundResult>(await outsiderController.MsgFavorita(message.Id));
        Assert.IsType<NotFoundResult>(await outsiderController.Eliminar(message.Id));
        Assert.False(message.IsFavorita);
        Assert.Equal(1, await testContext.Database.Mensagens.CountAsync(Xunit.TestContext.Current.CancellationToken));

        var recipientController = CreateMessageController(testContext, recipient.Id);
        Assert.IsType<RedirectResult>(await recipientController.MsgFavorita(message.Id));
        Assert.True(message.IsFavorita);
        Assert.IsType<NotFoundResult>(await recipientController.Eliminar(message.Id));
        Assert.Equal(1, await testContext.Database.Mensagens.CountAsync(Xunit.TestContext.Current.CancellationToken));

        var senderController = CreateMessageController(testContext, sender.Id);
        Assert.IsType<RedirectResult>(await senderController.Eliminar(message.Id));
        Assert.Empty(await testContext.Database.Mensagens.ToListAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Only_invite_recipient_can_accept_or_reject_and_only_participants_can_remove()
    {
        var sender = CreateUser("sender", "Sender", "sender@example.test");
        var recipient = CreateUser("recipient", "Recipient", "recipient@example.test");
        var outsider = CreateUser("outsider", "Outsider", "outsider@example.test");
        await using var testContext = await TestContext.CreateAsync(sender, recipient, outsider);
        var invitation = new Conexao
        {
            User1Id = sender.Id,
            User2Id = recipient.Id,
            Estado = "Pendente"
        };
        testContext.Database.Conexoes.Add(invitation);
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

        var senderController = CreateConnectionController(testContext, sender.Id);
        var outsiderController = CreateConnectionController(testContext, outsider.Id);
        var recipientController = CreateConnectionController(testContext, recipient.Id);

        Assert.IsType<NotFoundResult>(await senderController.Aceitar(invitation.Id));
        Assert.IsType<NotFoundResult>(await senderController.Rejeitar(invitation.Id));
        Assert.IsType<NotFoundResult>(await outsiderController.Aceitar(invitation.Id));
        Assert.Equal("Pendente", invitation.Estado);

        Assert.IsType<RedirectToActionResult>(await recipientController.Aceitar(invitation.Id));
        Assert.Equal("Aceite", invitation.Estado);
        Assert.IsType<NotFoundResult>(await outsiderController.Remover(invitation.Id));
        Assert.Equal(1, await testContext.Database.Conexoes.CountAsync(Xunit.TestContext.Current.CancellationToken));

        Assert.IsType<RedirectToActionResult>(await recipientController.Remover(invitation.Id));
        Assert.Empty(await testContext.Database.Conexoes.ToListAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Memories_include_only_the_current_users_received_favorites_and_important_journals()
    {
        var viewer = CreateUser("viewer", "Viewer", "viewer@example.test");
        var sender = CreateUser("sender", "Sender", "sender@example.test");
        var other = CreateUser("other", "Other", "other@example.test");
        await using var testContext = await TestContext.CreateAsync(viewer, sender, other);
        testContext.Database.Mensagens.AddRange(
            CreateMessage(sender.Id, viewer.Id, "My received favorite", isFavorite: true),
            CreateMessage(other.Id, sender.Id, "Someone else's favorite", isFavorite: true));
        testContext.Database.Diarios.AddRange(
            CreateImportantEntry(viewer.Id, "My important journal"),
            CreateImportantEntry(other.Id, "Someone else's journal"));
        await testContext.Database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);
        var controller = CreateMemoriesController(testContext, viewer.Id);

        var result = await controller.Index();

        var memories = Assert.IsAssignableFrom<IEnumerable<MemoriaViewModel>>(
            Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(2, memories.Count());
        Assert.Contains(memories, memory => memory.Conteudo == "My received favorite");
        Assert.Contains(memories, memory => memory.Conteudo == "My important journal");
        Assert.DoesNotContain(memories, memory => memory.Conteudo == "Someone else's favorite");
        Assert.DoesNotContain(memories, memory => memory.Conteudo == "Someone else's journal");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task Journal_create_rejects_empty_or_whitespace_content(string content)
    {
        var owner = CreateUser("owner", "Owner", "owner@example.test");
        await using var testContext = await TestContext.CreateAsync(owner);
        var controller = CreateJournalController(testContext, owner.Id);

        var result = await controller.Criar(new DiarioViewModel { Conteudo = content });

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(await testContext.Database.Diarios.ToListAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Profile_and_journal_controllers_require_authenticated_users()
    {
        Assert.NotNull(typeof(ProfileController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(typeof(DiarioController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(typeof(MensagemController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(typeof(ConexaoController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(typeof(MemoriasController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
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

    private static Diario CreateImportantEntry(string userId, string content) => new()
    {
        UserId = userId,
        Conteudo = content,
        DataCriacao = DateTime.UtcNow,
        IsPrivado = true,
        IsImportante = true
    };

    private static Mensagem CreateMessage(string senderId, string recipientId, string content, bool isFavorite = false) => new()
    {
        RemetenteId = senderId,
        DestinatarioId = recipientId,
        Conteudo = content,
        DataEnvio = DateTime.UtcNow,
        IsFavorita = isFavorite,
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

    private static MensagemController CreateMessageController(TestContext testContext, string userId) => new(testContext.Database, testContext.UserManager)
    {
        ControllerContext = CreateControllerContext(userId)
    };

    private static ConexaoController CreateConnectionController(TestContext testContext, string userId) => new(testContext.Database, testContext.UserManager)
    {
        ControllerContext = CreateControllerContext(userId)
    };

    private static MemoriasController CreateMemoriesController(TestContext testContext, string userId) => new(testContext.Database, testContext.UserManager)
    {
        ControllerContext = CreateControllerContext(userId)
    };

    private static ControllerContext CreateControllerContext(string userId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId)],
                authenticationType: "Test"))
        };
        httpContext.Request.Headers.Referer = "/";
        return new ControllerContext { HttpContext = httpContext };
    }

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
            await connection.OpenAsync(Xunit.TestContext.Current.CancellationToken);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;
            var database = new ApplicationDbContext(options);
            await database.Database.EnsureCreatedAsync(Xunit.TestContext.Current.CancellationToken);
            database.Users.AddRange(users);
            await database.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

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
