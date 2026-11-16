using System.ComponentModel.DataAnnotations;

namespace Everkeep.ViewModels
{
    public class ProfileViewModel
    {
        // These identifiers are present on the profile page, but absent from the edit form.
        public string? UserId { get; set; }
        public string? CurrentUserId { get; set; }
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public string EstadoEmocional { get; set; } = string.Empty;

        public string EstadoConexao { get; set; } = string.Empty;
        public bool PodeVerPerfil { get; set; }

        public List<DiarioViewModel> DiariosPublicos { get; set; } = [];
    }
}
