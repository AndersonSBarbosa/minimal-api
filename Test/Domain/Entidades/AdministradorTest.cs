using minimal_api.Dominio.Entidades;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Test.Domain.Entidades
{

    [TestClass]
    public class AdministradorTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange
            var adm = new Administrador();

            // Act
            adm.Id = 1;
            adm.Email = "teste@teste.com.br";
            adm.Senha = "senha123";
            adm.Perfil = "Admin";

            // Assert

            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("teste@teste.com.br", adm.Email);
            Assert.AreEqual("1234566", adm.Senha);
            Assert.AreEqual("Adm", adm.Perfil);

        }
    }
}
