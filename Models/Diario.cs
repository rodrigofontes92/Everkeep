using System;
using System.ComponentModel.DataAnnotations;

namespace Everkeep.Models
{
    // Representa uma entrada pessoal do diário do utilizador.
    // Permite guardar pensamentos e memórias com opção de privacidade e destaque.
    public class Diario
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Conteudo { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool IsPrivado { get; set; }
        public bool IsImportante { get; set; }


    }
}
