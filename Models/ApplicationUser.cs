
using Microsoft.AspNetCore.Identity;

namespace Everkeep.Models
{
    /*
     * Classe de customização do utilizador
     * O sistema deixa de tratar o utilizador como apenas login técnico
     * Passa a conter a identidade emocional (EstadoEmocional) e personalização (Nome + Bio)
     */
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; }
        public string Bio { get; set; }
        public string EstadoEmocional { get; set; }
    }
}
