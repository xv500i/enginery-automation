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
        public void CanReadAddProperties()
        {
            var reader = new ExcelReader();
            var automatedChanges = reader.GetAutomatedChanges(Path.Combine("TestData", "CasoUsoEntrada.xlsx"));
            var singleUpdatePropertyByValue = automatedChanges.UpdatePropertySetByValues.First();
            singleUpdatePropertyByValue.PropertyName.Should().Be("Name");
            singleUpdatePropertyByValue.PropertySetName.Should().Be("Name");
            singleUpdatePropertyByValue.NewValue.Should().Be("Name");
        }
    }
}
