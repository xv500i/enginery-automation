using IfcLibrary;
using IfcLibrary.Ifc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace IfcLibraryTests
{
    [TestClass]
    public class IfcReaderTests
    {
        [TestMethod]
        public void CanReadPropertyGroup()
        {
            var reader = new IfcReader();
            var ids = reader.GetPropertySetIds(Path.Combine("TestData", "simple.ifc"));
            var id = ids.Single();
            Assert.AreEqual("2u_olyjv13oRt0GvSVSxHS", id);
        }

        [TestMethod]
        public void CanReadAllPropertySets()
        {
            var reader = new IfcReader();
            var ids = reader.GetPropertySetIds(Path.Combine("TestData", "file.ifc"));
            CollectionAssert.AreEquivalent(IFCFileIds.PropertySetIds, ids);
        }

        [TestMethod]
        public void CanPatchPropertyGroup()
        {
            var reader = new IfcReader();
            reader.PatchFile(
                Path.Combine("TestData", "simple.ifc"),
                Path.Combine("TestData", "simple_patched.ifc"),
                new List<IFCUpdate>()
                {
                    new IFCUpdate
                    {
                        PropertySetName = "Basic set of properties",
                        PropertyName = "Length property",
                        NewValue = "57."
                    },
                    new IFCUpdate
                    {
                        PropertySetName = "Basic set of properties",
                        PropertyName = "Text property",
                        NewValue = "Change"
                    }
                });
            var contents = File.ReadAllLines(Path.Combine("TestData", "simple_patched.ifc"));
            
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCLENGTHMEASURE(57.)")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('Change')")));
        }
    }
}
