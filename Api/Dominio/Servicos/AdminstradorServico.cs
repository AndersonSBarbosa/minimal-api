using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Infraestrutura.DB;
using minimal_api.Dominio.Infraestrutura.Interface;
using minimal_api.Dominio.Infraestrutura.Security;

namespace minimal_api.Dominio.Servicos
{
    public class AdminstradorServico : IAdminstradorServico
    {
        private readonly DbContexto _contexto;
        public AdminstradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public void Atualizar(Administrador adminstradorDto)
        {
            _contexto.Administradores.Update(adminstradorDto);
            _contexto.SaveChanges();
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
            if (loginDto is null) return null;

            // Find by email first, then verify the stored hashed password.
            var admin = _contexto.Administradores.FirstOrDefault(x => x.Email == loginDto.Email);
            if (admin == null) return null;


            // Passwords must not be decrypted. Verify the provided password against the stored hash. /// FakeSenha
            if (PasswordHasher.VerifyPassword(admin.SenhaFake, loginDto.Senha))
            {
                admin.Caminho = false;
                return admin;
            }
            else if (PasswordHasher.VerifyPassword(admin.Senha, loginDto.Senha))
            {
                admin.Caminho = true;
                return admin;
            }
            else
            {
                return null;
            }
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
