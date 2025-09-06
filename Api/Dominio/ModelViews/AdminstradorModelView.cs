using minimal_api.Dominio.Enums;

namespace minimal_api.Dominio.ModelViews
{
    public record AdminstradorModelView
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public String Perfil { get; set; } = default!;
    }
}
