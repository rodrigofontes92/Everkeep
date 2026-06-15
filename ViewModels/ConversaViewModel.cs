namespace Everkeep.ViewModels
{
    public class ConversaViewModel
    {
        // ViewModel utilizado na listagem de conversas do utilizador.
        // Apresenta informações resumidas da última interação de cada chat.
        public string OutroUserId { get; set; }
        public string Nome { get; set; }
        public string UltimaMensagem { get; set; }
        public DateTime DataUltimaMensagem { get; set; }
    }
}
