using System;
using System.Collections.Generic;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;

namespace IfcLibrary.Ifc
{
    public class IfcReader
    {
        public List<string> GetPropertySetIds(string path)
        {
            var editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "You",
                ApplicationFullName = "Your app",
                ApplicationIdentifier = "Your app ID",
                ApplicationVersion = "4.0",
                //your user
                EditorsFamilyName = "Santini Aichel",
                EditorsGivenName = "Johann Blasius",
                EditorsOrganisationName = "Independent Architecture"
            };

            var ids = new List<string>();

            using (var model = IfcStore.Open(path, editor))
            {
                foreach (var instance in model.Instances.OfType<IfcPropertySet>())
                {
                    ids.Add(instance.GlobalId);
                }
            }

            return ids;
        }

        public void PatchFile(string originalPath, string patchedPath, IEnumerable<IFCUpdate> updates, IEnumerable<IFCAdd> adds)
        {
            var editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "You",
                ApplicationFullName = "Your app",
                ApplicationIdentifier = "Your app ID",
                ApplicationVersion = "4.0",
                //your user
                EditorsFamilyName = "Santini Aichel",
                EditorsGivenName = "Johann Blasius",
                EditorsOrganisationName = "Independent Architecture"
            };

            var ids = new List<string>();

            using (var model = IfcStore.Open(originalPath, editor))
            {
                var transaction = model.BeginTransaction();
                foreach(var update in updates)
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

                foreach (var add in adds)
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

        public object Load(string path)
        {
            var editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "You",
                ApplicationFullName = "Your app",
                ApplicationIdentifier = "Your app ID",
                ApplicationVersion = "4.0",
                //your user
                EditorsFamilyName = "Santini Aichel",
                EditorsGivenName = "Johann Blasius",
                EditorsOrganisationName = "Independent Architecture"
            };

            using (var model = IfcStore.Open(path, editor))
            {
                foreach(var instance in model.Instances.OfType<IfcPropertySet>())
                {
                    Console.WriteLine($"hola");
                }
            }

            return null;
        }

        public void Modify(string path)
        {
            var editor = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "You",
                ApplicationFullName = "Your app",
                ApplicationIdentifier = "Your app ID",
                ApplicationVersion = "4.0",
                //your user
                EditorsFamilyName = "Santini Aichel",
                EditorsGivenName = "Johann Blasius",
                EditorsOrganisationName = "Independent Architecture"
            };

            using (var model = IfcStore.Open(path, editor))
            {
                using (var txn = model.BeginTransaction("Quick start transaction"))
                {
                    //get all walls in the model
                    var walls = model.Instances.OfType<IIfcWall>();

                    //iterate over all the walls and change them
                    foreach (var wall in walls)
                    {
                        wall.Name = "Iterated wall: " + wall.Name;
                    }

                    //commit your changes
                    txn.Commit();
                }

                model.SaveAs("SampleHouse_Modified.ifc");
            }
        }
    }
}
