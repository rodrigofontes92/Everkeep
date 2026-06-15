namespace Everkeep.ViewModels
{
    public class DashboardViewModel
    {
        // ViewModel utilizado no dashboard inicial do sistema.
        // Reúne informações rápidas e recentes do utilizador autenticado.
        public string Nome { get; set; }
        public List<string> UltimasInteracoes { get; set; }

        // Verifica se a lista possui interações
        // Retorna true se a lista existir e se possuir elementos
        public bool TemInteracoes => UltimasInteracoes != null && UltimasInteracoes.Any();
    }
}
