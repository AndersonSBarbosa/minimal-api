using minimal_api.Dominio.Enums;

namespace minimal_api.Dominio.Dtos
{
    public class AdminstradorDto
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public Perfil? Perfil { get; set; } = default!;
    }
}
