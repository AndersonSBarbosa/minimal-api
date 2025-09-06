using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Infraestrutura.DB;
using minimal_api.Dominio.Infraestrutura.Interface;

namespace minimal_api.Dominio.Servicos
{
    public class VeiculosServico : IVeiculosServico
    {
        private readonly DbContexto _contexto;
        public VeiculosServico(DbContexto contexto)
        {
            _contexto = contexto;
        }
        public void Apagar(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public void ApagarId(int id)
        {
            throw new NotImplementedException();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            //return _contexto.Veiculos.Find(id) ?? throw new KeyNotFoundException("Veículo não encontrado.");
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null, string? modelo = null, int? ano = null)
        {
            var query = _contexto.Veiculos.AsQueryable(); 
            
            if (!string.IsNullOrEmpty(nome))
                query = query.Where(v => v.Nome.ToLower().Contains(nome.ToLower()));

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(v => v.Marca.ToLower().Contains(marca.ToLower()));

            if (!string.IsNullOrEmpty(modelo))
                query = query.Where(v => v.Modelo.ToLower().Contains(modelo.ToLower()));

            //Paginaçao
            int tamanhoPagina = 10;
            if(pagina != null)
                query = query.Skip((int)((pagina - 1) * tamanhoPagina)).Take(tamanhoPagina);

            return query.ToList();
        }
    }
}
