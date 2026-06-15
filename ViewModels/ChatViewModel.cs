using Everkeep.Models;

namespace Everkeep.ViewModels
{
    // ViewModel utilizado na página de conversa entre dois utilizadores.
    // Reúne os dados do chat e a nova mensagem a ser enviada.
    public class ChatViewModel
    {
        public string OutroUserId { get; set; }
        public string NomeOutroUser { get; set; }

        public List<Mensagem> Mensagens { get; set; }

        public string NovaMensagem { get; set; }
    }
}
