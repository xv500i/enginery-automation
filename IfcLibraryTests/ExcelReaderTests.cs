using FluentAssertions;
using IfcLibrary.Excel;
using IfcLibraryTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace IfcLibraryTests
{
    [TestClass]
    public class ExcelReaderTests
    {
        [TestMethod]
        public void CanReadFirstEntity()
        {
            var reader = new ExcelReader();
            var manipulations = reader.GetChanges(TestFileNames.InputExcel);

            var firstEntityChange = manipulations.EntityChanges.First(x => x.Identifier == "1nsLFSbor8AQdQO784RL3r");

            firstEntityChange
                .PropertyChanges
                .Should()
                .HaveCount(16);

            firstEntityChange
                .PropertyChanges
                .First(x => x.PropertySetName == "00.Quantities" && x.PropertyName == "00.01.Length")
                .Value
                .Should().Be("-");

            firstEntityChange
                .PropertyChanges
                .First(x => x.PropertySetName == "00.Quantities" && x.PropertyName == "00.02.Width")
                .Value
                .Should().Be("TK_ETQ_Ancho");

            firstEntityChange
                .PropertyChanges
                .First(x => x.PropertySetName == "00.Quantities" && x.PropertyName == "00.03.Height")
                .Value
                .Should().Be("TK_ETQ_Alto");

            firstEntityChange
                .PropertyChanges
                .First(x => x.PropertySetName == "02.Phasing" && x.PropertyName == "02.02.PhaseDemolished")
                .Value
                .Should().Be("-");
        }

        [TestMethod]
        public void CanReadCleanupProperties()
        {
            var reader = new ExcelReader();
            var manipulations = reader.GetChanges(TestFileNames.InputExcel);

            var propertySetCleanups = manipulations.PropertySetCleanups;

            propertySetCleanups.Should().HaveCount(3);

            var quantities = propertySetCleanups.Single(x => x.PropertySetName == "00.Quantities");
            var identification = propertySetCleanups.Single(x => x.PropertySetName == "01.Identification");
            var phasing = propertySetCleanups.Single(x => x.PropertySetName == "02.Phasing");
        }
    }
}
