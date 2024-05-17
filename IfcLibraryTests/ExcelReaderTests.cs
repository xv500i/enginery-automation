using IfcLibrary.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
    }
}
