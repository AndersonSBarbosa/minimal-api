using minimal_api.Dominio.Entidades;

namespace minimal_api.Dominio.Infraestrutura.Interface
{
    public interface IVeiculosServico
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null, string? modelo = null, int? ano = null );
        Veiculo? BuscaPorId(int id);
        void Incluir(Veiculo veiculo);
        void Atualizar(Veiculo veiculo);
        void Apagar(Veiculo veiculo);
        void ApagarId(int id);
    }
}
