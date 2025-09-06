using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Infraestrutura.DB;
using minimal_api.Dominio.Infraestrutura.Interface;

namespace minimal_api.Dominio.Servicos
{
    public class AdminstradorServico : IAdminstradorServico
    {
        private readonly DbContexto _contexto;
        public AdminstradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }


        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
        }

        public Administrador? Incluir(Administrador adminstrador)
        {
            _contexto.Administradores.Add(adminstrador);
            _contexto.SaveChanges();
            return adminstrador;
        }

        public Administrador? Login(LoginDTO loginDto)
        {
           return _contexto.Administradores.Where(x => x.Email == loginDto.Email && x.Senha == loginDto.Senha).FirstOrDefault() ;            
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _contexto.Administradores.AsQueryable();

            //Paginaçao
            int tamanhoPagina = 10;
            if (pagina != null)
                query = query.Skip((int)((pagina - 1) * tamanhoPagina)).Take(tamanhoPagina);

            return query.ToList();
        }
    }
}
