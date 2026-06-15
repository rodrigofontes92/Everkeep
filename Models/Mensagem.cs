using System;
using System.ComponentModel.DataAnnotations;

namespace Everkeep.Models
{
    // Representa uma mensagem trocada entre dois utilizadores conectados.
    // Permite armazenar o conteúdo da conversa e classificações emocionais.
    public class Mensagem
    {
        public int Id { get; set; }

        [Required]
        public string RemetenteId { get; set; }

        [Required]
        public string DestinatarioId { get; set; }

        [Required]
        public string Conteudo {  get; set; }

        public DateTime DataEnvio { get; set; }
        public bool IsFavorita { get; set; }

        //não será mais utilizada
        public bool IsImportante { get; set; }

    }
}
