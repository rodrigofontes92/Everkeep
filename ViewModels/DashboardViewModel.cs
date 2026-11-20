namespace Everkeep.ViewModels
{
    public class DashboardViewModel
    {
        // ViewModel utilizado no dashboard inicial do sistema.
        // Reúne informações rápidas e recentes do utilizador autenticado.
        public required string Nome { get; set; }
        public List<string> UltimasInteracoes { get; set; } = [];

        // Verifica se a lista possui interações
        // A lista inicia vazia e permanece não nula.
        public bool TemInteracoes => UltimasInteracoes.Count > 0;
    }
}
