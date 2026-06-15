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
    public class DiarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiarioController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            // Filtra apenas diários do utilizador autenticado
            // ordena do mais recente ao mais antigo
            var diarios = await _context.Diarios
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DataCriacao)
                .ToListAsync();

            // cada entidade diarios é transformada em um DiarioViewModel
            var model = diarios.Select(d => new DiarioViewModel
            {
                Id = d.Id,
                Conteudo = d.Conteudo,
                DataCriacao = d.DataCriacao,
                IsPrivado = d.IsPrivado,
                IsImportante = d.IsImportante
            }).ToList();

            return View(model);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(DiarioViewModel model)
        {
            var userId = GetUserId();

            // Impede criação de entradas vazias.
            if (string.IsNullOrEmpty(model.Conteudo))
                return BadRequest();

            // Cria a nova entrada do diário.
            var entrada = new Diario
            {
                UserId = userId,
                Conteudo = model.Conteudo,
                DataCriacao = DateTime.Now,
                IsPrivado = true,
                IsImportante = false
            };

            _context.Diarios.Add(entrada);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var userId = GetUserId();

            // Procura o diário via PK
            var entrada = await _context.Diarios.FindAsync(id);

            // Garante que apenas o dono pode editar.
            if (entrada == null || entrada.UserId != userId)
                return NotFound();

            // Cria o ViewModel utilizado pela View.
            var model = new DiarioViewModel
            {
                Conteudo = entrada.Conteudo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, DiarioViewModel model)
        {
            var userId = GetUserId();
            var entrada = await _context.Diarios.FindAsync(id);

            if (entrada == null || entrada.UserId != userId)
                return NotFound();

            // Impede conteúdo vazio.
            if (string.IsNullOrEmpty(model.Conteudo))
                return BadRequest();

            entrada.Conteudo = model.Conteudo;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPrivado(int id)
        {
            var userId = GetUserId();
            var entrada = await _context.Diarios.FindAsync(id);

            // Garante que apenas o dono pode alterar a privacidade.
            if (entrada == null || entrada.UserId != userId)
                return NotFound();

            // Inversão do valor booleano
            // Se está privado, passa a público e vice-versa
            entrada.IsPrivado = !entrada.IsPrivado;

            await _context.SaveChangesAsync();

            // Retorna para a página anterior, já com o status atualizado
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarImportante(int id)
        {
            var userId = GetUserId();
            var entrada = await _context.Diarios.FindAsync(id);

            if (entrada == null || entrada.UserId != userId)
                return NotFound();

            entrada.IsImportante = !entrada.IsImportante;
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var userId = GetUserId();
            var entrada = await _context.Diarios.FindAsync(id);

            // Garante que apenas o dono pode eliminar.
            if (entrada == null || entrada.UserId != userId)
                return NotFound();

            _context.Diarios.Remove(entrada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}
