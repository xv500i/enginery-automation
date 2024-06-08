using IfcLibrary.Domain;
using System;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;

namespace IfcLibrary.Ifc
{
    public class IfcAdapter : IIfcAdapter
    {
        public void PatchFile(string originalPath, string patchedPath, AutomatedChanges automatedChanges)
        {
            using (var model = IfcStore.Open(originalPath, null))
            {
                var transaction = model.BeginTransaction();
                foreach(var update in  automatedChanges.UpdatePropertySetByValues)
                {
                    ApplyUpdatePropertySetByValue(model, update);
                }

                foreach (var add in automatedChanges.AddPropertySetWithPropertyAndValues)
                {
                    ApplyAddPropertySetWithPropertyAndValue(model, add);
                }
                transaction.Commit();
                model.SaveAs(patchedPath);
            }
        }

        private static void ApplyAddPropertySetWithPropertyAndValue(IfcStore model, AddPropertySetWithPropertyAndValue add)
        {
            var allTargetObjects = model.Instances.OfType<IfcObject>()
                .Where(x =>
                {
                    switch(x)
                    {
                        //case IIfcWall _:
                        //case IIfcCableCarrierFitting _:
                        //case IIfcCableCarrierSegment _:
                        //case IIfcCableSegment _:
                        //case IIfcProtectiveDevice _:
                        //case IIfcElectricDistributionBoard _:
                        //case IIfcEnergyConversionDevice _:
                        //case IIfcSwitchingDevice _:
                        //    return true;
                        case IIfcDistributionPort _:
                            // TODO: Da problemas
                            return false;
                        case IIfcObject _:
                            return true;
                    }
                    
                    return false;
                });

            foreach (var ifcObject in allTargetObjects)
            {
                var addPropertySet = ifcObject.PropertySets.FirstOrDefault(x => x.Name == add.NewPropertySetName);
                if (addPropertySet == null)
                {
                    addPropertySet = model.Instances.New<IfcPropertySet>(p =>
                    {
                        p.Name = add.NewPropertySetName;
                    });
                    var pSetRel = model.Instances.New<IfcRelDefinesByProperties>(r =>
                    {
                        r.GlobalId = Guid.NewGuid();
                        r.RelatingPropertyDefinition = addPropertySet;
                    });
                    pSetRel.RelatedObjects.Add(ifcObject);
                }

                var addProperty = addPropertySet.HasProperties.FirstOrDefault(x => x.Name == add.NewPropertyName);
                if (addProperty == null)
                {
                    addProperty = model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = add.NewPropertyName;
                    });
                    addPropertySet.HasProperties.Add(addProperty);
                }
                switch (addProperty)
                {
                    case IfcPropertySingleValue ifcPropertySingleValue:
                        ifcPropertySingleValue.NominalValue = new IfcText(add.NewValue);
                        break;
                    case object o:
                        var a = 1;
                        break;
                }
            }
        }

        private static void ApplyUpdatePropertySetByValue(IfcStore model, UpdatePropertySetByValue update)
        {
            foreach (var propertySet in model.Instances.OfType<IfcPropertySet>())
            {
                // What if does not exist
                if (propertySet.Name == update.PropertySetName)
                {
                    var property = propertySet.HasProperties.FirstOrDefault(x => x.Name == update.PropertyName);
                    if (property != null)
                    {
                        // TODO: implement all possible property types
                        if (property is IfcPropertySingleValue v)
                        {
                            var type = v.NominalValue.UnderlyingSystemType;
                            // TODO: respect type
                            if (type == typeof(string))
                            {
                                v.NominalValue = new IfcText(update.NewValue);
                            }
                            else
                            {
                                v.NominalValue = new IfcLengthMeasure(update.NewValue);
                            }

                        }
                    }
                    else
                    {
                        propertySet.HasProperties.Add(
                            model.Instances.New<IfcPropertySingleValue>(p =>
                            {
                                p.Name = update.PropertyName;
                                p.NominalValue = new IfcText(update.NewValue);
                            }));
                    }
                }
            }
        }
    }
}
