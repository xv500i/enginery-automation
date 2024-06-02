using FluentAssertions;
using IfcLibrary.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace IfcLibraryTests
{
    [TestClass]
    public class ExcelReaderTests
    {
        [TestMethod]
        public void CanReadExcel()
        {
            var reader = new ExcelReader();
            var cells = reader.GetCells(Path.Combine("TestData", "Libro1.xlsx"));
            Assert.IsNotNull(cells);
        }

        [TestMethod]
        public void CanReadUpdatePropertySetByValues()
        {
            var reader = new ExcelReader();
            var automatedChanges = reader.GetAutomatedChanges(Path.Combine("TestData", "CasoUsoEntrada.xlsx"));
            var singleUpdatePropertyByValue = automatedChanges.UpdatePropertySetByValues.Single();
            singleUpdatePropertyByValue.PropertyName.Should().Be("Modelador ");
            singleUpdatePropertyByValue.PropertySetName.Should().Be("Pset_TeKton3D_Etiquetas");
            singleUpdatePropertyByValue.NewValue.Should().Be("Aleix");
        }

        [TestMethod]
        public void CanReadAddPropertySetWithPropertyAndValues()
        {
            var reader = new ExcelReader();
            var automatedChanges = reader.GetAutomatedChanges(Path.Combine("TestData", "CasoUsoEntrada.xlsx"));
            var singleUpdatePropertyByValue = automatedChanges.AddPropertySetWithPropertyAndValues.Single();
            singleUpdatePropertyByValue.NewPropertyName.Should().Be("00.03.Height");
            singleUpdatePropertyByValue.NewPropertySetName.Should().Be("00.Quantities");
            singleUpdatePropertyByValue.NewValue.Should().Be("2");
        }

        [TestMethod]
        public void CanReadAddPropertySetWithRelativePropertyAndValues()
        {
            var reader = new ExcelReader();
            var automatedChanges = reader.GetAutomatedChanges(Path.Combine("TestData", "CasoUsoEntrada.xlsx"));
            automatedChanges.AddPropertySetWithRelativePropertyAndValues
                .Should().HaveCount(3);

            var firstAdd = automatedChanges.AddPropertySetWithRelativePropertyAndValues[0];
            firstAdd.NewPropertyName.Should().Be("00.01.Length");
            firstAdd.NewPropertySetName.Should().Be("00.Quantities");
            firstAdd.CopyFromPropertyName.Should().Be("ICAT-Longitud");
            firstAdd.CopyFromPropertySetName.Should().Be("ICAT-Geometria");

            var secondAdd = automatedChanges.AddPropertySetWithRelativePropertyAndValues[1];
            secondAdd.NewPropertyName.Should().Be("00.02.Width");
            secondAdd.NewPropertySetName.Should().Be("00.Quantities");
            secondAdd.CopyFromPropertyName.Should().Be("ICAT-Ample");
            secondAdd.CopyFromPropertySetName.Should().Be("ICAT-Geometria");

            var thirdAdd = automatedChanges.AddPropertySetWithRelativePropertyAndValues[2];
            thirdAdd.NewPropertyName.Should().Be("01.01.TypeName");
            thirdAdd.NewPropertySetName.Should().Be("01.Identification");
            thirdAdd.CopyFromPropertyName.Should().Be("ICAT_02-DescripcioGuBIMclass");
            thirdAdd.CopyFromPropertySetName.Should().Be("ICAT-Identificacio");
        }
    }
}
