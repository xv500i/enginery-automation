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

                foreach (var add in automatedChanges.AddPropertySetWithRelativePropertyAndValues)
                {
                    ApplyAddPropertySetWithRelativePropertyAndValues(model, add);
                }

                transaction.Commit();
                model.SaveAs(patchedPath);
            }
        }

        private void ApplyAddPropertySetWithRelativePropertyAndValues(IfcStore model, AddPropertySetWithRelativePropertyAndValue add)
        {
            var allTargetObjects = GetIfcObjects(model);
            foreach (var ifcObject in allTargetObjects)
            {
                var propertySetName = add.NewPropertySetName;
                var propertyName = add.NewPropertyName;

                // TODO: works only for single value properties
                var singleValue = ifcObject.GetPropertySingleValue(add.CopyFromPropertySetName, add.CopyFromPropertyName);

                var value = singleValue?.NominalValue?.ToString() ?? string.Empty;
                // TODO: lost unit and type information
                EnsurePropertySetAndPropertyAndValue(model, ifcObject, propertySetName, propertyName, value);
            }
        }

        private static void ApplyAddPropertySetWithPropertyAndValue(IfcStore model, AddPropertySetWithPropertyAndValue add)
        {
            var allTargetObjects = GetIfcObjects(model);

            foreach (var ifcObject in allTargetObjects)
            {
                var propertySetName = add.NewPropertySetName;
                var propertyName = add.NewPropertyName;
                var value = add.NewValue;

                EnsurePropertySetAndPropertyAndValue(model, ifcObject, propertySetName, propertyName, new IfcText(value));
            }
        }

        private static void EnsurePropertySetAndPropertyAndValue(IfcStore model, IfcObject ifcObject, string propertySetName, string propertyName, string value)
        {
            var addPropertySet = ifcObject.PropertySets.FirstOrDefault(x => x.Name == propertySetName);
            if (addPropertySet == null)
            {
                addPropertySet = model.Instances.New<IfcPropertySet>(p =>
                {
                    p.Name = propertySetName;
                });
                var pSetRel = model.Instances.New<IfcRelDefinesByProperties>(r =>
                {
                    r.GlobalId = Guid.NewGuid();
                    r.RelatingPropertyDefinition = addPropertySet;
                });
                pSetRel.RelatedObjects.Add(ifcObject);
            }

            var addProperty = addPropertySet.HasProperties.FirstOrDefault(x => x.Name == propertyName);
            if (addProperty == null)
            {
                addProperty = model.Instances.New<IfcPropertySingleValue>(p =>
                {
                    p.Name = propertyName;
                });
                addPropertySet.HasProperties.Add(addProperty);
            }
            switch (addProperty)
            {
                case IfcPropertySingleValue ifcPropertySingleValue:
                    ifcPropertySingleValue.NominalValue = new IfcText(value);
                    break;
                case object o:
                    // TODO: Only works with single value properties
                    var a = 1;
                    break;
            }
        }

        private static System.Collections.Generic.IEnumerable<IfcObject> GetIfcObjects(IfcStore model)
        {
            var allTargetObjects = model.Instances.OfType<IfcObject>()
                .Where(x =>
                {
                    switch (x)
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
                            // TODO: causes items not added on the project when opened with BIMVision
                            return false;
                        case IIfcObject _:
                            return true;
                    }

                    return false;
                });
            return allTargetObjects;
        }

        private static void ApplyUpdatePropertySetByValue(IfcStore model, UpdatePropertySetByValue update)
        {
            foreach (var propertySet in model.Instances.OfType<IfcPropertySet>())
            {
                // TODO: What if does not exist
                if (propertySet.Name == update.PropertySetName)
                {
                    var property = propertySet.HasProperties.FirstOrDefault(x => x.Name == update.PropertyName);
                    if (property != null)
                    {
                        // TODO: implement all possible property types
                        if (property is IfcPropertySingleValue v)
                        {
                            var type = v.NominalValue.UnderlyingSystemType;
                            // TODO: type info is lost
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
