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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            // Lista temporária para guardar data e texto da interação
            var interacoes = new List<(DateTime data, string texto)>();

            // Filtra apenas mensagens onde o utilizador enviou ou recebeu
            // Ordena da mais recente para a mais antiga
            // Limita o resultado às 5 mais recentes
            var mensagens = await _context.Mensagens
                .Where(m => m.RemetenteId == userId || m.DestinatarioId == userId)
                .OrderByDescending(m => m.DataEnvio)
                .Take(5)
                .ToListAsync();

            // Adiciona cada mensagem à lista
            foreach (var m in mensagens)
            {
                interacoes.Add((m.DataEnvio, "💬 " + m.Conteudo));
            }

            // Apenas diários do utilizador autenticado
            var diarios = await _context.Diarios
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DataCriacao)
                .Take(5)
                .ToListAsync();

            // Adiciona cada entrada do diário à lista de interações.
            foreach (var d in diarios)
            {
                interacoes.Add((d.DataCriacao, "📝 " + d.Conteudo));
            }

            // ordenar por data
            // Transforma cada item da lista - mantém apenas o texto
            var ultimas = interacoes
                .OrderByDescending(i => i.data)
                .Take(5)
                .Select(i => i.texto)
                .ToList();

            // garantir pelo menos 3 mensagens fake (fallback UX)
            if (ultimas.Count < 3)
            {
                ultimas.Add("✨ Comece a criar memórias especiais.");
                ultimas.Add("💡 Envie uma mensagem a alguém importante.");
                ultimas.Add("📖 Escreva algo no seu diário hoje.");
            }

            // Cria o ViewModel enviado para a View.
            var model = new DashboardViewModel
            {
                Nome = user.Nome,
                UltimasInteracoes = ultimas.Take(5).ToList()
            };

            return View(model);
        }

    }
}
