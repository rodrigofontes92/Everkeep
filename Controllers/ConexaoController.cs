using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Everkeep.Data;
using Everkeep.Models;
using Everkeep.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Everkeep.Controllers
{
    [Authorize]
    public class ConexaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConexaoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = GetUserId();

            /*
             * Busca e lista todas as conexões onde o utilizador participa.
             * 
             * Where filtra apenas registos que correspondem à condição.
             */
            var conexoes = await _context.Conexoes
                .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
                .ToListAsync();

            // Carrega todos os utilizadores do sistema.
            var users = await _userManager.Users.ToListAsync();

            var lista = conexoes.Select(c =>
            {
                // Verifica qual utilizador não é o atual
                var outroId = c.User1Id == currentUserId ? c.User2Id : c.User1Id;

                // Busca o primeiro resultado. Null se não encontrar
                var outroUser = users.FirstOrDefault(u => u.Id == outroId);

                return new ConexaoViewModel
                {
                    OutroUserId = outroId,
                    Nome = outroUser?.Nome,
                    Estado = c.Estado,

                    // Define se o convite foi recebido pelo utilizador atual.
                    FoiRecebido = c.User2Id == currentUserId,
                    Id = c.Id
                };
            }).ToList();

            return View(lista);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarConvite(string email)
        {
            var currentUserId = GetUserId();

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Index");

            var userDestino = await _userManager.FindByEmailAsync(email);

            if (userDestino == null)
            {
                TempData["Erro"] = "Utilizador não encontrado.";
                return RedirectToAction("Index");
            }

            if (userDestino.Id == currentUserId)
            {
                TempData["Erro"] = "Não pode enviar convite para si próprio.";
                return RedirectToAction("Index");
            }

            /*
             * AnyAsync verifica se já existe pelo menos uma conexão entre os dois utilizadores.
             * 
             * Retorna true ou false.
             */
            var jaExiste = await _context.Conexoes.AnyAsync(c =>
                (c.User1Id == currentUserId && c.User2Id == userDestino.Id) ||
                (c.User1Id == userDestino.Id && c.User2Id == currentUserId)
            );

            if (jaExiste)
            {
                TempData["Erro"] = "Já existe uma conexão com este utilizador.";
                return RedirectToAction("Index");
            }

            // Cria a nova conexão com estado pendente.
            var conexao = new Conexao
            {
                User1Id = currentUserId,
                User2Id = userDestino.Id,
                Estado = "Pendente"
            };

            _context.Conexoes.Add(conexao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Convite enviado.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceitar(int id)
        {
            var currentUserId = GetUserId();

            // Busca registo via PK.
            var conexao = await _context.Conexoes.FindAsync(id);

            // Garante que apenas o destinatário pode aceitar.
            if (conexao == null || conexao.User2Id != currentUserId)
                return NotFound();

            conexao.Estado = "Aceite";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id)
        {
            var currentUserId = GetUserId();
            var conexao = await _context.Conexoes.FindAsync(id);

            if (conexao == null || conexao.User2Id != currentUserId)
                return NotFound();

            conexao.Estado = "Rejeitado";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id)
        {
            var currentUserId = GetUserId();
            var conexao = await _context.Conexoes.FindAsync(id);

            if (conexao == null)
                return NotFound();

            // Garante que apenas participantes da conexão podem removê-la.
            if (conexao.User1Id != currentUserId && conexao.User2Id != currentUserId)
                return NotFound();

            _context.Conexoes.Remove(conexao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }

}
