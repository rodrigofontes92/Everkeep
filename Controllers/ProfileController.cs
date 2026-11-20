using Everkeep.Data;
using Everkeep.Models;
using Everkeep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Everkeep.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(currentUserId))
                return Challenge();

            // Show the signed-in user's profile when no target user is provided.
            if (string.IsNullOrEmpty(userId))
                userId = currentUserId;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var isOwner = currentUserId == userId;
            var connectionState = "Nenhuma";
            var isAcceptedConnection = false;

            if (!isOwner)
            {
                var relationships = _context.Conexoes.Where(c =>
                    (c.User1Id == currentUserId && c.User2Id == userId) ||
                    (c.User1Id == userId && c.User2Id == currentUserId));

                isAcceptedConnection = await relationships.AnyAsync(c => c.Estado == "Aceite");
                connectionState = isAcceptedConnection
                    ? "Aceite"
                    : await relationships.Select(c => c.Estado).FirstOrDefaultAsync() ?? "Nenhuma";
            }

            var canViewProfile = isOwner || isAcceptedConnection;
            var publicJournalEntries = canViewProfile
                ? await _context.Diarios
                    .Where(d => d.UserId == userId && !d.IsPrivado)
                    .OrderByDescending(d => d.DataCriacao)
                    .ToListAsync()
                : new List<Diario>();

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                CurrentUserId = currentUserId,
                Nome = canViewProfile ? user.Nome : string.Empty,
                Email = isOwner ? user.Email ?? string.Empty : string.Empty,
                Bio = canViewProfile ? user.Bio : string.Empty,
                EstadoConexao = connectionState,
                PodeVerPerfil = canViewProfile,
                EstadoEmocional = canViewProfile ? user.EstadoEmocional : string.Empty,
                DiariosPublicos = publicJournalEntries.Select(d => new DiarioViewModel
                {
                    Id = d.Id,
                    Conteudo = d.Conteudo,
                    DataCriacao = d.DataCriacao
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                Nome = user.Nome,
                Bio = user.Bio,
                EstadoEmocional = user.EstadoEmocional
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            user.Nome = model.Nome;
            user.Bio = model.Bio;
            user.EstadoEmocional = model.EstadoEmocional;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index), new { userId = user.Id });
        }

    }
}
