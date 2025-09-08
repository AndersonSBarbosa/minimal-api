using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Infraestrutura.DB;
using minimal_api.Dominio.Servicos;
using System.Reflection;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdminstradorServicoTeste
    {

        private DbContexto CriarContextoTeste()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestandoSalvarAdminstrador()
        {
            // Arrange
            var context = CriarContextoTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com.br";
            adm.Senha = "senha123";
            adm.Perfil = "Admin";
            
            var adminstradorServico = new AdminstradorServico(context);

            // Act
            adminstradorServico.Incluir(adm);

            // Assert

            Assert.AreEqual(1, adminstradorServico.Todos(1).Count());
            Assert.AreEqual("teste@teste.com.br", adm.Email);
            Assert.AreEqual("1234566", adm.Senha);
            Assert.AreEqual("Adm", adm.Perfil);

        }

        [TestMethod]
        public void TestandoBuscaPorID()
        {
            // Arrange
            var context = CriarContextoTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com.br";
            adm.Senha = "senha123";
            adm.Perfil = "Admin";

            var adminstradorServico = new AdminstradorServico(context);

            // Act
            adminstradorServico.Incluir(adm);
            var admin = adminstradorServico.BuscaPorId(adm.Id);

            // Assert

            Assert.AreEqual(1, admin.Id);

        }
    }
}
