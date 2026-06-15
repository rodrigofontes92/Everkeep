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
    public class TimelineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimelineController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var lista = new List<TimelineViewModel>();

            // Mensagens
            var mensagens = await _context.Mensagens
                .Where(m => m.RemetenteId == userId || m.DestinatarioId == userId)
                .ToListAsync();

            foreach (var m in mensagens)
            {
                var outroUserId = m.RemetenteId == userId ? m.DestinatarioId : m.RemetenteId;
                var outroUser = await _userManager.FindByIdAsync(outroUserId);

                lista.Add(new TimelineViewModel
                {
                    Tipo = "Mensagem",
                    Conteudo = m.Conteudo,
                    Data = m.DataEnvio,
                    OutroUserNome = outroUser?.Nome
                });
            }

            // Diário
            var diarios = await _context.Diarios
                .Where(d => d.UserId == userId)
                .ToListAsync();

            foreach (var d in diarios)
            {
                lista.Add(new TimelineViewModel
                {
                    Tipo = "Diario",
                    Conteudo = d.Conteudo,
                    Data = d.DataCriacao
                });
            }

            // Ordenação
            lista = lista.OrderByDescending(x => x.Data).ToList();

            return View(lista);
        }
    }
}
