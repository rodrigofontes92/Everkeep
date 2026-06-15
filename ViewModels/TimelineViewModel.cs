namespace Everkeep.ViewModels
{
    public class TimelineViewModel
    {
        public string Tipo { get; set; } // "Mensagem" ou "Diario"
        public string Conteudo { get; set; }
        public DateTime Data { get; set; }

        public string OutroUserNome { get; set; } // opcional
    }
}
