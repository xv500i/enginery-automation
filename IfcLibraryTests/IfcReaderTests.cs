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
                        NewValue = "57.",
                    },
                    new IFCUpdate
                    {
                        PropertySetName = "Basic set of properties",
                        PropertyName = "Text property",
                        NewValue = "Change",
                    }
                },
                new List<IFCAdd>()
                {
                    new IFCAdd
                    {
                        PropertySetName = "Basic set of properties",
                        NewPropertyName = "MyProp",
                        NewValue = "Abc",
                    }
                });
            var contents = File.ReadAllLines(Path.Combine("TestData", "simple_patched.ifc"));
            
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCLENGTHMEASURE(57.)")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('Change')")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('Abc')")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("MyProp")));
        }

        [TestMethod]
        public void CanPatchWithBigFilePropertyGroup()
        {
            var reader = new IfcReader();
            reader.PatchFile(
                Path.Combine("TestData", "ICR_IFC4_PB_Mep.ifc"),
                Path.Combine("TestData", "ICR_IFC4_PB_Mep_patched.ifc"),
                new List<IFCUpdate>()
                {
                },
                new List<IFCAdd>()
                {
                    new IFCAdd
                    {
                        PropertySetName = "ICAT-Identificacio",
                        NewPropertyName = "Aleix",
                        NewValue = "Cuello",
                    }
                });
            var contents = File.ReadAllLines(Path.Combine("TestData", "ICR_IFC4_PB_Mep_patched.ifc"));

            Assert.IsNotNull(contents);
        }
    }
}
