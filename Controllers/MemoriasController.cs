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
    public class MemoriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemoriasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Lista que irá reunir mensagens favoritas e entradas importantes.
            var lista = new List<MemoriaViewModel>();

            // Busca apenas mensagens recebidas pelo utilizador e marcadas como favoritas
            var mensagensFavoritas = await _context.Mensagens
                .Where(m =>
                m.DestinatarioId == userId &&
                m.IsFavorita)                
                .OrderByDescending(m => m.DataEnvio)
                .ToListAsync();

            foreach (var m in mensagensFavoritas)
            {
                // Identifica o outro participante da conversa
                var outroUserId = m.RemetenteId == userId ? m.DestinatarioId : m.RemetenteId;

                // Procura o utilizador associado à mensagem.
                var outroUser = await _userManager.FindByIdAsync(outroUserId);

                // Adiciona a memória à lista final.
                lista.Add(new MemoriaViewModel
                {
                    Tipo = "Mensagem",
                    Conteudo = m.Conteudo,
                    Data = m.DataEnvio,
                    OutroUserNome = outroUser?.Nome
                });
            }

            // Busca apenas diários do utilizador e marcados como importantes
            var diarios = await _context.Diarios
                .Where(d =>
                d.UserId == userId &&
                d.IsImportante)
                .ToListAsync();

            // Adiciona a entrada à lista final
            foreach (var d in diarios)
            {
                lista.Add(new MemoriaViewModel
                {
                    Tipo = "Diario",
                    Conteudo = d.Conteudo,
                    Data = d.DataCriacao
                });
            }

            // Ordenação, mais recentes aos mais antigos
            lista = lista.OrderByDescending(x => x.Data).ToList();

            return View(lista);
        }
    }
}
