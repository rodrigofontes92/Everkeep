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
    public class MensagemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MensagemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            // Busca todas as mensagens que o utilizador enviou ou recebeu
            var mensagens = await _context.Mensagens
                .Where(m => m.RemetenteId == userId || m.DestinatarioId == userId)
                .ToListAsync();

            // Agrupar por utilizador
            var conversas = mensagens

                // Se o utilizador atual for o remetente, agrupa o destinatário
                // Caso contrário, agrupa o remetente
                .GroupBy(m => m.RemetenteId == userId ? m.DestinatarioId : m.RemetenteId)

                // Ordena as mensagens e pega apenas a mais recente
                .Select(g => g.OrderByDescending(m => m.DataEnvio).First())
                .ToList();

            var lista = new List<ConversaViewModel>();

            foreach (var m in conversas)
            {
                // Identifica o outro utilizador da conversa.
                var outroUserId = m.RemetenteId == userId ? m.DestinatarioId : m.RemetenteId;
                var outroUser = await _userManager.FindByIdAsync(outroUserId);

                lista.Add(new ConversaViewModel
                {
                    OutroUserId = outroUserId,
                    Nome = outroUser?.Nome,
                    UltimaMensagem = m.Conteudo,
                    DataUltimaMensagem = m.DataEnvio
                });
            }

            // ordenar por mais recente
            lista = lista.OrderByDescending(c => c.DataUltimaMensagem).ToList();

            return View(lista);

        }

        public async Task<IActionResult> Nova()
        {
            var currentUserId = GetUserId();

            // Busca apenas conexões com Estado "Aceite" do utilizador autenticado
            var conexoes = await _context.Conexoes
                .Where(c => c.Estado == "Aceite" &&
                       (c.User1Id == currentUserId || c.User2Id == currentUserId))
                .ToListAsync();

            // cada conexão fica como o id do outro utilizador
            var userIds = conexoes.Select(c =>
                c.User1Id == currentUserId ? c.User2Id : c.User1Id
            );

            // Verifica se o id do utilizador está em userIds
            var utilizadores = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            return View(utilizadores);
        }

        public async Task<IActionResult> Chat(string userId)
        {
            var currentUserId = GetUserId();

            // Impede acesso sem utilizador válido.
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            // Regra: só pode ver chat se conexão for aceite
            var conexaoExiste = await _context.Conexoes.AnyAsync(c =>
                ((c.User1Id == currentUserId && c.User2Id == userId) ||
                 (c.User1Id == userId && c.User2Id == currentUserId))
                && c.Estado == "Aceite"
            );

            // Apenas conexões aceites podem trocar mensagens.
            if (!conexaoExiste)
                return NotFound();

            // Busca todas as mensagens entre os dois utilizadores
            var mensagens = await _context.Mensagens
                .Where(m =>
                    (m.RemetenteId == currentUserId && m.DestinatarioId == userId) ||
                    (m.RemetenteId == userId && m.DestinatarioId == currentUserId))
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();

            // Procura o outro utilizador da conversa.
            var outroUser = await _userManager.FindByIdAsync(userId);

            // Cria o ViewModel enviado para a View.
            var model = new ChatViewModel
            {
                OutroUserId = userId,
                NomeOutroUser = outroUser?.Nome,
                Mensagens = mensagens
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enviar(string destinatarioId, string conteudo)
        {
            var remetenteId = GetUserId();

            // Impede envio vazio.
            if (string.IsNullOrEmpty(destinatarioId) || string.IsNullOrEmpty(conteudo))
                return BadRequest();

            // Regra crítica: validar conexão aceite
            var conexaoExiste = await _context.Conexoes.AnyAsync(c =>
                ((c.User1Id == remetenteId && c.User2Id == destinatarioId) ||
                 (c.User1Id == destinatarioId && c.User2Id == remetenteId))
                && c.Estado == "Aceite"
            );

            if (!conexaoExiste)
                return Forbid("Só pode enviar mensagens para conexões aceites.");

            var mensagem = new Mensagem
            {
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                Conteudo = conteudo,
                DataEnvio = DateTime.Now,
                IsFavorita = false,
                IsImportante = false
            };

            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { userId = destinatarioId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MsgFavorita(int id)
        {
            var userId = GetUserId();
            var mensagem = await _context.Mensagens.FindAsync(id);

            if (mensagem == null)
                return NotFound();
            
            if (mensagem.RemetenteId != userId && mensagem.DestinatarioId != userId)
                return NotFound();

            mensagem.IsFavorita = !mensagem.IsFavorita;
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var userId = GetUserId();

            var mensagem = await _context.Mensagens.FindAsync(id);

            if (mensagem == null)
                return NotFound();

            // Apenas o remetente pode eliminar a mensagem.
            if (mensagem.RemetenteId != userId)
                return NotFound();

            _context.Mensagens.Remove(mensagem);
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        protected string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}