using System.IO;

namespace IfcLibrary.Ifc
{
    public class IfcReader
    {
        public string[] Load(string path)
        {
            return File.ReadAllLines(path);
        }
    }
}
