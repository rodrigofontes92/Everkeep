using System.ComponentModel.DataAnnotations;

namespace Everkeep.Models
{
    // Representa a conexão entre dois utilizadores da plataforma.
    // Esta entidade controla o estado da relação (pendente, aceite ou rejeitado).
    public class Conexao
    {
        public int Id { get; set; }

        [Required]
        public string User1Id { get; set; }

        [Required]
        public string User2Id { get; set; }

        [Required]
        public string Estado {  get; set; }
    }
}
