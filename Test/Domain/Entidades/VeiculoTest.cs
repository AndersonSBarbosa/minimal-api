using minimal_api.Dominio.Entidades;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange
            var car = new Veiculo();

            // Act
            car.Id = 1;
            car.Nome = "Preto";
            car.Marca = "SANDERO";
            car.Modelo = "RENAULT";
            car.Ano = 2020;

            // Assert

            Assert.AreEqual(1, car.Id);
            Assert.AreEqual("Nome do Carro", car.Nome);
            Assert.AreEqual("Carro", car.Marca);
            Assert.AreEqual("Fiesta", car.Modelo);
            Assert.AreEqual(2015, car.Ano);

        }
    }
}
