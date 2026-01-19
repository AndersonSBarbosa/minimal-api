using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Dominio.Infraestrutura.Interface
{
    public interface IAdminstradorServico
    {
        Administrador? Login(LoginDTO loginDto);
        Administrador? Incluir(Administrador adminstradorDto);
        List<Administrador> Todos(int? pagina);
        Administrador? BuscaPorId(int id);
        void Atualizar(Administrador adminstradorDto);
    }
}
