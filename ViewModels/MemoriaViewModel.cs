namespace Everkeep.ViewModels
{
    public class MemoriaViewModel
    {
        // ViewModel utilizado no painel de memórias do sistema.
        // Permite apresentar mensagens e entradas do diário numa estrutura única.
        public string Tipo { get; set; } // "Mensagem" ou "Diario"
        public string Conteudo { get; set; }
        public DateTime Data { get; set; }

        // Nome do outro utilizador associado à memória.
        // Utilizado apenas quando a memória é uma mensagem.
        public string OutroUserNome { get; set; }
    }
}
