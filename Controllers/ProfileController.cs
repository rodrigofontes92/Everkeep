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

        public async Task<IActionResult> Index(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Se não vier userId, mostrar o próprio perfil
            if (string.IsNullOrEmpty(userId))
                userId = currentUserId;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();


            // verifica a primeira conexão encontrada
            var conexao = await _context.Conexoes.FirstOrDefaultAsync(c =>
                (c.User1Id == currentUserId && c.User2Id == userId) ||
                (c.User1Id == userId && c.User2Id == currentUserId)
            );

            // Verifica se existe conexão - se não existir, estado = "Nenhuma"
            string estado = conexao?.Estado ?? "Nenhuma";

            // podeVer é true se estado = "Aceite" ou o utilizador está no seu próprio perfil
            bool podeVer = estado == "Aceite" || currentUserId == userId;


            // somente diários públicos
            var diariosPublicos = await _context.Diarios
                .Where(d => d.UserId == userId && !d.IsPrivado)
                .OrderByDescending(d => d.DataCriacao)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                CurrentUserId = currentUserId,
                Nome = user.Nome,
                Email = user.Email,
                Bio = user.Bio,
                EstadoConexao = estado,
                PodeVerPerfil = podeVer,
                EstadoEmocional = user.EstadoEmocional,

                // Cada entidade Diario num DiarioViewModel
                DiariosPublicos = diariosPublicos.Select(d => new DiarioViewModel
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
