using IfcLibrary.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;

namespace IfcLibrary.Ifc
{
    public class IfcAdapter : IIfcAdapter
    {
        private const string NotDefinedValue = "-";

        public void PatchFile(string originalPath, string patchedPath, List<EntityChangeInfo> entityChangeInfos)
        {
            using (var model = IfcStore.Open(originalPath, null))
            {
                var transaction = model.BeginTransaction();

                foreach (var entityChangeInfo in entityChangeInfos) 
                {
                    ApplyEntityChangeInfo(model, entityChangeInfo);
                }
                
                transaction.Commit();
                model.SaveAs(patchedPath);
            }
        }

        private void ApplyEntityChangeInfo(IfcStore model, EntityChangeInfo entityChangeInfo)
        {
            var entity = model.Instances.OfType<IfcObject>().Where(x => x.Name == entityChangeInfo.Identifier).FirstOrDefault();
            if (entity == null)
            {
                return;
            }

            foreach(var propertyChangeInfo in entityChangeInfo.PropertyChangeInfos)
            {
                var value = string.Empty;
                if (string.IsNullOrWhiteSpace(propertyChangeInfo.Value) || value == NotDefinedValue)
                {
                    value = NotDefinedValue;
                }
                else
                {
                    var propertyName = propertyChangeInfo.Value;

                    var ifcPropertySet = entity.PropertySets.FirstOrDefault(x => x.HasProperties.Any(y => y.Name == propertyName));

                    if (ifcPropertySet == null)
                    {
                        value = NotDefinedValue;
                    }
                    else
                    {
                        var singleValue = entity.GetPropertySingleValue(ifcPropertySet.Name, propertyName);
                        value = singleValue?.NominalValue?.ToString() ?? NotDefinedValue;
                    }

                    EnsurePropertySetAndPropertyAndValue(model, entity, propertyChangeInfo.PropertySetName, propertyChangeInfo.PropertyName, value);
                }
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
    }
}
