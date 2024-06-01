using Xbim.Ifc;
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
                    foreach(var propertySet in model.Instances.OfType<IfcPropertySet>())
                    {
                        // What if does not exist
                        if(propertySet.Name == update.PropertySetName)
                        {
                            var property = propertySet.HasProperties.FirstOrDefault(x => x.Name == update.PropertyName);
                            if (property != null) 
                            {
                                // TODO: implement all possible property types
                                if (property is Xbim.Ifc4.PropertyResource.IfcPropertySingleValue v)
                                {
                                    var type = v.NominalValue.UnderlyingSystemType;
                                    // TODO: respect type
                                    if (type == typeof(string))
                                    {
                                        v.NominalValue = new IfcText(update.NewValue);
                                    } else
                                    {
                                        v.NominalValue = new IfcLengthMeasure(update.NewValue);
                                    }

                                }
                            }
                        }
                    }
                }

                foreach (var add in automatedChanges.AddPropertySetWithPropertyAndValues)
                {
                    foreach (var propertySet in model.Instances.OfType<IfcPropertySet>())
                    {
                        // What if does not exist
                        if (propertySet.Name == add.PropertySetName)
                        {
                            propertySet.HasProperties.Add(model.Instances.New<IfcPropertySingleValue>(p =>
                            {
                                p.Name = add.NewPropertyName;
                                // TODO: support types
                                p.NominalValue = new IfcText(add.NewValue);
                            }));
                        }
                    }
                }
                transaction.Commit();
                model.SaveAs(patchedPath);
            }
        }
    }
}
