namespace minimal_api.Dominio.ModelViews
{
    public class AdmLogado
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public String Perfil { get; set; } = default!;
        public string Token { get; set; } = default!;
        public bool Caminho { get; set; }
    }
}
