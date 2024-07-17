using FluentAssertions;
using IfcLibrary.Excel;
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
            var entityChangeInfos = reader.GetChanges("Edit.xlsx");

            var firstEntityChange = entityChangeInfos.First(x => x.Identifier == "id: 621429");
            firstEntityChange
                .Entity
                .Should().Be("IfcAirTerminal");

            firstEntityChange
                .PropertyChangeInfos
                .Should()
                .HaveCount(16);

            firstEntityChange
                .PropertyChangeInfos
                .First(x => x.PropertySetName == "00.Quantities" && x.PropertyName == "00.02.Width")
                .Value
                .Should().Be("TK_ETQ_Alto");
            firstEntityChange
                .PropertyChangeInfos
                .First(x => x.PropertySetName == "00.Quantities" && x.PropertyName == "00.03.Height")
                .Value
                .Should().Be("-");
        }
    }
}
