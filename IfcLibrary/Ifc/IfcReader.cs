using System;
using System.IO;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace IfcLibrary.Ifc
{
    public class IfcReader
    {
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
                foreach(var instance in model.Instances)
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
