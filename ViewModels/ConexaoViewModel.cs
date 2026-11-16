namespace Everkeep.ViewModels
{
    public class ConexaoViewModel
    {
        // ViewModel utilizado para apresentar informações das conexões do utilizador.
        // Reúne os dados necessários para listagem e gestão de convites.
        public int Id { get; set; }
        public string? Nome { get; set; }
        public required string Estado { get; set; }
        public required string OutroUserId { get; set; }
        public bool FoiRecebido { get; set; }
    }
}
