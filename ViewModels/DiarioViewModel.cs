namespace Everkeep.ViewModels
{
    public class DiarioViewModel
    {
        // ViewModel utilizado para apresentar informações das entradas do diário.
        // Contém apenas os dados necessários para exibição nas views.
        public int Id { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool IsPrivado { get; set; }
        public bool IsImportante { get; set; }
    }
}
