using IfcLibrary.Ifc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace IfcLibraryTests
{
    [TestClass]
    public class IfcReaderTests
    {
        [TestMethod]
        public void CanReadRaw()
        {
            var reader = new IfcReader();
            var lines = reader.Load(Path.Combine("TestData", "simple.ifc"));
            Assert.IsNotNull(lines);
        }
    }
}
