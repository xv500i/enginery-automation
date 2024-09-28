using IfcLibrary.Domain;
using System;
using System.Collections.Generic;
using System.IO;
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
        private const string NotFoundValue = "NotFound";

        public event EventHandler<PatchProgress> PatchProgressUpdated;

        public void PatchFile(string originalPath, string patchedPath, IfcManipulations ifcManipulations)
        {
            var totalEntityChanges = ifcManipulations.EntityChanges.Count;
            var completedEntityChanges = 0;
            var totalCleanups = ifcManipulations.EntityChanges.Count;
            var completedCleanups = 0;
            var totalItems = totalCleanups + totalEntityChanges;

            var currentTmpPath = GetTempIfcFile();
            using (var model = IfcStore.Open(originalPath, null))
            {
                using (var transaction = model.BeginTransaction())
                {
                    foreach (var entityChangeInfo in ifcManipulations.EntityChanges)
                    {
                        ApplyEntityChangeInfo(model, entityChangeInfo);
                        completedEntityChanges++;
                        this.PatchProgressUpdated?.Invoke(this, new PatchProgress
                        {
                            PercentComplete = 100 * (double)(completedCleanups + completedEntityChanges) / totalItems,
                            Text = $"Actualizando propiedades en {completedEntityChanges} de {totalEntityChanges} entidades.",
                        });
                    }

                    transaction.Commit();
                }
                model.SaveAs(currentTmpPath);
            }

            using (var model = IfcStore.Open(currentTmpPath, null))
            {
                for (int i = 0; i < ifcManipulations.EntityChanges.Count; i++)
                {
                    var entityChangeInfo = ifcManipulations.EntityChanges[i];
                    using (var transaction = model.BeginTransaction())
                    {
                        try
                        {
                            Cleanup(model, entityChangeInfo, ifcManipulations.PropertySetCleanups);
                            transaction.Commit();
                        }
                        catch (NullReferenceException)
                        {
                            transaction.RollBack();
                        }
                    }
                    completedCleanups++;
                    this.PatchProgressUpdated?.Invoke(this, new PatchProgress
                    {
                        PercentComplete = 100 * (double)(completedCleanups + completedEntityChanges) / totalItems,
                        Text = $"Eliminando propiedades en {completedCleanups} de {totalCleanups} entidades.",
                    });
                }

                model.SaveAs(patchedPath);
            }

            File.Delete(currentTmpPath);
        }

        private static string GetTempIfcFile()
        {
            return Path.GetTempPath() + Guid.NewGuid().ToString() + ".ifc";
        }

        private void Cleanup(IfcStore model, EntityChange entityChangeInfo, List<PropertySetCleanup> propertySetCleanups)
        {
            var entity = model.Instances.OfType<IfcObject>().Where(x => x.GlobalId == entityChangeInfo.Identifier).FirstOrDefault();
            if (entity == null)
            {
                return;
            }

            IEnumerable<IfcRelDefinesByProperties> relations = entity.IsDefinedBy.OfType<IfcRelDefinesByProperties>();
            var propertySets = new List<IfcPropertySet>();
            foreach (IfcRelDefinesByProperties rel in relations)
            {
                IfcPropertySet pSet = rel.RelatingPropertyDefinition as IfcPropertySet;
                if (rel.RelatingPropertyDefinition is IfcPropertySet a)
                {
                    propertySets.Add(pSet);
                }
                
            }

            //var propertySets = entity.PropertySets.ToList();
            foreach (var propertySet in propertySets)
            {
                var missingProperties = propertySet.HasProperties.Count;
                foreach (var property in propertySet.HasProperties.ToList())
                {
                    if (propertySetCleanups.Any(x =>
                        x.PropertySetName == propertySet.Name
                        && x.PropertyNamesToKeep.Contains(property.Name)))
                    {

                    }
                    else
                    {
                        model.Delete(property);
                        missingProperties--;
                    }
                }

                if (missingProperties <= 0)
                {
                    model.Delete(propertySet);
                }
            }
        }

        private void ApplyEntityChangeInfo(IfcStore model, EntityChange entityChangeInfo)
        {
            var entity = model.Instances.OfType<IfcObject>().Where(x => x.GlobalId == entityChangeInfo.Identifier).FirstOrDefault();
            if (entity == null)
            {
                return;
            }

            foreach (var propertyChangeInfo in entityChangeInfo.PropertyChanges)
            {
                var value = propertyChangeInfo.Value;
                if (string.IsNullOrWhiteSpace(value) || value == NotDefinedValue)
                {
                    value = NotDefinedValue;
                }
                else
                {
                    var propertyName = propertyChangeInfo.Value;

                    var ifcPropertySet = entity.PropertySets.FirstOrDefault(x => x.HasProperties.Any(y => y.Name == propertyName));

                    if (ifcPropertySet == null)
                    {
                        value = NotFoundValue;
                    }
                    else
                    {
                        var singleValue = entity.GetPropertySingleValue(ifcPropertySet.Name, propertyName);
                        value = singleValue?.NominalValue?.ToString() ?? NotFoundValue;
                    }
                }

                EnsurePropertySetAndPropertyAndValue(model, entity, propertyChangeInfo.PropertySetName, propertyChangeInfo.PropertyName, value);
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
