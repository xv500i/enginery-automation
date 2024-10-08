﻿using FluentAssertions;
using IfcLibrary.Domain;
using IfcLibrary.Ifc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Kernel;

namespace IfcLibraryTests
{
    [TestClass]
    public class IfcReaderTests
    {
        [TestMethod]
        public void IfcReaderTest()
        {
            var adapter = new IfcAdapter();

            var outputFile = "IFC - edifici-patched.ifc";
            var entityIdentifier = "id: 621429";

            adapter.PatchFile("IFC - edifici.ifc", outputFile, new IfcManipulations
            {
                EntityChanges = new List<EntityChange>
                {
                    new EntityChange
                    {
                        Identifier = entityIdentifier,
                        PropertyChanges = new List<PropertyChange>
                        {
                            new PropertyChange
                            {
                                PropertyName = "00.01.Length",
                                PropertySetName = "00.Quantities",
                                Value = "TK_ETQ_Ancho",
                            }
                        }
                    }
                },
                PropertySetCleanups = null,
            });

            using (var model = IfcStore.Open(outputFile, null))
            {
                var entity = model.Instances.OfType<IfcObject>().Where(x => x.Name == entityIdentifier).First();
                entity.GetPropertySingleValue("00.Quantities", "00.01.Length")
                    .NominalValue
                    .Value
                    .ToString()
                    .Should().Be("0.35");
            }
        }
    }
}
