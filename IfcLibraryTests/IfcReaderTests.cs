using IfcLibrary.Domain;
using IfcLibrary.Ifc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace IfcLibraryTests
{
    [TestClass]
    public class IfcReaderTests
    {
        [TestMethod]
        public void CanPatchPropertyGroup()
        {
            var reader = new IfcAdapter();

            var automatedChanges = new AutomatedChanges();
            automatedChanges.UpdatePropertySetByValues.Add(
                new UpdatePropertySetByValue
                {
                    PropertySetName = "Basic set of properties",
                    PropertyName = "Length property",
                    NewValue = "57.",
                });
            automatedChanges.UpdatePropertySetByValues.Add(
                new UpdatePropertySetByValue
                {
                    PropertySetName = "Basic set of properties",
                    PropertyName = "Text property",
                    NewValue = "Change",
                });

            automatedChanges.AddPropertySetWithPropertyAndValues.Add(
                new AddPropertySetWithPropertyAndValue
                {
                    NewPropertySetName = "Basic set of properties",
                    NewPropertyName = "MyProp",
                    NewValue = "Abc",
                });

            reader.PatchFile(
                Path.Combine("TestData", "simple.ifc"),
                Path.Combine("TestData", "simple_patched.ifc"),
                automatedChanges);

            var contents = File.ReadAllLines(Path.Combine("TestData", "simple_patched.ifc"));
            
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCLENGTHMEASURE(57.)")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('Change')")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('Abc')")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("MyProp")));
        }

        [TestMethod]
        public void UpdatePropertySetByValue_PropertyDoesNotExistInPropertySet_CreatesThePropertyInThePropertySet()
        {
            var reader = new IfcAdapter();

            var automatedChanges = new AutomatedChanges();
            automatedChanges.UpdatePropertySetByValues.Add(
                new UpdatePropertySetByValue
                {
                    PropertySetName = "Basic set of properties",
                    PropertyName = "Modelador",
                    NewValue = "Nombre",
                });

            reader.PatchFile(
                Path.Combine("TestData", "simple.ifc"),
                Path.Combine("TestData", "simple_patched.ifc"),
                automatedChanges);

            var contents = File.ReadAllLines(Path.Combine("TestData", "simple_patched.ifc"));

            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('Nombre')")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("Modelador")));
        }

        [TestMethod]
        public void AddPropertySetWithPropertyAndValue_PropertySetDoesNotExist_CreatesThePropertyInThePropertySet()
        {
            var reader = new IfcAdapter();

            var automatedChanges = new AutomatedChanges();
            automatedChanges.AddPropertySetWithPropertyAndValues.Add(
                new AddPropertySetWithPropertyAndValue
                {
                    NewPropertySetName = "1239788974123",
                    NewPropertyName = "7640923409",
                    NewValue = "212349090123",
                });

            reader.PatchFile(
                Path.Combine("TestData", "simple.ifc"),
                Path.Combine("TestData", "simple_patched.ifc"),
                automatedChanges);

            var contents = File.ReadAllLines(Path.Combine("TestData", "simple_patched.ifc"));

            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("IFCTEXT('212349090123')")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("7640923409")));
            Assert.IsNotNull(contents.FirstOrDefault(x => x.Contains("1239788974123")));
        }

        [TestMethod]
        public void CanPatchWithBigFilePropertyGroup()
        {
            var reader = new IfcAdapter();

            var automatedChanges = new AutomatedChanges();
            automatedChanges.AddPropertySetWithPropertyAndValues.Add(
                new AddPropertySetWithPropertyAndValue
                {
                    NewPropertySetName = "ICAT-Identificacio",
                    NewPropertyName = "Abc",
                    NewValue = "Def",
                });

            reader.PatchFile(
                Path.Combine("TestData", "ICR_IFC4_PB_Mep.ifc"),
                Path.Combine("TestData", "ICR_IFC4_PB_Mep_patched.ifc"),
                automatedChanges);

            var contents = File.ReadAllLines(Path.Combine("TestData", "ICR_IFC4_PB_Mep_patched.ifc"));

            Assert.IsNotNull(contents);
        }
    }
}
