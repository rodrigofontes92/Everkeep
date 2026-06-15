using System.ComponentModel.DataAnnotations;

namespace Everkeep.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string CurrentUserId { get; set; }
        public string Email { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Bio { get; set; }

        public string EstadoEmocional { get; set; }

        public string EstadoConexao { get; set; }
        public bool PodeVerPerfil { get; set; }

        public List<DiarioViewModel> DiariosPublicos { get; set; }
    }
}
